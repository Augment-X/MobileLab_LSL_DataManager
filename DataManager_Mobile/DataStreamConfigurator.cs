using System;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Diagnostics;
using Libfmax;
using LSL;
using System.Net;
using System.Net.Sockets;
using System.Xml.Linq;
using static Libfmax.StreamMeta;
using static System.Windows.Forms.DataFormats;
using System.Globalization;
using System.Threading.Channels;

namespace DataManager
{
    public partial class MainForm : Form
    {
        private class DataStreamConfigurator
        {
            public class RecordMeta
            {
                public string DataPath { get; set; } = "";
                public string Project { get; set; } = "";
                public string Experiment { get; set; } = "";
                public string Session { get; set; } = "";
                public string Subject { get; set; } = "";
            }

            private const int PLOT_TIME_WINDOW = 5000;    // ms
            private const int PLOT_REFRESH_RATE = 30;   // ms

            public string Name { get; set; }
            public string DataPath { get; set; }
            public string UdpStreamIp { get; set; }
            public int UdpStreamPort { get; set; }
            [JsonIgnore]
            public bool UdpStreamEnable { get; set; }
            [JsonIgnore]
            public RecordMeta Record { get; set; }
            [JsonIgnore]
            public List<DataStream> Streams { get; set; }
            [JsonIgnore]
            public int NbSubscribed { get { return Streams.Count; } }
            [JsonIgnore]
            public StreamInfo[] StreamInfos { get; set; }
            public delegate void InfoMessageDelegate(object sender, Info info);
            public event InfoMessageDelegate InfoMessageReceived;
            public double RecordedKBytes
            {
                get
                {
                    double recordedKBytes = 0;
                    foreach (var stream in Streams)
                    {
                        recordedKBytes += stream.RecordedBytes / 1024.0;
                    }
                    return recordedKBytes;
                }
            }
            private Dictionary<DataStream, FormPlotter> plotterDict;
            private UdpClient udpStreamClient;


            public DataStreamConfigurator()
            {
                Streams = new List<DataStream>();
                plotterDict = new Dictionary<DataStream, FormPlotter>();
                Record = new RecordMeta();
                udpStreamClient = new UdpClient();
            }

            ~DataStreamConfigurator()
            {
                // CHECK:what is needed here
                udpStreamClient.Close();
                udpStreamClient.Dispose();
            }

            protected virtual void InfoMessage(Info info)
            {
                InfoMessageReceived?.Invoke(this, info);
            }

            private void SubInfoMessageReceived(object sender, Info info)
            {
                InfoMessage(info);
            }

            
            public void NewDataReceived(object sender, int index)
            {
                if (UdpStreamEnable)
                {
                    var stream = (DataStream)sender;
                    var udpstring = "";
                    byte[] udpbytes;
                    if (!MobileCommunication.Recording)
                    {
                        udpstring = stream.Name + " " + stream.Type + "\n";
                        for (int i = 0; i < stream.Data.Length; i++)
                        {
                            var channelLabel = stream.Channels[i].Label == "" ? $"ch{i + 1}" : stream.Channels[i].Label;
                            udpstring += $"{channelLabel}";
                            if (i < stream.Data.Length - 1) udpstring += ",";
                        }

                        udpbytes = ASCIIEncoding.ASCII.GetBytes("L" + udpstring);
                        MobileCommunication.Send(udpbytes);
                    }
                    else
                    {
                        udpstring = stream.Name + " " + stream.Type + "\n";
                        udpstring += $"{(ulong)(Streamer.ConvertLSL2UnixEpoch(stream.Timestamps[index]) * 1.0E09)}";//converted to ns
                        for (int i = 0; i < stream.Data.Length; i++)
                        {
                            var dataArr = stream.Data[i]; 
                            udpstring += $",{dataArr[index]}";
                            //if (i < stream.Data.Length - 1) udpstring += ",";
                        }

                        udpbytes = ASCIIEncoding.ASCII.GetBytes("D" + udpstring);
                        MobileCommunication.Send(udpbytes);
                    }
                }
            }

            public void ConnectUdpStream()
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Parse(UdpStreamIp), UdpStreamPort); // endpoint where server is listening                
                if (udpStreamClient != null && udpStreamClient.Client != null && !udpStreamClient.Client.Connected)
                    udpStreamClient.Connect(ep);
            }

