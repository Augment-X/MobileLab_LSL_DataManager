using System;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using System.Globalization;
using System.Timers;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.Interpolation;
using Libfmax;
using LSL;

namespace DataManager
{
    public delegate void DataInitEventHandler(object sender, int index, bool blocking);
    public delegate void NewDataEventHandler(object sender, int index);

    public class DataStream : StreamMeta
    {
        #region constants

        public const int MAX_BUFFER = 3;   // 3 seconds of buffer  //TODO: get LSL MaxBuffer value from Stream / Common class or config file?
        private const int DATA_SIZE = 10000;

        #endregion


        #region variables

        [JsonIgnore]
        public int RecordedBytes { get { return dataCount * (NbChannel + 1) * sizeof(double); } } // assuming here data size is same with double type...
        [JsonIgnore]
        public StreamInfo StreamInfo { get { return inlet.info(); } }
        public event InfoMessageDelegate InfoMessageReceived;
        public event DataInitEventHandler DataInitialized;
        public event NewDataEventHandler NewDataReceived;
        [JsonIgnore]
        public bool IsActive
        {
            get
            {
                return !dataReceiveTask.IsCompleted &&
                                           !dataReceiveTask.IsCanceled &&
                                           !dataReceiveTask.IsFaulted &&
                                           !inlet.IsClosed &&
                                           !inlet.IsInvalid;
            }
        }
        [JsonIgnore]
        public double[] Timestamps { get { return timestamps; } }
        [JsonIgnore]
        public double[][] Data { get { return data; } }
        [JsonIgnore]
        public string[][] MarkerData { get { return markerdata; } }
        private StreamInlet inlet;
        private int chunkLen;
        private double[] timestamps;
        private double[][] data;
        private string[][] markerdata;
        private double[] resTimestamps; //resampled
        private double[][] resData; //resampled
        private int resIndex;
        private int dataSize = DATA_SIZE;
        private int dataCount;
        private Task dataReceiveTask;
        private CancellationToken token;
        private readonly Object lockObj = new Object();
        private DateTime recordStartTime;

        #endregion

        public DataStream(StreamInfo streamInfo, CancellationToken token)
        {
            this.token = token;

            Name = streamInfo.name();
            Type = streamInfo.type();
            ChFormat = (ChannelFormat)streamInfo.channel_format();
            NbChannel = streamInfo.channel_count();
            SRate = streamInfo.nominal_srate();
            Channels = new List<ChannelInfo>(NbChannel);

            /* ❗ Important: if sRate is zero from the outlet and its chunkLen is larger than 1,
            then there is an 'gotcha' if timestamps are not defined and sent explicitly for the outlet. 
            The 'gotcha' is the data in the chunk will not be timestamped automatically in accordance with sample rate 
            which makes sense since sample rate is zero! */
            chunkLen = (int)Math.Floor(SRate / 100) + 1;  // chunkLen is increased accordingly when freq > 100 Hz

            // open inlet temporarily only to get its meta-data. Reason to do it this way: 
            // StreamInfo resulted from resolve_streams does not contain extended xml nodes! 
            inlet = new StreamInlet(streamInfo);

            Console.WriteLine(inlet.info().as_xml().ToString());

            try
            {
                XMLElement xmlChannels = inlet.info().desc().child("channels");
                XMLElement xmlChannel = xmlChannels.first_child();
                for (int i = 0; i < NbChannel; i++)
                {
                    Channels.Add(new ChannelInfo()
                    {
                        Label = xmlChannel.child_value("label"),
                        Unit = xmlChannel.child_value("unit"),
                        Precision = Int32.Parse(xmlChannel.child_value("precision"))
                    });
                    xmlChannel = xmlChannel.next_sibling();
                }
            }
            catch
            {
                Console.WriteLine($"{Name}:{Type}, " + "Lsl stream channel meta-data could not be retrieved");

                //TODO: below default values can be loaded up from the config file.. 
                for (int i = 0; i < NbChannel; i++)
                {
                    int precision = 0;
                    if (ChFormat == ChannelFormat.Double64 || ChFormat == ChannelFormat.Float32) precision = 1; 
                    Channels.Add(new ChannelInfo()
                    {
                        Label = "Ch" + i.ToString(),
                        Unit = "",
                        Precision = precision
                    });                    
                }
            };

            try
            {
                XMLElement xmlHardware = inlet.info().desc().child("hardware");
                Hardware = new HardwareInfo()
                {
                    Manufacturer = xmlHardware.child_value("manufacturer"),
                    Model = xmlHardware.child_value("model"),
                    Serial = xmlHardware.child_value("serial"),
                    Config = xmlHardware.child_value("config"),
                    Location = xmlHardware.child_value("location")
                };
            }
            catch
            {
                Console.WriteLine($"{Name}:{Type}, " + "Lsl stream hardware meta-data could not be retrieved");
                Hardware = new HardwareInfo();
            };

            try
            {
                XMLElement xmlSync = inlet.info().desc().child("synchronization");

                Sync = new SyncInfo()
                {
                    TimeSource = (TimeSource)Enum.Parse(typeof(TimeSource),
                            xmlSync.child_value("time_source")),
                    OffsetMean = Double.Parse(xmlSync.child_value("offset_mean")),
                    CanDropSamples = Boolean.Parse(xmlSync.child_value("can_drop_samples")),
                    Ipo = (InletProcessingOptions)Enum.Parse(typeof(InletProcessingOptions),
                            xmlSync.child_value("inlet_processing_options")),
                    Opo = (OutletProcessingOptions)Enum.Parse(typeof(OutletProcessingOptions),
                            xmlSync.child_value("outlet_processing_options")),
                    DriftCoeff = Double.Parse(xmlSync.child_value("outlet_drift_coeffificent")),
                    JitterCoeff = Double.Parse(xmlSync.child_value("outlet_jitter_coeffificent"))
                };
            }
            catch
            {
                Console.WriteLine($"{Name}:{Type}, " + "Lsl stream synchronization meta-data could not be retrieved");

                //TODO: below default values can be loaded up from the config file.. 
                Sync = new SyncInfo()
                {
                    TimeSource = TimeSource.Mod0,
                    OffsetMean = 0,
                    CanDropSamples = true,
                    Ipo = InletProcessingOptions.Clocksync,
                    Opo = OutletProcessingOptions.None,
                    DriftCoeff = 0.0,
                    JitterCoeff = 0.0
                };
            };

            inlet.close_stream();

            inlet = new StreamInlet(streamInfo, max_buflen: MAX_BUFFER, max_chunklen: chunkLen,
                            postproc_flags: (processing_options_t)Sync.Ipo);

            InitData();

            dataReceiveTask = Task.Run(async () => await DataReceiver(this.token), this.token);

        }

