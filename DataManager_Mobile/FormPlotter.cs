using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;
using Libfmax;
using LSL;
using MathNet.Numerics.Statistics;
using System.Linq;
using ScottPlot.Plottable;

namespace DataManager
{

    public partial class FormPlotter : Form
    {
        public class SignalSet
        {
            public string Name { get; set; }
            public SignalPlotXY[] SignalPlot { get; set; }
            public double TimeWindow { get; set; }
            public int Index { get; set; }
            private int precision;

            public SignalSet(string name, SignalPlotXY[] signalPlot, double timeWindow, int index, int precision)
            {
                this.Name = name;
                this.SignalPlot = signalPlot;
                this.TimeWindow = timeWindow;
                this.Index = index;
                this.precision = precision;
                UpdateRenderWindow();
            }

            public double UpdateRenderWindow()
            {
                double maxTs = 0;

                try
                {
                    foreach (var sig in SignalPlot)
                    {
                        sig.MaxRenderIndex = Math.Max(Math.Min(Index, sig.Xs.Length - 1), 0);
                        var minIndex = sig.MaxRenderIndex;
                        if (sig.Xs[sig.MaxRenderIndex] > maxTs) maxTs = sig.Xs[sig.MaxRenderIndex];
                        while (minIndex > 0 && (sig.Xs[minIndex] > sig.Xs[sig.MaxRenderIndex] - TimeWindow))
                        {
                            minIndex--;
                        }
                        if (minIndex < sig.MaxRenderIndex) minIndex += 1;
                        sig.MinRenderIndex = minIndex;

                    }
                }
                catch (Exception e)
                { }

                return maxTs;
            }
        }

        #region variables

        public static bool Run = true;
        public double TimeWindow { get; set; }  //in seconds
        public event EventHandler TimerUpdated;
        private List<SignalSet> SignalSets { get; set; } = new List<SignalSet>();
        private Dictionary<int, ScottPlot.Renderable.Axis> axisDict = new System.Collections.Generic.Dictionary<int, ScottPlot.Renderable.Axis>();
        private readonly Object lockObj = new Object();
        private CancellationToken token;
        private bool disposing;
        private IAsyncResult newDataReceived;
        private string lbInfoText = "";
        private List<double> markerLines = new List<double>();

        #endregion

        public FormPlotter(string title, double timeWindow, int tickInterval, CancellationToken token)
        {
            InitializeComponent();

            this.Text = "Plotter: " + title;
            this.TimeWindow = timeWindow / 1000.0;
            this.token = token;

            formsPlot1.Reset();
            formsPlot1.Plot.Legend();
            formsPlot1.Plot.XLabel("Timestamp[s]");
            formsPlot1.Plot.XAxis.TickLabelFormat("F3", dateTimeFormat: false);
            formsPlot1.Plot.YAxis.TickLabelFormat("F1", dateTimeFormat: false);
            formsPlot1.Plot.SetCulture(System.Globalization.CultureInfo.InvariantCulture);

            WinApi.TimeBeginPeriod(1);
            timerUpdate.Interval = tickInterval;       //default interval in miliseconds
            timerUpdate.Enabled = true;

            lbStatus.Visible = !Run;

        }

        private void FormPlotter_FormClosing(object sender, EventArgs e)
        {
            WinApi.TimeEndPeriod(1);
            disposing = true;
            this.Dispose(true);
        }

        protected virtual void OnTimerUpdate(EventArgs e)
        {
            TimerUpdated?.Invoke(this, e);
        }

        public void DataInitialized(object sender, int index, bool blocking)
        {
            if (this.IsHandleCreated)
            {
                IAsyncResult result = this.BeginInvoke(new Action(() =>
                {
                    lock (lockObj)
                    {
                        var stream = (DataStream)sender;

                        var found = false;
                        foreach (var signalSet in SignalSets)
                        {
                            if (signalSet.Name == stream.Name + "-" + stream.Type)
                            {
                                found = true;

                                for (int i = 0; i < signalSet.SignalPlot.Length; i++)
                                {
                                    var signalPlot = signalSet.SignalPlot[i];
                                    signalPlot.Xs = stream.Timestamps;
                                    signalPlot.Ys = stream.Data[i];
                                }
                                signalSet.Index = index;

                                break;
                            }
                        }

                        if (!found) NewSignalSet(stream.Name + "-" + stream.Type, stream.Timestamps, stream.Data, index, stream.Channels[0].Precision);
                    }
                }));
                if (blocking) this.EndInvoke(result);
            }
        }