            /// <summary>
            /// MOBILE_CHANGES close UDPClient 
            /// </summary>
            public void DisconnectUdpStream()
            {
                if (udpStreamClient!= null && udpStreamClient.Client != null && udpStreamClient.Client.Connected)
                    udpStreamClient.Close();
            }

            public DataStream GetStream(StreamInfo streamInfo)
            {
                try
                {
                    foreach (var stream in Streams)
                    {
                        if (streamInfo.uid() == stream.StreamInfo.uid()) return stream;
                    }
                }
                catch (Exception e)
                {
                    InfoMessage(new Info($"Stream {streamInfo.name()}-{streamInfo.type()} could not be found", Info.Mode.Error));
                }

                return null;
            }

            public bool AddStream(StreamInfo streamInfo)
            {
                if (GetStream(streamInfo) == null)
                {
                    var stream = new DataStream(streamInfo, token);
                    stream.InfoMessageReceived += SubInfoMessageReceived;
                    stream.NewDataReceived += NewDataReceived;
                    Streams.Add(stream);
                    return true;
                }
                return false;
            }

            /// <summary>
            /// MOBILE_CHANGES Add a method to clear the Streams
            /// </summary>
            public void ClearStream()
            {
                Streams.Clear();
            }

            public void ShowPlotter(int index)
            {
                if (index >= 0)
                {
                    var stream = GetStream(StreamInfos[index]);

                    if (stream != null && (!plotterDict.ContainsKey(stream) || (plotterDict.ContainsKey(stream) && plotterDict[stream].IsDisposed)))
                    {
                        var plotter = new FormPlotter($"{stream.Name}, {stream.Type}, {stream.SRate}Hz, {stream.NbChannel} channels",
                                                            PLOT_TIME_WINDOW, PLOT_REFRESH_RATE, token);
                        plotter.Show();
                        plotter.WindowState = FormWindowState.Normal;
                        stream.DataInitialized += plotter.DataInitialized;
                        stream.NewDataReceived += plotter.NewDataReceived;
                        stream.TriggerDataInit();
                    }
                }
            }

            public void StartRecord()
            {
                var formPlotterRun = FormPlotter.Run;
                FormPlotter.Run = false;
                foreach (var stream in Streams)
                {
                    stream.InitData();
                }
                FormPlotter.Run = formPlotterRun;
            }

            public void SaveRecord()
            {
                //CHANGE
                //var directory = $"{Record.DataPath}\\{Record.Project}\\{Record.Experiment}\\{Record.Session}\\{Record.Subject}";
                var directory = $"{Record.DataPath}";
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                string filenames = "";
                foreach (var stream in Streams)
                {
                    if (stream.RecordedBytes > 0)
                    {
                        //CHANGE
                        //var subject = (Record.Subject != "") ? $"_{Record.Subject}" : "";
                        //var filename = $"{Record.Session}{subject}_{stream.Name}_{stream.Type}";
                        //filenames += filename + " ";


                        var filename = $"{stream.Name}_{ stream.Type}";
                        filenames += filename + " ";


                        File.WriteAllText(directory + @"\" + filename + ".csv", stream.GetRecordData());
                        File.WriteAllText(directory + @"\" + filename + ".json", stream.GetRecordMeta());
                    }
                }
                if (filenames != "")
                {
                    InfoMessage(new Info($"Log file(s) {filenames} successfully created.", Info.Mode.Event));
                    Process.Start("explorer.exe", directory);
                }
            }
            public void SaveSteps(List<(DateTime, string)> steps)
            {
                //CHANGE
                //var directory = $"{Record.DataPath}\\{Record.Project}\\{Record.Experiment}\\{Record.Session}\\{Record.Subject}";
                var directory = $"{Record.DataPath}";

                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                StringBuilder csv = new StringBuilder();

                foreach (var (t, s) in steps)
                {
                    csv.AppendLine(t.ToString("HH:mm:ss:FFFF") + "," + s);
                }

                var filename = $"Steps";
                File.WriteAllText(directory + @"\" + filename + ".csv", csv.ToString());
            }
        }
    }
}