        ~DataStream()
        {
            inlet?.Dispose();
        }

        protected virtual void InfoMessage(Info info)
        {
            InfoMessageReceived?.Invoke(this, info);
        }

        protected virtual void OnDataInitialize(int index, bool blocking)
        {
            DataInitialized?.Invoke(this, index, blocking);
        }

        protected virtual void OnNewData(int index)
        {
            NewDataReceived?.Invoke(this, index);
        }

        public void InitData()
        {
            lock (lockObj)
            {
                Thread.Sleep(10);  //this is to ensure NewDataReceived of the plotter is finalized (it has BeginInvoke in it)
                dataSize = DATA_SIZE;
                timestamps = new double[dataSize];
                data = new double[NbChannel][];
                markerdata = new string[NbChannel][];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = new double[dataSize];
                    markerdata[i] = new string[dataSize];
                }

                resTimestamps = new double[0];
                resData = new double[NbChannel][];
                //markerdata = new string[NbChannel][];  //TODO: include markers in resampling?...
                for (int i = 0; i < data.Length; i++)
                {
                    resData[i] = new double[0];
                }

                recordStartTime = DateTime.Now;
                dataCount = 0;
                resIndex = 0;

                TriggerDataInit();
            }
        }

        public void TriggerDataInit()
        {
            OnDataInitialize(0, true);
        }

