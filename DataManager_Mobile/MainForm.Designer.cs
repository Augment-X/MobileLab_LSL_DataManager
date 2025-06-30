namespace DataManager
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        //public const int MAIN_WIDTH = 400;    /// MOBILE_CHANGES
        //public const int MAIN_HEIGHT1 = 400;  /// DEPRECATED
        //public const int MAIN_HEIGHT2 = 100;  ///

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            btSettings = new System.Windows.Forms.Button();
            tabControlMain = new System.Windows.Forms.TabControl();
            tabPageMain = new System.Windows.Forms.TabPage();
            tbPort = new System.Windows.Forms.TextBox();
            lbPort = new System.Windows.Forms.Label();
            lbIP_Address = new System.Windows.Forms.Label();
            btListStreams = new System.Windows.Forms.Button();
            btCheckStreams = new System.Windows.Forms.Button();
            lbStatus = new System.Windows.Forms.Label();
            btSubscribeStreams = new System.Windows.Forms.Button();
            btRecord = new System.Windows.Forms.Button();
            cbUdpStream = new System.Windows.Forms.CheckBox();
            lbElapsedTime = new System.Windows.Forms.Label();
            lbPacketCount = new System.Windows.Forms.Label();
            lbWarning = new System.Windows.Forms.Label();
            clbStreams = new System.Windows.Forms.CheckedListBox();
            lbDataPath = new System.Windows.Forms.Label();
            tbDataPath = new System.Windows.Forms.TextBox();
            btDataPath = new System.Windows.Forms.Button();
            tbProject = new System.Windows.Forms.TextBox();
            tbExperiment = new System.Windows.Forms.TextBox();
            tbSession = new System.Windows.Forms.TextBox();
            lbSubject = new System.Windows.Forms.Label();
            tbSubject = new System.Windows.Forms.TextBox();
            btConfirm = new System.Windows.Forms.Button();
            lbProject = new System.Windows.Forms.Label();
            lbExperiment = new System.Windows.Forms.Label();
            lbSession = new System.Windows.Forms.Label();
            tabPageMultiplot = new System.Windows.Forms.TabPage();
            btMultiplot = new System.Windows.Forms.Button();
            clbMultiplotStreams = new System.Windows.Forms.CheckedListBox();
            tbDebug = new System.Windows.Forms.TabPage();
            lbShowDebugArea = new System.Windows.Forms.Label();
            tbConsole = new System.Windows.Forms.TextBox();
            tmFormUpdate = new System.Windows.Forms.Timer(components);
            btUnity = new System.Windows.Forms.Button();
            tabControlMain.SuspendLayout();
            tabPageMain.SuspendLayout();
            tabPageMultiplot.SuspendLayout();
            tbDebug.SuspendLayout();
            SuspendLayout();
            // 
            // btSettings
            // 
            btSettings.Location = new System.Drawing.Point(142, 485);
            btSettings.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            btSettings.Name = "btSettings";
            btSettings.Size = new System.Drawing.Size(58, 38);
            btSettings.TabIndex = 1;
            btSettings.Text = "⚙";
            btSettings.UseVisualStyleBackColor = true;
            btSettings.Click += btSettings_Click;
            // 
            // tabControlMain
            // 
            tabControlMain.Alignment = System.Windows.Forms.TabAlignment.Right;
            tabControlMain.Controls.Add(tabPageMain);
            tabControlMain.Controls.Add(tabPageMultiplot);
            tabControlMain.Controls.Add(tbDebug);
            tabControlMain.Location = new System.Drawing.Point(7, 6);
            tabControlMain.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            tabControlMain.Multiline = true;
            tabControlMain.Name = "tabControlMain";
            tabControlMain.SelectedIndex = 0;
            tabControlMain.Size = new System.Drawing.Size(985, 467);
            tabControlMain.TabIndex = 0;
            // 
            // tabPageMain
            // 
            tabPageMain.Controls.Add(tbPort);
            tabPageMain.Controls.Add(lbPort);
            tabPageMain.Controls.Add(lbIP_Address);
            tabPageMain.Controls.Add(btListStreams);
            tabPageMain.Controls.Add(btCheckStreams);
            tabPageMain.Controls.Add(lbStatus);
            tabPageMain.Controls.Add(btSubscribeStreams);
            tabPageMain.Controls.Add(btRecord);
            tabPageMain.Controls.Add(cbUdpStream);
            tabPageMain.Controls.Add(lbElapsedTime);
            tabPageMain.Controls.Add(lbPacketCount);
            tabPageMain.Controls.Add(lbWarning);
            tabPageMain.Controls.Add(clbStreams);
            tabPageMain.Controls.Add(lbDataPath);
            tabPageMain.Controls.Add(tbDataPath);
            tabPageMain.Controls.Add(btDataPath);
            tabPageMain.Controls.Add(tbProject);
            tabPageMain.Controls.Add(tbExperiment);
            tabPageMain.Controls.Add(tbSession);
            tabPageMain.Controls.Add(lbSubject);
            tabPageMain.Controls.Add(tbSubject);
            tabPageMain.Controls.Add(btConfirm);
            tabPageMain.Controls.Add(lbProject);
            tabPageMain.Controls.Add(lbExperiment);
            tabPageMain.Controls.Add(lbSession);
            tabPageMain.Location = new System.Drawing.Point(4, 4);
            tabPageMain.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            tabPageMain.Name = "tabPageMain";
            tabPageMain.Size = new System.Drawing.Size(947, 459);
            tabPageMain.TabIndex = 0;
            tabPageMain.Text = "Main";
            // 
            // tbPort
            // 
            tbPort.AcceptsReturn = true;
            tbPort.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            tbPort.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            tbPort.Location = new System.Drawing.Point(148, 349);
            tbPort.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            tbPort.Name = "tbPort";
            tbPort.Size = new System.Drawing.Size(95, 31);
            tbPort.TabIndex = 11;
            tbPort.TabStop = false;
            tbPort.Text = "1100";
            // 
            // lbPort
            // 
            lbPort.AutoSize = true;
            lbPort.Location = new System.Drawing.Point(5, 351);
            lbPort.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            lbPort.Name = "lbPort";
            lbPort.Size = new System.Drawing.Size(143, 25);
            lbPort.TabIndex = 10;
            lbPort.Text = "Connection Port:";
            // 
            // lbIP_Address
            // 
            lbIP_Address.AutoSize = true;
            lbIP_Address.Location = new System.Drawing.Point(5, 324);
            lbIP_Address.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            lbIP_Address.Name = "lbIP_Address";
            lbIP_Address.Size = new System.Drawing.Size(178, 25);
            lbIP_Address.TabIndex = 9;
            lbIP_Address.Text = "IP Address: 127.0.0.1";
            // 
            // btListStreams
            // 
            btListStreams.AutoSize = true;
            btListStreams.Location = new System.Drawing.Point(5, 6);
            btListStreams.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            btListStreams.Name = "btListStreams";
            btListStreams.Size = new System.Drawing.Size(125, 38);
            btListStreams.TabIndex = 1;
            btListStreams.Text = "List";
            btListStreams.UseVisualStyleBackColor = true;
            btListStreams.Click += btListStreams_Click;
            // 
            // btCheckStreams
            // 
            btCheckStreams.Location = new System.Drawing.Point(5, 56);
            btCheckStreams.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            btCheckStreams.Name = "btCheckStreams";
            btCheckStreams.Size = new System.Drawing.Size(125, 38);
            btCheckStreams.TabIndex = 2;
            btCheckStreams.Text = "Check all";
            btCheckStreams.UseVisualStyleBackColor = true;
            btCheckStreams.Click += btCheckStreams_Click;
            // 
            // lbStatus
            // 
            lbStatus.AutoSize = true;
            lbStatus.Location = new System.Drawing.Point(5, 388);
            lbStatus.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            lbStatus.Name = "lbStatus";
            lbStatus.Size = new System.Drawing.Size(69, 25);
            lbStatus.TabIndex = 2;
            lbStatus.Text = "Status: ";
            // 
            // btSubscribeStreams
            // 
            btSubscribeStreams.Location = new System.Drawing.Point(5, 106);
            btSubscribeStreams.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            btSubscribeStreams.Name = "btSubscribeStreams";
            btSubscribeStreams.Size = new System.Drawing.Size(125, 38);
            btSubscribeStreams.TabIndex = 3;
            btSubscribeStreams.Text = "Subscribe";
            btSubscribeStreams.UseVisualStyleBackColor = true;
            btSubscribeStreams.Click += btSubscribe_Click;
            // 
            // btRecord
            // 
            btRecord.Enabled = false;
            btRecord.Location = new System.Drawing.Point(625, 100);
            btRecord.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            btRecord.Name = "btRecord";
            btRecord.Size = new System.Drawing.Size(317, 161);
            btRecord.TabIndex = 4;
            btRecord.Text = "Record";
            btRecord.UseVisualStyleBackColor = true;
            btRecord.Click += btRecord_Click;
            // 
            // cbUdpStream
            // 
            cbUdpStream.AutoSize = true;
            cbUdpStream.Enabled = true;
            cbUdpStream.Location = new System.Drawing.Point(381, 273);
            cbUdpStream.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            cbUdpStream.Name = "cbUdpStream";
            cbUdpStream.Size = new System.Drawing.Size(166, 29);
            cbUdpStream.TabIndex = 5;
            cbUdpStream.Text = "Transmit metrics";
            cbUdpStream.UseVisualStyleBackColor = true;
            cbUdpStream.CheckedChanged += cbUdpStream_CheckedChanged;
            // 
            // lbElapsedTime
            // 
            lbElapsedTime.AutoSize = true;
            lbElapsedTime.Location = new System.Drawing.Point(5, 267);
            lbElapsedTime.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            lbElapsedTime.Name = "lbElapsedTime";
            lbElapsedTime.Size = new System.Drawing.Size(150, 25);
            lbElapsedTime.TabIndex = 6;
            lbElapsedTime.Text = "Elapsed: 00:00:00";
            // 
            // lbPacketCount
            // 
            lbPacketCount.AutoSize = true;
            lbPacketCount.Location = new System.Drawing.Point(5, 295);
            lbPacketCount.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            lbPacketCount.Name = "lbPacketCount";
            lbPacketCount.Size = new System.Drawing.Size(166, 25);
            lbPacketCount.TabIndex = 7;
            lbPacketCount.Text = "Data packets: 0.0kB";
            // 
            // lbWarning
            // 
            lbWarning.AutoSize = true;
            lbWarning.BackColor = System.Drawing.Color.Crimson;
            lbWarning.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lbWarning.Location = new System.Drawing.Point(26, 175);
            lbWarning.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            lbWarning.Name = "lbWarning";
            lbWarning.Size = new System.Drawing.Size(85, 60);
            lbWarning.TabIndex = 8;
            lbWarning.Text = "🔗";
            lbWarning.Visible = false;
            // 
            // clbStreams
            // 
            clbStreams.Location = new System.Drawing.Point(140, 5);
            clbStreams.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            clbStreams.Name = "clbStreams";
            clbStreams.ScrollAlwaysVisible = true;
            clbStreams.Size = new System.Drawing.Size(407, 256);
            clbStreams.TabIndex = 6;
            // 
            // lbDataPath
            // 
            lbDataPath.BackColor = System.Drawing.Color.DarkGray;
            lbDataPath.ForeColor = System.Drawing.Color.White;
            lbDataPath.Location = new System.Drawing.Point(625, 6);
            lbDataPath.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            lbDataPath.Name = "lbDataPath";
            lbDataPath.Size = new System.Drawing.Size(100, 29);
            lbDataPath.TabIndex = 0;
            lbDataPath.Text = "Data Path:";
            // 
            // tbDataPath
            // 
            tbDataPath.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            tbDataPath.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            tbDataPath.Location = new System.Drawing.Point(735, 6);
            tbDataPath.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            tbDataPath.Name = "tbDataPath";
            tbDataPath.ReadOnly = true;
            tbDataPath.Size = new System.Drawing.Size(164, 31);
            tbDataPath.TabIndex = 1;
            // 
            // btDataPath
            // 
            btDataPath.Location = new System.Drawing.Point(909, 6);
            btDataPath.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            btDataPath.Name = "btDataPath";
            btDataPath.Size = new System.Drawing.Size(33, 29);
            btDataPath.TabIndex = 2;
            btDataPath.Text = "...";
            btDataPath.UseVisualStyleBackColor = true;
            btDataPath.Click += btDataPath_Click;
            // 
            // tbProject
            // 
                //tbProject.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
                //tbProject.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
                //tbProject.Location = new System.Drawing.Point(735, 49);
                //tbProject.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
                //tbProject.Name = "tbProject";
                //tbProject.Size = new System.Drawing.Size(207, 31);
                //tbProject.TabIndex = 3;
                //tbProject.GotFocus += tabRecord_GotFocus;
                //tbProject.KeyPress += tabRecord_KeyPress;
                //tbProject.LostFocus += tabRecord_LostFocus;
                //tbProject.PreviewKeyDown += tabRecord_PreviewKeyDown;
            // 
            // tbExperiment
            // 
                //tbExperiment.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
                //tbExperiment.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
                //tbExperiment.Enabled = false;
                //tbExperiment.Location = new System.Drawing.Point(735, 92);
                //tbExperiment.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
                //tbExperiment.Name = "tbExperiment";
                //tbExperiment.Size = new System.Drawing.Size(207, 31);
                //tbExperiment.TabIndex = 4;
                //tbExperiment.GotFocus += tabRecord_GotFocus;
                //tbExperiment.KeyPress += tabRecord_KeyPress;
                //tbExperiment.LostFocus += tabRecord_LostFocus;
                //tbExperiment.PreviewKeyDown += tabRecord_PreviewKeyDown;
            // 49
            // tbSession
            // 
                //tbSession.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
                //tbSession.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
                //tbSession.Enabled = false;
                //tbSession.Location = new System.Drawing.Point(735, 135);
                //tbSession.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
                //tbSession.Name = "tbSession";
                //tbSession.Size = new System.Drawing.Size(207, 31);
                //tbSession.TabIndex = 5;
                //tbSession.GotFocus += tabRecord_GotFocus;
                //tbSession.KeyPress += tabRecord_KeyPress;
                //tbSession.LostFocus += tabRecord_LostFocus;
                //tbSession.PreviewKeyDown += tabRecord_PreviewKeyDown;
            // 
            // lbSubject
            // 
                //lbSubject.BackColor = System.Drawing.Color.DarkGray;
                //lbSubject.ForeColor = System.Drawing.Color.White;
                //lbSubject.Location = new System.Drawing.Point(625, 178);
                //lbSubject.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
                //lbSubject.Name = "lbSubject";
                //lbSubject.Size = new System.Drawing.Size(100, 29);
                //lbSubject.TabIndex = 6;
                //lbSubject.Text = "Subject:";
            // 
            // tbSubject
            // 
                //tbSubject.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
                //tbSubject.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
                //tbSubject.Enabled = false;
                //tbSubject.Location = new System.Drawing.Point(735, 178);
                //tbSubject.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
                //tbSubject.Name = "tbSubject";
                //tbSubject.Size = new System.Drawing.Size(207, 31);
                //tbSubject.TabIndex = 6;
                //tbSubject.GotFocus += tabRecord_GotFocus;
                //tbSubject.KeyPress += tabRecord_KeyPress;
                //tbSubject.LostFocus += tabRecord_LostFocus;
                //tbSubject.PreviewKeyDown += tabRecord_PreviewKeyDown;
            // 
            // btConfirm
            // 
            btConfirm.Location = new System.Drawing.Point(625, 49);
            btConfirm.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            btConfirm.Name = "btConfirm";
            btConfirm.Size = new System.Drawing.Size(317, 38);
            btConfirm.TabIndex = 7;
            btConfirm.Text = "Confirm";
            btConfirm.UseVisualStyleBackColor = true;
            btConfirm.Click += btConfirm_Click;
            // 
            // lbProject
            // 
                //lbProject.BackColor = System.Drawing.Color.DarkGray;
                //lbProject.ForeColor = System.Drawing.Color.White;
                //lbProject.Location = new System.Drawing.Point(625, 49);
                //lbProject.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
                //lbProject.Name = "lbProject";
                //lbProject.Size = new System.Drawing.Size(100, 29);
                //lbProject.TabIndex = 3;
                //lbProject.Text = "Project:";
            // 
            // lbExperiment
            // 
                //lbExperiment.BackColor = System.Drawing.Color.DarkGray;
                //lbExperiment.ForeColor = System.Drawing.Color.White;
                //lbExperiment.Location = new System.Drawing.Point(625, 92);
                //lbExperiment.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
                //lbExperiment.Name = "lbExperiment";
                //lbExperiment.Size = new System.Drawing.Size(100, 29);
                //lbExperiment.TabIndex = 4;
                //lbExperiment.Text = "Experiment:";
            // 
            // lbSession
            // 
                //lbSession.BackColor = System.Drawing.Color.DarkGray;
                //lbSession.ForeColor = System.Drawing.Color.White;
                //lbSession.Location = new System.Drawing.Point(625, 135);
                //lbSession.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
                //lbSession.Name = "lbSession";
                //lbSession.Size = new System.Drawing.Size(100, 29);
                //lbSession.TabIndex = 5;
                //lbSession.Text = "Session:";
            // 
            // tabPageMultiplot
            // 
            tabPageMultiplot.Controls.Add(btMultiplot);
            tabPageMultiplot.Controls.Add(clbMultiplotStreams);
            tabPageMultiplot.Location = new System.Drawing.Point(4, 4);
            tabPageMultiplot.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            tabPageMultiplot.Name = "tabPageMultiplot";
            tabPageMultiplot.Size = new System.Drawing.Size(947, 459);
            tabPageMultiplot.TabIndex = 2;
            tabPageMultiplot.Text = "Multiplot";
            // 
            // btMultiplot
            // 
            btMultiplot.Location = new System.Drawing.Point(17, 19);
            btMultiplot.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            btMultiplot.Name = "btMultiplot";
            btMultiplot.Size = new System.Drawing.Size(125, 38);
            btMultiplot.TabIndex = 0;
            btMultiplot.Text = "Plot 📉";
            btMultiplot.UseVisualStyleBackColor = true;
            btMultiplot.Click += btMultiplot_Click;
            // 
            // clbMultiplotStreams
            // 
            clbMultiplotStreams.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            clbMultiplotStreams.Location = new System.Drawing.Point(192, 19);
            clbMultiplotStreams.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            clbMultiplotStreams.Name = "clbMultiplotStreams";
            clbMultiplotStreams.ScrollAlwaysVisible = true;
            clbMultiplotStreams.Size = new System.Drawing.Size(509, 396);
            clbMultiplotStreams.TabIndex = 6;
            // 
            // tbDebug
            // 
            tbDebug.Controls.Add(lbShowDebugArea);
            tbDebug.Controls.Add(tbConsole);
            tbDebug.Location = new System.Drawing.Point(4, 4);
            tbDebug.Name = "tbDebug";
            tbDebug.Padding = new System.Windows.Forms.Padding(3);
            tbDebug.Size = new System.Drawing.Size(947, 459);
            tbDebug.TabIndex = 3;
            tbDebug.Text = "Debug";
            tbDebug.UseVisualStyleBackColor = true;
            // 
            // lbShowDebugArea
            // 
            lbShowDebugArea.BackColor = System.Drawing.Color.DarkGray;
            lbShowDebugArea.ForeColor = System.Drawing.Color.BlueViolet;
            lbShowDebugArea.Location = new System.Drawing.Point(839, 12);
            lbShowDebugArea.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            lbShowDebugArea.Name = "lbShowDebugArea";
            lbShowDebugArea.Size = new System.Drawing.Size(100, 29);
            lbShowDebugArea.TabIndex = 4;
            lbShowDebugArea.Text = "Debug ⏬";
            lbShowDebugArea.Visible = false;
            // 
            // tbConsole
            // 
            tbConsole.Location = new System.Drawing.Point(8, 9);
            tbConsole.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            tbConsole.Multiline = true;
            tbConsole.Name = "tbConsole";
            tbConsole.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            tbConsole.Size = new System.Drawing.Size(613, 441);
            tbConsole.TabIndex = 3;
            // 
            // tmFormUpdate
            // 
            tmFormUpdate.Interval = 1000;
            tmFormUpdate.Tick += tmFormUpdate_Tick;
            // 
            // btUnity
            // 
            btUnity.Location = new System.Drawing.Point(7, 485);
            btUnity.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            btUnity.Name = "btUnity";
            btUnity.Size = new System.Drawing.Size(125, 38);
            btUnity.TabIndex = 1;
            btUnity.Text = "Unity";
            btUnity.UseVisualStyleBackColor = true;
            btUnity.Click += btUnity_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1002, 544);
            Controls.Add(btSettings);
            Controls.Add(tabControlMain);
            Controls.Add(btUnity);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            Name = "MainForm";
            Text = "AugmentX Data Manager";
            FormClosing += MainForm_FormClosing;
            tabControlMain.ResumeLayout(false);
            tabPageMain.ResumeLayout(false);
            tabPageMain.PerformLayout();
            tabPageMultiplot.ResumeLayout(false);
            tbDebug.ResumeLayout(false);
            tbDebug.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Button btSettings;
        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabPageMain;
        private System.Windows.Forms.Button btListStreams;
        private System.Windows.Forms.Button btCheckStreams;
        private System.Windows.Forms.Button btSubscribeStreams;
        private System.Windows.Forms.Button btRecord;
        private System.Windows.Forms.CheckBox cbUdpStream;
        private System.Windows.Forms.Label lbElapsedTime;
        private System.Windows.Forms.Label lbPacketCount;
        private System.Windows.Forms.Label lbWarning;
        private System.Windows.Forms.CheckedListBox clbStreams;
        private System.Windows.Forms.TabPage tabPageMultiplot;
        private System.Windows.Forms.Button btMultiplot;
        private System.Windows.Forms.CheckedListBox clbMultiplotStreams;
        private System.Windows.Forms.Label lbStatus;
        private System.Windows.Forms.Timer tmFormUpdate;

        private System.Windows.Forms.Button btUnity;
        private System.Windows.Forms.Label lbDataPath;
        private System.Windows.Forms.TextBox tbDataPath;
        private System.Windows.Forms.Button btDataPath;
        private System.Windows.Forms.TextBox tbProject;
        private System.Windows.Forms.TextBox tbExperiment;
        private System.Windows.Forms.TextBox tbSession;
        private System.Windows.Forms.Label lbSubject;
        private System.Windows.Forms.TextBox tbSubject;
        private System.Windows.Forms.Button btConfirm;
        private System.Windows.Forms.Label lbProject;
        private System.Windows.Forms.Label lbExperiment;
        private System.Windows.Forms.Label lbSession;
        private System.Windows.Forms.Label lbIP_Address;
        private System.Windows.Forms.TabPage tbDebug;
        private System.Windows.Forms.TextBox tbConsole;
        private System.Windows.Forms.Label lbShowDebugArea;
        private System.Windows.Forms.TextBox tbPort;
        private System.Windows.Forms.Label lbPort;
    }
}


