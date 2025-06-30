using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Runtime.Serialization;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using System.ComponentModel.Design;
using System.ComponentModel;
using static System.Net.Mime.MediaTypeNames;

namespace AugmentX_Mobile.ViewModel
{
    //[QueryProperty(nameof(Text), "id")]
    public partial class SensorListViewModel : ObservableObject
    {
        [ObservableProperty]
        public ObservableCollection<SensorItem> sensors;

        [ObservableProperty]
        private SensorItem? selectedSensor;

        [RelayCommand]
        private async Task SelectionChanged()
        {
            string s = "someString";
            if (SelectedSensor is not null)
                await Shell.Current.GoToAsync($"{nameof(ParameterListPage)}?nameDisplay={s}",
                   new Dictionary<string, object>
                   {
                   { "parameters", SelectedSensor.Parameters },
                   }
                   );
        }

        public SensorListViewModel()
        {
            Shell.Current.Navigating += OnShellNavigating;
            Shell.Current.Navigated += OnShellNavigated;

            AugmentXPartner.MessageReceived += AugmentXPartner_MessageReceived;
            AugmentXPartner.StartListening();

            sensors = [];
        }

        private void AugmentXPartner_MessageReceived(AugmentXPartner.Commands com, string arg1, IPEndPoint arg2)
        {
            if (com == AugmentXPartner.Commands.Subscribe && arg1 != null)
            {
                if (arg1 == "reset") { sensors.Clear(); }
                else
                {
                    string[] values = arg1.Split(',');

                    sensors.Add(new SensorItem()
                    { 
                        Id = values[0] + values[1],
                        DisplayName = values[0] + values[1],
                    });
                }
            }
            else if (com == AugmentXPartner.Commands.List && arg1 != null)
            {
                string[] v = arg1.Split('\n');

                var foundSensor = sensors.FirstOrDefault(s => s.Id == v[0]);


                if (foundSensor != null)
                {
                    string[] parameters = v[1].Split(",");
                    foreach (string param in parameters)
                    {
                        if (foundSensor.Parameters.FirstOrDefault(s => s.Name == param) == null)
                            foundSensor.Parameters.Add(new ParameterItem(){ Name = param, Father = v[0], Index = foundSensor.Parameters.Count });
                    }
                }
                else
                {
                    Console.WriteLine("Sensor not found.");
                }
            }
        }

        [RelayCommand]
        async Task Tap()
        {
            await Shell.Current.GoToAsync(nameof(ControlPage));
        }

        private void OnShellNavigating(object sender, ShellNavigatingEventArgs e)
        {
            if (e.Source == ShellNavigationSource.Pop && ControlViewModel.IsRecording)
            {
                e.Cancel(); // Cancel the back navigation
                Shell.Current.DisplayAlert("Alert", "Cannot go back while recording", "Ok");
            }
        }

        private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
        {
            
            if (Shell.Current.CurrentPage?.BindingContext == this)
            {
                if (e.Source == ShellNavigationSource.Pop)
                {
                    Console.WriteLine("BackButton Pressed");
                    SelectedSensor = null;
                }
            }
        }
    }

    public partial class ParameterItem : ObservableObject
    {
        [ObservableProperty]
        public int index = 0;

        [ObservableProperty]
        public string father = string.Empty;

        [ObservableProperty]
        public string name = string.Empty;

        [ObservableProperty]
        bool isSelected = false;

        partial void OnIsSelectedChanged(bool value)
        {
            SelectionService.UpdateSelection(this, value);
        }
    }

    public partial class SensorItem : ObservableObject
    {
        [ObservableProperty]
        bool isSelected;

        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public ObservableCollection<ParameterItem> Parameters { get; set; } = [];
    }

    public partial class SelectionService : ObservableObject
    {
        [ObservableProperty]
        public ObservableCollection<ParameterItem> allSelected = [];

        [ObservableProperty]
        public int selectedCount = 0;

        private static SelectionService instance = new();
        public static SelectionService Instance { get => instance; }

        SelectionService()
        {
            instance = this;
        }

        [ObservableProperty]
        public int maxCount = 5;

        public static void UpdateSelection(ParameterItem item, bool isSelected)
        {
            if (isSelected)
            {
                if (Instance.AllSelected.Count >= Instance.MaxCount)
                {
                    item.IsSelected = false;
                    // TODO: alert user
                    return;
                }
                Instance.AllSelected.Add(item);
            }
            else
            {
                Instance.AllSelected.Remove(item);
            }
            Instance.SelectedCount = Instance.AllSelected.Count;
        }
    }


}