        private async Task DataReceiver(CancellationToken token)
        {
            string[,] sampleChunk = new string[chunkLen, NbChannel]; // read sample chunk
            double[] timestampChunk = new double[chunkLen];
           
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var num = inlet.pull_chunk(sampleChunk, timestampChunk, timeout: LSL.LSL.FOREVER);
                    if (num > 0)
                    {
                        lock (lockObj)
                        {
                            for (int i = 0; i < num; i++)
                            {
                                timestamps[dataCount] = timestampChunk[i] - Sync.OffsetMean; //  previously, before introducing processsing flags, this was: + inlet.time_correction();  

                                var infoMsg = Streamer.ConvertLSL2DT(timestamps[dataCount]).ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
                                for (int j = 0; j < sampleChunk.GetLength(1); j++)
                                {
                                    if (ChFormat != ChannelFormat.String && Double.TryParse(sampleChunk[i, j], NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                                    {
                                        if (Double.IsNaN(result))
                                        {
                                            result = 0;
                                        }
                                        data[j][dataCount] = result;
                                    }
                                    else
                                    {
                                        markerdata[j][dataCount] = sampleChunk[i, j];
                                    }
                                    infoMsg += $",{sampleChunk[i, j]}";
                                }

                                InfoMessage(new Info($"{Name}:{Type}, " + infoMsg, Info.Mode.Data));
                                OnNewData(dataCount);

                                if (dataCount < int.MaxValue)
                                {
                                    dataCount++;
                                }
                                else
                                {
                                    InfoMessage(new Info($"{Name}:{Type}, " + "Maximum data size is exceeded, stopping.", Info.Mode.CriticalEvent));
                                    throw new Exception("Maximum data size is exceeded");
                                }

                                if (dataCount >= dataSize)
                                {
                                    dataSize = (dataSize >= int.MaxValue / 2) ? int.MaxValue : 2 * dataSize;
                                    Array.Resize<double>(ref timestamps, dataSize);
                                    for (int j = 0; j < data.Length; j++)
                                    {
                                        Array.Resize<double>(ref data[j], dataSize);
                                        Array.Resize<string>(ref markerdata[j], dataSize);
                                    }

                                    OnDataInitialize(dataCount - 1, true);
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                InfoMessage(new Info($"{Name}:{Type}, " + "An exception occurred in the data receive loop", Info.Mode.CriticalError));
            }
            await Task.Delay(0);  //to prevent warning for async function with no await operator
        }

        public void ResampleData(int resFreq)
        {
            try
            {
                var dataCountMem = dataCount;
                var interpolWindow = dataCountMem - resIndex;

                if (interpolWindow >= 5)
                {
                    var appendIndex = resTimestamps.Length;
                    while (appendIndex > 0 && resTimestamps[appendIndex - 1] > timestamps[resIndex])
                    {
                        appendIndex--;
                    }

                    var appendTimestamp = timestamps[resIndex];
                    if (appendIndex > 0 && appendIndex < resTimestamps.Length)
                    {
                        appendTimestamp = resTimestamps[appendIndex];
                    }

                    var tarr = new double[interpolWindow];
                    Array.Copy(timestamps, resIndex, tarr, 0, interpolWindow);

                    var roiTs = Enumerable.Range(0, (int)(resFreq * ((tarr[tarr.Length - 1] - appendTimestamp))) + 1)
                                                      .Select(p => p / (double)resFreq + appendTimestamp).ToArray();
                    var roiData = new double[data.Length][];
                    LinearSpline[] roiDataLinSpline = new LinearSpline[data.Length];
                    for (int i = 0; i < data.Length; i++)
                    {
                        var darr = new double[interpolWindow];
                        Array.Copy(data[i], resIndex, darr, 0, interpolWindow);
                        roiDataLinSpline[i] = LinearSpline.Interpolate(tarr, darr);
                        roiData[i] = roiTs.Select(p => roiDataLinSpline[i].Interpolate(p)).ToArray();
                    }

                    // TEST:  overflow (Int32.MaxValue)
                    var resize = resTimestamps.Length + (appendIndex - resTimestamps.Length) + roiTs.Length;

                    Array.Resize<double>(ref resTimestamps, resize);
                    Array.Copy(roiTs, 0, resTimestamps, appendIndex, roiTs.Length);
                    for (int i = 0; i < data.Length; i++)
                    {
                        Array.Resize<double>(ref resData[i], resize);
                        Array.Copy(roiData[i], 0, resData[i], appendIndex, roiData[i].Length);
                    }

                    resIndex = dataCountMem - 1;
                }
            }
            catch (Exception e)
            { }
        }

        public string GetRecordData()
        {
            StringBuilder csv = new StringBuilder();

            csv.AppendLine("Name: " + Name);
            csv.AppendLine("Type: " + Type);
            csv.AppendLine("Channels: " + NbChannel);
            csv.AppendLine("SRate: " + SRate + "Hz");
            csv.AppendLine("Start time: " + recordStartTime.ToString("dd-MMM-yyyy HH:mm:ss.fff"));
            csv.AppendLine("End time: " + DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.fff") + "\n");

            Console.WriteLine(Name);

            string csvTitle = "Time[HH:mm:ss.fff]";

            for (int k = 0; k < NbChannel; k++)
            {
                var label = (Channels[k].Label == "") ? $"Ch{k + 1}" : $"{Channels[k].Label}";
                csvTitle += $",{label}[{Channels[k].Unit}]";
            }
            csv.AppendLine(csvTitle);

            for (int i = 0; i < dataCount; i++)
            {
                var s = Streamer.ConvertLSL2DT(timestamps[i]).ToString("HH:mm:ss.fff");
                for (int j = 0; j < NbChannel; j++)
                {
                    s += (ChFormat != ChannelFormat.String) ? $",{Convert.ToString(data[j][i], CultureInfo.InvariantCulture)}" : $",{markerdata[j][i]}";
                    Console.WriteLine(s.ToString());
                }
                csv.AppendLine(s);
            }

            return csv.ToString();
        }

        public string GetRecordMeta()
        {
            var options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };
            string jsonString = JsonSerializer.Serialize(this, options);
            return jsonString;
        }
    }

}