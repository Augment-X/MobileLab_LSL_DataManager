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

namespace DataManager
{
    public partial class MainForm : Form
    {
        private class DataStreamConfigurator
        {
            public class RecordMeta
            {
                public string DataPath { get; set; } = ""; ///MOBILE_CHANGES - REDUCED //public string Project { get; set; } = ""; . . .
            }

            private const int PLOT_TIME_WINDOW = 5000;    // ms
            private const int PLOT_REFRESH_RATE = 30;   // ms

            public string Name { get; set; }
            public string DataPath { get; set; }
            public string UdpStreamIp { get; set; }
            public int UdpStreamPort { get; set; }
            [JsonIgnore]
            public bool UdpStreamEnable { get; set; } = true; ///MOBILE_CHANGES // = false;
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


            public DataStreamConfigurator()
            {
                Streams = new List<DataStream>();
                plotterDict = new Dictionary<DataStream, FormPlotter>();
                Record = new RecordMeta();
            }

            ~DataStreamConfigurator()
            {   }

            protected virtual void InfoMessage(Info info)
            {
                InfoMessageReceived?.Invoke(this, info);
            }

            private void SubInfoMessageReceived(object sender, Info info)
            {
                InfoMessage(info);
            }

            /// <summary>
            /// Refurbished method to handle data received from an LSL Client,
            /// formate it into the desired package and send it via UDP to the Mobile companion
            /// </summary> MOBILE_CHANGES
            public async void NewDataReceived(object sender, int index)
            {
                if (UdpStreamEnable) /// Only if UDP data is set to be shared
                {
                    var stream = (DataStream)sender;
                    byte[] udpbytes;
                    string udpstring;

                    /// Get the Name and Type as title, add a separator (\n)
                    udpstring = stream.Name + " " + stream.Type + "\n";

                    if (!MobileCommunication.Recording) /// In case the App is not recording
                    {
                        /// Add all channels by name into a string (to be used as a list of names)
                        for (int i = 0; i < stream.Data.Length; i++)
                        {
                            /// If channel has no name, add a ch+(number) label in place of the name
                            var channelLabel = stream.Channels[i].Label == "" ? $"ch{i + 1}" : stream.Channels[i].Label;
                            udpstring += $"{channelLabel}";
                            /// Add a separator
                            if (i < stream.Data.Length - 1) udpstring += ",";
                        }

                        /// Add an L (Type:ListOfChannels) at the beginning if the packet so that the Mobile partner can catalog it on reception
                        udpbytes = ASCIIEncoding.ASCII.GetBytes("L" + udpstring);
                    }
                    else /// In case the App is recording
                    {
                        /// Add a timestamp to the data
                        udpstring += $"{(ulong)(Streamer.ConvertLSL2UnixEpoch(stream.Timestamps[index]) * 1.0E09)}";//converted to ns
                        //udpstring += $"{stream.Timestamps[index]}";
                        /// Add all channels' current received value (to be used as data for graphics) with a separator
                        for (int i = 0; i < stream.Data.Length; i++)
                        {
                            var dataArr = stream.Data[i];
                            udpstring += $",{dataArr[index]}";
                        }

                        /// Add a D (Type:DataPacket) at the beginning if the packet so that the Mobile partner can catalog it on reception
                        udpbytes = ASCIIEncoding.ASCII.GetBytes("D" + udpstring);
                    }

                    /// Send the packet to the mobile partner
                    await MobileCommunication.Send(udpbytes);
                }
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
            ///  Clear the Streams 
            /// </summary> MOBILE_CHANGES
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
                var directory = $"{Record.DataPath}"; ///MOBILE_CHANGES - REDUCED //var directory = $"{Record.DataPath}\\{Record.Project}\\{Record.Experiment}\\{Record.Session}\\{Record.Subject}";
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                string filenames = "";
                foreach (var stream in Streams)
                {
                    if (stream.RecordedBytes > 0)
                    {
                        
                        var filename = $"{stream.Name}_{ stream.Type}"; ///MOBILE_CHANGES - REDUCED //var subject = (Record.Subject != "") ? $"_{Record.Subject}" : ""; // var filename = $"{Record.Session}{subject}_{stream.Name}_{stream.Type}";
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

            /// <summary>
            /// Saves a table with all button presses made on the mobile partner and shared with this application
            /// while recording for event synchronization.  
            /// </summary> MOBILE_CHANGES
            /// <param name="steps"> List of button presses made on the mobile partner</param>
            public void SaveSteps(List<(DateTime, string)> steps)
            {
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