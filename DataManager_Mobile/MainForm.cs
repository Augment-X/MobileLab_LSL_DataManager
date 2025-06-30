// 🟡🔴🟠🟢🔵🟣🟤⚫
//TODO: Add multiplot feature
//TODO: Wrap status and info labels: see (https://stackoverflow.com/questions/1204804/word-wrap-for-a-label-in-windows-forms)
//"In my case (label on a panel) I set label.AutoSize = false and label.Dock = Fill. And the label text is wrapped automatically."

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Text.Json;
using System.Linq;
using System.Text.RegularExpressions;
using Libfmax;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.CompilerServices;

namespace DataManager
{
    public partial class MainForm : Form
    {
        // Info sources
        private enum InfoSource : int
        {
            Main = 0,
            Configurator = 1
        };
        // Info masks defined over each UI information display element and for each info source 
        private readonly Info.Mode[] InfoMaskConsole = new Info.Mode[2]{
                Info.Mode.All,
                Info.Mode.All & ~Info.Mode.Data};
        private readonly Info.Mode[] InfoMaskWinConsole = new Info.Mode[2]{
                Info.Mode.All& ~Info.Mode.Data,
                Info.Mode.All & ~Info.Mode.Data & ~Info.Mode.Error};
        private readonly Info.Mode[] InfoMaskSummary = new Info.Mode[2]{
                Info.Mode.All & ~Info.Mode.Data,
                Info.Mode.CriticalEvent | Info.Mode.CriticalError};

        private const string ConfigFileName = @"\config.json";
        private DataStreamConfigurator configurator;
        private static CancellationTokenSource tokenSource = new CancellationTokenSource();
        private static CancellationToken token = tokenSource.Token;
        private System.Diagnostics.Stopwatch stopWatch = new Stopwatch();
        private DataStreamMultiplotter multiplotter;
        private int formHeight;

        public MainForm()
        {
            InitializeComponent();

            String line, configString = "";
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader(Directory.GetCurrentDirectory() + ConfigFileName);
                //Read the first line of text
                line = sr.ReadLine();
                //Continue to read until you reach end of file
                while (line != null)
                {
                    configString += line;
                    //Read the next line
                    line = sr.ReadLine();
                }
                //close the file
                sr.Close();
                configurator = JsonSerializer.Deserialize<DataStreamConfigurator>(configString);
                configurator.InfoMessageReceived += ConfigurationInfoMessageReceived;
            }
            catch
            {
                MessageBox.Show("Could not open the config file, application closing");
                this.Close();
            }

            tbDataPath.Text = configurator.DataPath;

            this.formHeight = this.ClientSize.Height;


            //MOBILE_CHANGES add mobile communication event handler and IP data to the window
            MobileCommunication.DataPacketReceived += MobileCommunication_DataPacketReceived;
            MobileCommunication.StartDataReception();
            lbIP_Address.Text = "IP Address: " + MobileCommunication.GetLocalIPAddress();
        }

        List<(DateTime, string)> steps = new();
        private void MobileCommunication_DataPacketReceived(object sender, MobileCommunicationDataEventArgs e)
        {
            Invoke(new Action(() =>
            {
                switch (e.MessageID)
                {
                    case MobileCommunication.Commands.Record: steps.Add((DateTime.Now, "START/FINISH")); btRecord_Click(null, null); break;
                    case MobileCommunication.Commands.Blue: steps.Add((DateTime.Now, "B")); break;
                    case MobileCommunication.Commands.Green: steps.Add((DateTime.Now, "G")); break;
                    case MobileCommunication.Commands.Yellow: steps.Add((DateTime.Now, "Y")); break;
                    case MobileCommunication.Commands.Red: steps.Add((DateTime.Now, "R")); break;
                    default: /*MobileCommunication.Send(Encoding.ASCII.GetBytes(savedtable));*/ break;
                }
                //PrintInfo(new Info(e.MessageContent, Info.Mode.Event));
            }));
        }

