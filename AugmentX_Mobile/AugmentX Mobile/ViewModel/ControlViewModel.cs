using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Net;

namespace AugmentX_Mobile.ViewModel
{
    [QueryProperty(nameof(Text), "id")]
    //[QueryProperty(nameof(Client), "Client")]
    //[QueryProperty(nameof(EP), "IP_EndPoint")]
    public partial class ControlViewModel : ObservableObject
    {

        [ObservableProperty]
        ObservableCollection<string> items = [];

        [ObservableProperty]
        ObservableCollection<ISeries> series = [];

        [ObservableProperty]
        ICartesianAxis[] xAxes =
            [
                new Axis
                {
                    Name = "last 5s",
                    NameTextSize = 10,
                    CustomSeparators = [0, 60, 120, 180, 240, 300],
                    MinLimit = 0,
                    MaxLimit = 300,
                    SeparatorsPaint = new SolidColorPaint(SKColors.Black.WithAlpha(100)),
                    LabelsPaint = new SolidColorPaint(SKColors.Black.WithAlpha(0))
                }
            ];

        [ObservableProperty]
        ICartesianAxis[] nullAxes =
            [
                new Axis
                {
                    LabelsPaint = null
                }
            ];

        public static bool IsRecording { get; set; } = false;

        private readonly ObservableCollection<ObservablePoint>[] ObservableValues = new ObservableCollection<ObservablePoint>[5];

        public ControlViewModel()
        {
            for (int i = 0; i<ObservableValues.Length; i++)
            {
                ObservableValues[i] = [];
                Series.Add(
                    new LineSeries<ObservablePoint>(ObservableValues[i])
                    {
                        Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 2 },
                        Fill = null,
                        GeometryFill = null,
                        GeometryStroke = null//new SolidColorPaint(SKColors.Blue) { StrokeThickness = 4 }
                    }
                    );

                ((LineSeries<ObservablePoint>)Series[0]).Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 2 };
            }
        }

        [ObservableProperty]
        bool isToggled = true;

        [ObservableProperty]
        string text = string.Empty;

        [ObservableProperty]
        string recordName = "RECORD";

        [ObservableProperty]
        string ipaddress = "something something";

        [RelayCommand]
        async Task Tap()
        {
            //Text = string.Empty;
            if (RecordName == "RECORD")
            {
                RecordName = "STOP";
                AugmentXPartner.MessageReceived += AugmentXPartner_MessageReceived;
            }
            else
            {
                RecordName = "RECORD";
                AugmentXPartner.MessageReceived -= AugmentXPartner_MessageReceived;
            }

            var (Success, Error) = AugmentXPartner.Send(AugmentXPartner.Commands.Record, 1);

            if (Success)
            {
                //Text = "Command correct";
            }
            else if (Error != null)
            {
                string MsgTitle = Error.GetType().Name;
                Text = Error.Message;
                await Shell.Current.DisplayAlert(MsgTitle, Text, "Ok");
            }
        }

        private void AugmentXPartner_MessageReceived(AugmentXPartner.Commands com, string arg1, IPEndPoint arg2)
        {
            if (com == AugmentXPartner.Commands.DataPackets && arg1 != null)
            {
                string[] aux = arg1.Split('\n');
                string ID = aux[0];

                string[] values = aux[1].Split(',');

                int i = 0;
                foreach (var item in SelectionService.Instance.AllSelected)
                {
                    if (item.Father == ID && long.TryParse(values[0], out long value0) && double.TryParse(values[item.Index], out double value))
                    {
                        Series[i].Name = item.Father + ": " + item.Name;
                        TimeSpan timestamp = new(value0 / 100);
                        ObservableValues[i].Add(new() { Y = value, X = timestamp.TotalMilliseconds });
                        if (ObservableValues[i].Count > 300) { ObservableValues[i].RemoveAt(0); }
                        i++;
                    }
                }

                //if (long.TryParse(values[0], out long value0) && double.TryParse(values[1], out double value1) && double.TryParse(values[2], out double value2) && double.TryParse(values[3], out double value3) && double.TryParse(values[4], out double value4) && double.TryParse(values[5], out double value5))
                //{
                //    TimeSpan timestamp = new(value0 / 100);

                //    ObservableValues1.Add(new() { Y = value1, X = timestamp.TotalMilliseconds } );
                //    ObservableValues2.Add(new() { Y = value2, X = timestamp.TotalMilliseconds } );
                //    ObservableValues3.Add(new() { Y = value3, X = timestamp.TotalMilliseconds } );
                //    ObservableValues4.Add(new() { Y = value4, X = timestamp.TotalMilliseconds } );
                //    ObservableValues5.Add(new() { Y = value5, X = timestamp.TotalMilliseconds } );

                //    if (ObservableValues5.Count > 300)
                //    {
                //        ObservableValues1.RemoveAt(0);
                //        ObservableValues2.RemoveAt(0);
                //        ObservableValues3.RemoveAt(0);
                //        ObservableValues4.RemoveAt(0);
                //        ObservableValues5.RemoveAt(0);
                //    }
                //}
                //Text = arg1;
                
            }
        }

        [RelayCommand]
        async Task Press(string? buttonText)
        {
            AugmentXPartner.Commands com = AugmentXPartner.Commands.Blue;

            switch(buttonText)
            {
                case "B": com = AugmentXPartner.Commands.Blue; break;
                case "G": com = AugmentXPartner.Commands.Green; break;
                case "Y": com = AugmentXPartner.Commands.Yellow; break;
                case "R": com = AugmentXPartner.Commands.Red; IsToggled = !IsToggled; break;
            }
            AugmentXPartner.Send(com, 1);
        }

        [RelayCommand]
        async Task GoDetail(string s)
        {
            await Shell.Current.GoToAsync($"{nameof(DetailPage)}?id={s}",
                    new Dictionary<string, object>
                    {
                    {nameof(DetailPage), new object()}
                    }
                    );
        }

        [RelayCommand]
        async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}