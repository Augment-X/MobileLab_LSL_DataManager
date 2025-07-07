using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataManager
{
    public static class MobileCommunication
    {
        public static event EventHandler<MobileCommunicationDataEventArgs> DataPacketReceived;

        public static int Port { get; set; } = 1100;
        public static bool Recording { get; set; } = false;
        public static bool ThreadRunning { get; set; } = false;

        public static List<(DateTime, string)> Steps = new();

        static Thread MobileDataThread;
        static IPEndPoint sender = new(IPAddress.Any, 0);
        private static readonly IPEndPoint ipep = new(IPAddress.Any, Port);
        private static readonly UdpClient  newsock = new(ipep);


        public static void StartDataReception()
        {
            MobileDataThread ??= new Thread(new ThreadStart(UDPThread));
            if (!MobileDataThread.IsAlive) MobileDataThread.Start();
        }

        public static void StopDataReception()
        {
            if (MobileDataThread.IsAlive)
            { ThreadRunning = false; newsock.Close(); }
        }

        public static void ChangePort(int port) { ipep.Port = port; }

        static void UDPThread()
        {
            byte[] data;
            ThreadRunning = true;

            newsock.Client.SendBufferSize = 65535; // 64 KB

            while (ThreadRunning)
            {
                try
                {
                    data = newsock.Receive(ref sender);

                    if (data.Length == 8 && data.SequenceEqual(new byte[] { 1, 0, 1, 0, 1, 0, 1, 1 })) { newsock.Send(new byte[] { 1, 1, 0, 1, 0, 1, 0, 1 }, 8, sender); }
                    else
                    {
                        string message = Encoding.ASCII.GetString(data, 1, data.Length - 1);
                        var e = new MobileCommunicationDataEventArgs() { MessageID = (Commands)data[0], MessageContent = message };
                        DataPacketReceived?.Invoke(null, e);
                    }
                }
                catch (Exception) { }
            }
        }

        public enum Commands
        {
            ACK = 'K',
            Red = 'R',
            Yellow = 'Y',
            Green = 'G',
            Blue = 'B',
            Record = 'X',
            List = 'L',
            DataPackets = 'D',
            Subscribe = 'S',
            NameTags = 'N',
            Error = 'E'
        }

        public static async Task Send(byte[] data)
        {
            if(data.Length >= 64000) { data = Encoding.ASCII.GetBytes("File too large"); }

            try { await newsock.SendAsync(data, data.Length, sender); } catch { }
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            string resp = string.Empty;
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    resp = ip.ToString();
                }
            }
            return resp;
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }

    public class MobileCommunicationDataEventArgs : EventArgs
    {
        public MobileCommunication.Commands MessageID { get; set; }
        public string MessageContent { get; set; }
    }
}