        private void MainForm_FormClosing(Object sender, FormClosingEventArgs e)
        {
            if (!token.IsCancellationRequested)
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
            }
            tmFormUpdate.Dispose();
            configurator = null;
        }

        private void PrintInfo(Info info, InfoSource infoSource = InfoSource.Main)
        {
            var s = $"{DateTime.Now.ToString("HH:mm:ss.fff")}, " + info.msg;

            if ((info.mode & InfoMaskConsole[(int)infoSource]) == info.mode)
            {
                Console.WriteLine(s);
            }

            if ((info.mode & InfoMaskWinConsole[(int)infoSource]) == info.mode)
            {
                tbConsole.BeginInvoke(new Action(() =>
                {
                    tbConsole.Text += s + "\r\n";
                    tbConsole.SelectionStart = tbConsole.Text.Length;
                    tbConsole.ScrollToCaret();
                }));
            }

            if ((info.mode & InfoMaskSummary[(int)infoSource]) == info.mode)
            {
                this.BeginInvoke(new Action(() =>
                {
                    lbStatus.Text = $"Status: {info.msg.Replace("\t", "   ")}";
                    if (info.mode == Info.Mode.Error || info.mode == Info.Mode.CriticalError)
                    {
                        lbStatus.ForeColor = System.Drawing.Color.Red;
                    }
                    else if (info.mode == Info.Mode.CriticalEvent)
                    {
                        lbStatus.ForeColor = System.Drawing.Color.DarkOrange;
                    }
                    else
                    {
                        lbStatus.ForeColor = System.Drawing.Color.Black;
                    }

                }));
            }
        }

        private void ConfigurationInfoMessageReceived(object sender, Info info)
        {
            PrintInfo(info, InfoSource.Configurator);
        }

        private void btSettings_Click(object sender, EventArgs e)
        {
            Process.Start("Notepad", Directory.GetCurrentDirectory() + @"\config.json");
        }

        private void btListStreams_Click(object sender, EventArgs e)
        {
            PrintInfo(new Info("Listing streams.", Info.Mode.Event));

            clbStreams.Items.Clear();

            // wait until all data streams are retrieved
            configurator.StreamInfos = LSL.LSL.resolve_streams().OrderBy(ob => ob.name()).ToArray();

            foreach (var si in configurator.StreamInfos)
            {
                string s = String.Format("{0}, {1}, {2}Hz, {3} channels", si.name(), si.type(), si.nominal_srate(), si.channel_count());

                var ni = clbStreams.Items.Add(s, CheckState.Unchecked);
                if (configurator.GetStream(si) != null)
                {
                    clbStreams.SetItemChecked(ni, true);
                }
            }
        }

        private void btCheckStreams_Click(object sender, EventArgs e)
        {
            if (clbStreams.Items.Count > 0)
            {
                if (btCheckStreams.Text == "Check all")
                {
                    for (int i = 0; i < clbStreams.Items.Count; i++)
                    {
                        clbStreams.SetItemChecked(i, true);
                    }
                    btCheckStreams.Text = "Uncheck all";
                }
                else
                {
                    for (int i = 0; i < clbStreams.Items.Count; i++)
                    {
                        clbStreams.SetItemChecked(i, false);
                    }
                    btCheckStreams.Text = "Check all";
                }
            }
        }

        private void btSubscribe_Click(object sender, EventArgs e)
        {
            //MOBILE_CHANGES Uncheck all and clear streams when Unsubscribing
            if (btSubscribeStreams.Text == "Unsubscribe")
            {
                btListStreams.Enabled = true;
                btCheckStreams.Enabled = true;
                btSubscribeStreams.Text = "Subscribe";
                PrintInfo(new Info("Unsubscribed to streams.", Info.Mode.Event));

                clbMultiplotStreams.Items.Clear();
                multiplotter = new DataStreamMultiplotter();
                configurator.DisconnectUdpStream();
                cbUdpStream.Enabled = false;

                clbStreams.ItemCheck -= new System.Windows.Forms.ItemCheckEventHandler(clbStreams_ItemCheck);
                clbStreams.DoubleClick -= new System.EventHandler(clbStreams_DoubleClick);
                for (int i = 0; i < clbStreams.Items.Count; i++)
                {
                    clbStreams.SetItemChecked(i, false);
                }
                configurator.ClearStream();
            }

            for (int i = 0; i < clbStreams.Items.Count; i++)
            {
                if (clbStreams.GetItemChecked(i))
                {
                    var si = configurator.StreamInfos[i];

                    var udpbytes = ASCIIEncoding.ASCII.GetBytes("S" + clbStreams.Items[i].ToString());
                    MobileCommunication.Send(udpbytes);

                    configurator.AddStream(si);
                }
            }

            //MOBILE_CHANGES deprecated part of the code
            //for (int i = 0; i < clbStreams.Items.Count; i++)
            //{
            //    if (!clbStreams.GetItemChecked(i))
            //    {
            //        clbStreams.Items.RemoveAt(i);
            //        configurator.StreamInfos = configurator.StreamInfos.Where((source, index) => index != i).ToArray();
            //        i = i - 1;
            //    }
            //}

            if (configurator.NbSubscribed > 0)
            {
                btListStreams.Enabled = false;
                btCheckStreams.Enabled = false;
                btSubscribeStreams.Text = "Unsubscribe";
                clbStreams.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(clbStreams_ItemCheck);  //instead of clbStreams.Enabled = false;
                clbStreams.DoubleClick += new System.EventHandler(clbStreams_DoubleClick);
                PrintInfo(new Info("Subscribed to streams.", Info.Mode.Event));

                clbMultiplotStreams.Items.Clear(); //MOBILE_CHANGES clear the check list box before filling it again
                foreach (var si in configurator.StreamInfos)
                {
                    string s = String.Format("{0}, {1}, {2}Hz, {3} channels", si.name(), si.type(), si.nominal_srate(), si.channel_count());
                    var ni = clbMultiplotStreams.Items.Add(s, CheckState.Unchecked);
                }

                multiplotter = new DataStreamMultiplotter();
                //configurator.ConnectUdpStream();
                cbUdpStream.Enabled = true;
            }
        }

        private void cbUdpStream_CheckedChanged(object sender, EventArgs e)
        {
            configurator.UdpStreamEnable = ((CheckBox)sender).Checked;
        }

        private void btMultiplot_Click(object sender, EventArgs e)
        {
            if (multiplotter != null)
            {
                for (int i = 0; i < clbMultiplotStreams.Items.Count; i++)
                {
                    var si = configurator.StreamInfos[i];
                    var stream = configurator.GetStream(si);
                    if (clbMultiplotStreams.GetItemChecked(i))
                    {
                        if (stream != null && !multiplotter.Streams.Exists(e => (e.Name == stream.Name && e.Type == stream.Type)))
                        {
                            multiplotter.Streams.Add(stream);
                        }
                    }
                    else
                    {
                        if (stream != null)
                        {
                            multiplotter.Streams.Remove(stream);
                        }
                    }
                }
                multiplotter.InitPlotter();
            }
        }

        private void clbStreams_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            e.NewValue = e.CurrentValue;
        }

        private void clbStreams_DoubleClick(object sender, EventArgs e)
        {
            configurator?.ShowPlotter(clbStreams.SelectedIndex);
        }

        public void btRecord_Click(object sender, EventArgs e)
        {
            if (configurator.NbSubscribed > 0)
            {
                if (btRecord.Text == "Record")
                {
                    MobileCommunication.Recording = true;
                    btRecord.Enabled = false;
                    configurator.StartRecord();
                    tmFormUpdate.Start();
                    stopWatch.Start();
                    btRecord.Text = "Stop";
                    PrintInfo(new Info($"Recording started.", Info.Mode.Event));

                    btRecord.Enabled = true;
                }
                else
                {
                    MobileCommunication.Recording = false;
                    btRecord.Enabled = false;
                    if (!token.IsCancellationRequested)
                    {
                        tokenSource.Cancel();
                        tokenSource.Dispose();
                    }
                    PrintInfo(new Info($"Recording stopped.", Info.Mode.Event));
                    configurator.SaveSteps(steps);
                    configurator.SaveRecord();
                    tmFormUpdate.Stop();
                    stopWatch.Stop();

                    btRecord.Text = "Record";   ///CHANGE
                    btRecord.Enabled = true;    ///CHANGE
                }
            }
        }

        private void btDataPath_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {

                    tbDataPath.Text = fbd.SelectedPath.ToString();
                    tbProject.Focus();
                }
            }
        }

        private void tabRecord_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((((TextBox)sender).Text.Length == 0))
            {
                e.Handled =
                            (Char.IsPunctuation(e.KeyChar) || Char.IsSeparator(e.KeyChar) || Char.IsSymbol(e.KeyChar));
            }
            else
            {
                e.Handled = e.KeyChar != '-' &&
                            e.KeyChar != '_' &&
                            (Char.IsPunctuation(e.KeyChar) || Char.IsSeparator(e.KeyChar) || Char.IsSymbol(e.KeyChar));
            }
        }

        private void tabRecord_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //if (e.KeyValue == Convert.ToChar(Keys.Enter))
            //{
            //    var regexItem = new Regex("^[a-zA-Z0-9 _/-]*$");

            //    if (sender.Equals(tbProject))
            //    {
            //        if (tbProject.Text != "" && regexItem.IsMatch(tbProject.Text))
            //        {
            //            configurator.Record.Project = tbProject.Text;
            //            tbExperiment.Enabled = true;
            //            tbExperiment.Focus();
            //        }
            //        else
            //        {
            //            PrintInfo(new Info("Please enter a valid project name", Info.Mode.Error));
            //        }
            //    }
            //    else if (sender.Equals(tbExperiment))
            //    {
            //        if (tbExperiment.Text != "" && regexItem.IsMatch(tbExperiment.Text))
            //        {
            //            configurator.Record.Experiment = tbExperiment.Text;
            //            tbSession.Enabled = true;
            //            tbSession.Focus();
            //        }
            //        else
            //        {
            //            PrintInfo(new Info("Please enter a valid experiment name", Info.Mode.Error));
            //        }
            //    }
            //    else if (sender.Equals(tbSession))
            //    {
            //        if (tbSession.Text != "" && regexItem.IsMatch(tbSession.Text))
            //        {
            //            configurator.Record.Session = tbSession.Text;
            //            tbSubject.Enabled = true;
            //            tbSubject.Focus();

            //        }
            //        else
            //        {
            //            PrintInfo(new Info("Please enter a valid session name", Info.Mode.Error));
            //        }
            //    }
            //    else if (sender.Equals(tbSubject))
            //    {
            //        if (regexItem.IsMatch(tbSubject.Text))
            //        {
            //            configurator.Record.Subject = tbSubject.Text;
            //            btConfirm.Enabled = true;
            //            PrintInfo(new Info("Click confirm to enable recording. \n" +
            //                     "⚠ Warning: Existing logs with the same reference will be overwritten.", Info.Mode.CriticalEvent));
            //        }
            //        else
            //        {
            //            PrintInfo(new Info("Please enter a valid subject name", Info.Mode.Error));
            //        }
            //    }
            //}
        }

        private void tabRecord_GotFocus(object sender, EventArgs e)
        {
            //var currentPath = "";
            //if (sender.Equals(tbProject))
            //{
            //    currentPath = tbDataPath.Text;
            //}
            //else if (sender.Equals(tbExperiment))
            //{
            //    currentPath = tbDataPath.Text + "\\" + tbProject.Text;
            //}
            //else if (sender.Equals(tbSession))
            //{
            //    currentPath = tbDataPath.Text + "\\" + tbProject.Text + "\\" + tbExperiment.Text;
            //}
            //else if (sender.Equals(tbSubject))
            //{
            //    currentPath = tbDataPath.Text + "\\" + tbProject.Text + "\\" + tbExperiment.Text + "\\" + tbSession.Text;
            //}

            //if (Directory.Exists(currentPath))
            //{
            //    var source = new AutoCompleteStringCollection();
            //    string[] subdirs = Directory.GetDirectories(currentPath)
            //            .Select(Path.GetFileName)
            //            .ToArray();
            //    source.AddRange(subdirs);
            //    ((TextBox)sender).AutoCompleteCustomSource = source;
            //}
        }

        private void tabRecord_LostFocus(object sender, EventArgs e)
        {
            //if (sender.Equals(tbProject))
            //{
            //    tbProject.Text = configurator?.Record.Project;
            //}
            //else if (sender.Equals(tbExperiment))
            //{
            //    tbExperiment.Text = configurator?.Record.Experiment;
            //}
            //else if (sender.Equals(tbSession))
            //{
            //    tbSession.Text = configurator?.Record.Session;
            //}
            //else if (sender.Equals(tbSubject))
            //{
            //    tbSubject.Text = configurator?.Record.Subject;
            //}
        }

        private void btConfirm_Click(object sender, EventArgs e)
        {
            tbProject.Enabled = false;
            tbExperiment.Enabled = false;
            tbSession.Enabled = false;
            tbSubject.Enabled = false;
            btConfirm.Enabled = false;
            btDataPath.Enabled = false;
            btRecord.Enabled = true;

            configurator.Record.DataPath = tbDataPath.Text;

            //CHANGE
            //this.Text += $" ***New Recording: {configurator.Record.Project} - {configurator.Record.Experiment} - {configurator.Record.Session} - {configurator.Record.Subject}";
            this.Text += $" ***New Recording: {tbDataPath.Text}";
            PrintInfo(new Info("Recording session is registered. Ready to record.", Info.Mode.Event));
        }

        /// <summary>
        /// MOBILE_CHANGES DEPRECATED METHOD. NO LONGER IN USE
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbShowDebugArea_Click(object sender, EventArgs e)
        {
            /*
            if (lbShowDebugArea.Text == "Debug ⏬")
            {
                lbShowDebugArea.Text = "Debug ⏫";
                var ratio = ((double)(MAIN_HEIGHT1 + MAIN_HEIGHT2)) / ((double)MAIN_HEIGHT1);
                this.ClientSize = new System.Drawing.Size(this.ClientSize.Width, (int)((double)this.formHeight * ratio));
            }
            else
            {
                lbShowDebugArea.Text = "Debug ⏬";
                this.ClientSize = new System.Drawing.Size(this.ClientSize.Width, this.formHeight);
            }
            */
        }

        private void tmFormUpdate_Tick(object sender, EventArgs e)
        {
            TimeSpan ts = stopWatch.Elapsed;
            lbElapsedTime.Text = $"Elapsed: {ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
            var recordedKBytes = configurator.RecordedKBytes;
            lbPacketCount.Text = "Data packets: " + ((recordedKBytes < 1024.0) ? $"{recordedKBytes:F1}kB" : $"{recordedKBytes / 1024.0:F3}MB");

            var lostStreams = "";
            foreach (var si in configurator.StreamInfos)
            {
                var stream = configurator.GetStream(si);
                if (stream == null || !stream.IsActive)
                {
                    lostStreams += $"{si.name()}-{si.type()} ";
                }
            }
            if (lostStreams != "")
            {
                lbWarning.Visible = true;
                PrintInfo(new Info($"{lostStreams}: Lost!", Info.Mode.CriticalError));
            }
            else
            {
                lbWarning.Visible = false;
            }
        }

        private void btUnity_Click(object sender, EventArgs e)
        {
            Process.Start(Directory.GetCurrentDirectory() + @"\ErgoInterface.exe");
        }
    }
}

