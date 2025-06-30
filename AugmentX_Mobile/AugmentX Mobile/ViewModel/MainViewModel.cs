using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace AugmentX_Mobile.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        IConnectivity connectivity;
        public MainViewModel(IConnectivity connectivity)
        {
            Items = [];
            this.connectivity = connectivity;
        }

        [ObservableProperty]
        ObservableCollection<string> items;

        [ObservableProperty]
        bool loading = false;

        [ObservableProperty]
        string text = "Empty";

        [ObservableProperty]
        string ip_address = Preferences.Default.Get("IP_Address", "192.168.129.4");

        [ObservableProperty]
        string connection_port = Preferences.Default.Get("Connection_Port", "1100");

        [RelayCommand]
        async Task Add()
        {
            //if (string.IsNullOrEmpty(Text)) return;
            try
            {
                //UDPConnect();
            }
            catch { }

            if (connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Shell.Current.DisplayAlert("woah", "No Internet", "Ok");

                return;
            }


            //Items.Add(Text);
        }

        [RelayCommand]
        void Remove(string s)
        {
            if (Items.Contains(s))
                Items.Remove(s);
        }

        [RelayCommand]
        async Task Tap()
        {
            Preferences.Default.Set("IP_Address", Ip_address);
            Preferences.Default.Set("Connection_Port", Connection_port);

            AugmentXPartner.SetEndPoint(Ip_address, Connection_port);

            var (Success, Error) = AugmentXPartner.Pair();

            if (Success)
            {
                Text = "Connection Sucesfull";

                //string s = "Connected";
                //await Shell.Current.GoToAsync($"{nameof(ControlPage)}?id={s}"
                //    /*new Dictionary<string, object>
                //    {
                //    { "Client", null },
                //    { "IP_EndPoint", null }
                //    }*/
                //    );
                string s = "Connected";
                await Shell.Current.GoToAsync(nameof(SensorListPage));
                //await Shell.Current.GoToAsync(nameof(ControlPage));
            }
            else if(Error != null)
            {
                string MsgTitle = Error.GetType().Name;
                Text = Error.Message;
                await Shell.Current.DisplayAlert(MsgTitle, Text, "Ok");
            }
        }

        //[RelayCommand]
        //async Task Tap()
        //{
        //    Loading = true;
        //    string MsgTitle = string.Empty;

        //    Preferences.Default.Set("IP_Address", Ip_address);
        //    Preferences.Default.Set("Connection_Port", Connection_port);

        //    UdpClient client = new();
        //    IPEndPoint ep = new(IPAddress.Parse(Ip_address), Convert.ToInt32(Connection_port));

        //    try
        //    {
        //        client.Client.ReceiveTimeout = 5000;
        //        client.Connect(ep);

        //        // send data
        //        byte[] sendBytes = Encoding.ASCII.GetBytes("Initiate Connection");
        //        client.Send(sendBytes, sendBytes.Length);

        //        // then receive data
        //        byte[] receiveBytes = client.Receive(ref ep);
        //        string returnData = Encoding.ASCII.GetString(receiveBytes);

        //        Text = returnData.ToString();

        //        Loading = false;
        //    }
        //    catch (System.Net.Sockets.SocketException socEx)
        //    {
        //        Text = socEx.Message;
        //        MsgTitle = socEx.GetType().Name;
        //    }
        //    catch (Exception ex)
        //    {
        //        Text = ex.Message;
        //        MsgTitle = ex.GetType().Name;
        //    }

        //    if (Loading)
        //    { 
        //        Loading = false;
        //        await Shell.Current.DisplayAlert(MsgTitle, Text, "Ok");
        //    }
        //    else
        //    {
        //        string s = "Connected";
        //        await Shell.Current.GoToAsync($"{nameof(ControlPage)}?id={s}",
        //            new Dictionary<string, object>
        //            {
        //                { "Client", client },
        //                { "IP_EndPoint", ep }
        //            }
        //            );
        //    }
        //}
    }
}