        public void NewDataReceived(object sender, int index)
        {
            if (this.IsHandleCreated)
            {
                // these lines can be added at the cost of lost markers: if (newDataReceived == null || newDataReceived.IsCompleted)  
                newDataReceived = this.BeginInvoke(new Action(() =>
                {
                    lock (lockObj)
                    {
                        var stream = ((DataStream)sender);
                        foreach (var signalSet in SignalSets)
                        {
                            if (signalSet.Name == stream.Name + "-" + stream.Type)
                            {
                                signalSet.Index = index;
                            }
                        }
                        var ts = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:F3}", stream.Timestamps[index]);
                        var lbInfoTextTemp = $"{stream.Name}-{stream.Type}: {ts}";
                        if (stream.ChFormat == StreamMeta.ChannelFormat.String)
                        {
                            foreach (var mdata in stream.MarkerData)
                            {
                                markerLines.Add(stream.Timestamps[index]);
                                lbInfoTextTemp += ", " + mdata[index];
                            }
                        }
                        else
                        {
                            for (int i = 0; i < stream.Data.Length; i++)
                            {
                                var data = stream.Data[i];
                                var precision = stream.Channels[i].Precision;
                                var format = "{" + $"0:F{precision}" + "}";
                                lbInfoTextTemp += ", " + String.Format(System.Globalization.CultureInfo.InvariantCulture, format, data[index]);
                            }
                        }

                        var s = lbInfoText.Split('\n');
                        var found = false;
                        for (int i = 0; i < s.Length; i++)
                        {
                            if (s[i].Contains($"{stream.Name}-{stream.Type}:"))
                            {
                                s[i] = lbInfoTextTemp;
                                found = true;
                                break;
                            }
                        }
                        lbInfoText = found ? String.Join('\n', s) : lbInfoText + lbInfoTextTemp + "\n";
                    }
                }));
            }
        }

        private void ResetPlot()
        {
            lock (lockObj)
            {
                formsPlot1.Plot.Clear();
                foreach (var axis in axisDict.Values)
                {
                    formsPlot1.Plot.RemoveAxis(axis);
                }
                axisDict.Clear();
                SignalSets.Clear();
            }
        }

        private SignalSet NewSignalSet(string name, double[] timestamps, double[][] data, int index, int precision = 0)
        {
            lock (lockObj)
            {
                SignalSet signalSet = null;

                if (timestamps.Length > 0)
                {
                    var signalPlot = new SignalPlotXY[data.Length];
                    if (SignalSets.Count > 0)
                    {
                        var axis = formsPlot1.Plot.AddAxis(ScottPlot.Renderable.Edge.Left, axisIndex: SignalSets.Count + 1);
                        if (!axisDict.ContainsKey(SignalSets.Count + 1))
                            axisDict.Add(SignalSets.Count + 1, axis);
                    }

                    for (int i = 0; i < data.Length; i++)
                    {
                        signalPlot[i] = formsPlot1.Plot.AddSignalXY(timestamps, data[i], label: $"{name}[{i + 1}]");

                        if (SignalSets.Count > 0)
                        {
                            signalPlot[i].YAxisIndex = SignalSets.Count + 1;
                        }
                    }

                    signalSet = new SignalSet(name, signalPlot, TimeWindow, index, precision);
                    SignalSets.Add(signalSet);
                }

                return signalSet;
            }
        }

        private SignalSet this[int index]
        {
            get => (index < SignalSets.Count && index >= 0) ? SignalSets[index] : null;
        }

        private void AddMarkerLine(double timestamp)
        {
            lock (lockObj)
            {
                var markerLine = formsPlot1.Plot.AddVerticalLine(timestamp, System.Drawing.Color.BlueViolet, 2);
                markerLine.IgnoreAxisAuto = true;
                markerLine.PositionLabel = true;
                markerLine.PositionLabelBackground = markerLine.Color;
                Func<double, string> xFormatter = x => $"X={x:F3}";
                markerLine.PositionFormatter = xFormatter;
            }
        }

        private void timerUpdate_Tick(Object sender, EventArgs e)
        {
            timerUpdate?.Stop();

            if (Run)
            {
                OnTimerUpdate(e);

                lock (lockObj)
                {
                    double maxTs = 0;
                    foreach (var sig in SignalSets)
                    {
                        var ts = sig.UpdateRenderWindow();
                        if (ts > maxTs) maxTs = ts;
                    }

                    lbInfo.Text = lbInfoText;

                    foreach (var marker in markerLines)
                    {
                        AddMarkerLine(marker);
                    }
                    markerLines.Clear();

                    try
                    {
                        formsPlot1.Plot.AxisAuto();
                        formsPlot1.Plot.SetAxisLimitsX(maxTs - TimeWindow, maxTs);

                        formsPlot1.Render();
                    }
                    catch (Exception ex)
                    { };
                }
            }

            if (!token.IsCancellationRequested && !disposing) timerUpdate?.Start();
        }

        private void formsPlot1_DoubleClick(Object sender, EventArgs e)
        {
            Run = !Run;
            lbStatus.Visible = !Run;

            if (!Run)
            {
                lbStatus.Location = new System.Drawing.Point(this.Width / 2 - 50, lbStatus.Location.Y);

                foreach (var signalSet in SignalSets)
                {
                    foreach (var signalPlot in signalSet.SignalPlot)
                    {
                        var markerLine = formsPlot1.Plot.AddVerticalLine(signalPlot.Xs[signalPlot.MaxRenderIndex], System.Drawing.Color.Red, 2);
                        markerLine.IgnoreAxisAuto = true;
                        markerLine.PositionLabel = true;
                        markerLine.PositionLabelBackground = markerLine.Color;
                        Func<double, string> xFormatter = x => $"X={x:F3}";
                        markerLine.PositionFormatter = xFormatter;
                        markerLine.DragEnabled = true;
                    }
                }
            }
        }
    }
}
