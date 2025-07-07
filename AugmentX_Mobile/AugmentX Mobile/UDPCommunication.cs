using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;

namespace AugmentX_Mobile
{
    /// <summary>
    /// 
    /// </summary>
    static internal class AugmentXPartner
    {
        private static readonly UdpClient udpClient = new();
        private static IPEndPoint endPoint = new(IPAddress.Any, 0);

        private static bool isListening = false;

        private const int defaultTimeout = 5000;

        public static void SetEndPoint(string ipAdress, string connectionPort)
        {
            endPoint.Address = IPAddress.Parse(ipAdress);
            endPoint.Port = Convert.ToInt32(connectionPort);
        }

        public static (bool Success, Exception? Error) Pair(int timeOut = defaultTimeout)
        {
            bool success = false;
            Exception? error = null;
            try
            {
                udpClient.Client.SendTimeout = timeOut;
                udpClient.Client.ReceiveTimeout = timeOut;

                udpClient.Connect(endPoint);

                udpClient.Send([1, 0, 1, 0, 1, 0, 1, 1], 8);

                if (udpClient.Receive(ref endPoint).SequenceEqual(new byte[] { 1, 1, 0, 1, 0, 1, 0, 1 }))
                    success = true;
            }
            catch (SocketException socEx)
            { error = socEx; }
            catch (Exception ex)
            { error = ex; }

            return (success, error); 
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

        public static event Action<Commands, string, IPEndPoint>? MessageReceived;
        public static async Task StartListening()
        {
            isListening = true;

            while (isListening)
            {
                try
                {
                    UdpReceiveResult message = await udpClient.ReceiveAsync();
                    MessageReceived?.Invoke((Commands)message.Buffer[0], Encoding.ASCII.GetString(message.Buffer,1,message.Buffer.Length - 1), message.RemoteEndPoint);
                }
                catch (Exception ex) 
                {
                    MessageReceived?.Invoke(Commands.Error, ex.Message, new(IPAddress.Any, 0));
                }
                
            }

        }
        public static void StopListening() { isListening = false; }

        public static (bool Success, Exception? Error) Send(Commands command, byte state = 1, string content = "")
        {
            bool success = false;
            Exception? error = null;
            try
            {
                udpClient.Send([(byte)command, state], 2);
                success = true;
            }
            catch (SocketException socEx)
            { error = socEx; }
            catch (Exception ex)
            { error = ex; }

            return (success, error);
        }

    }
}
