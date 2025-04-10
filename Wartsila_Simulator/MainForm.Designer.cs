namespace Wartsila_Simulator
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtUserEngValue = new TextBox();
            lbRawBus = new Label();
            btnToggleBits = new Button();
            FileDialogDB = new OpenFileDialog();
            lbTag = new Label();
            txtTag = new TextBox();
            lbDescription = new Label();
            txtDescription = new TextBox();
            lbAdress = new Label();
            txtAddress = new TextBox();
            lbEngLow = new Label();
            txtEngLow = new TextBox();
            lbEngHigh = new Label();
            txtEngHigh = new TextBox();
            lbSerialLow = new Label();
            txtSerialLow = new TextBox();
            lbSerialHigh = new Label();
            txtSerialHigh = new TextBox();
            comboBoxSerialLine = new ComboBox();
            lbEngUnit = new Label();
            txtEngUnit = new TextBox();
            menuStrip1 = new MenuStrip();
            toolStripFile = new ToolStripMenuItem();
            importIOListToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            toolStripCommSettings = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            userManualToolStripMenuItem = new ToolStripMenuItem();
            btnStartSimulator = new Button();
            btnStopSimulator = new Button();
            label2 = new Label();
            label3 = new Label();
            btnResultNotOK = new Button();
            btnResultOK = new Button();
            txtRawBusValue = new TextBox();
            lbRawBusValue = new Label();
            cbPlusRegister = new CheckBox();
            cbMinusRegister = new CheckBox();
            label1 = new Label();
            txtWatchDog = new TextBox();
            listViewSignals = new ListView();
            columnHeaderTag = new ColumnHeader();
            comboBoxWatchdogInterval = new ComboBox();
            btnWatchdogStart = new Button();
            lbWatchdogMs = new Label();
            rtuCheckboxConnected = new UI.RoundCheckbox();
            txtWatchdogAddress = new TextBox();
            lbWatchdogAddress = new Label();
            lbRtuCommunication = new Label();
            tcpCheckboxConnected = new UI.RoundCheckbox();
            lbTcpCommunication = new Label();
            analogTrackBar = new TrackBar();
            lbMinAnalogValueSlider = new Label();
            lbMaxAnalogValueSlider = new Label();
            lbAnalogValueSlider = new Label();
            btnPreviousTag = new Button();
            btnNextTag = new Button();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)analogTrackBar).BeginInit();
            SuspendLayout();
            // 
            // txtUserEngValue
            // 
            txtUserEngValue.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtUserEngValue.Location = new Point(676, 340);
            txtUserEngValue.Name = "txtUserEngValue";
            txtUserEngValue.Size = new Size(125, 27);
            txtUserEngValue.TabIndex = 5;
            txtUserEngValue.TextChanged += txtHoldingValue_TextChanged;
            txtUserEngValue.KeyPress += txtHoldingValue_KeyPress;
            // 
            // lbRawBus
            // 
            lbRawBus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lbRawBus.AutoSize = true;
            lbRawBus.Location = new Point(676, 317);
            lbRawBus.Name = "lbRawBus";
            lbRawBus.Size = new Size(74, 20);
            lbRawBus.TabIndex = 6;
            lbRawBus.Text = "Eng Value";
            // 
            // btnToggleBits
            // 
            btnToggleBits.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnToggleBits.Location = new Point(821, 328);
            btnToggleBits.Name = "btnToggleBits";
            btnToggleBits.Size = new Size(138, 51);
            btnToggleBits.TabIndex = 9;
            btnToggleBits.Text = "Toggle";
            btnToggleBits.UseVisualStyleBackColor = true;
            btnToggleBits.Click += btnToggleValue;
            // 
            // lbTag
            // 
            lbTag.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lbTag.AutoSize = true;
            lbTag.Location = new Point(307, 48);
            lbTag.Name = "lbTag";
            lbTag.Size = new Size(32, 20);
            lbTag.TabIndex = 13;
            lbTag.Text = "Tag";
            // 
            // txtTag
            // 
            txtTag.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtTag.Location = new Point(307, 71);
            txtTag.Name = "txtTag";
            txtTag.ReadOnly = true;
            txtTag.Size = new Size(494, 27);
            txtTag.TabIndex = 15;
            // 
            // lbDescription
            // 
            lbDescription.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lbDescription.AutoSize = true;
            lbDescription.Location = new Point(307, 101);
            lbDescription.Name = "lbDescription";
            lbDescription.Size = new Size(85, 20);
            lbDescription.TabIndex = 16;
            lbDescription.Text = "Description";
            // 
            // txtDescription
            // 
            txtDescription.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtDescription.Location = new Point(307, 125);
            txtDescription.Name = "txtDescription";
            txtDescription.ReadOnly = true;
            txtDescription.Size = new Size(494, 27);
            txtDescription.TabIndex = 17;
            // 
            // lbAdress
            // 
            lbAdress.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lbAdress.AutoSize = true;
            lbAdress.Location = new Point(307, 180);
            lbAdress.Name = "lbAdress";
            lbAdress.Size = new Size(53, 20);
            lbAdress.TabIndex = 18;
            lbAdress.Text = "Adress";
            // 
            // txtAddress
            // 
            txtAddress.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtAddress.Location = new Point(307, 203);
            txtAddress.Name = "txtAddress";
            txtAddress.ReadOnly = true;
            txtAddress.Size = new Size(125, 27);
            txtAddress.TabIndex = 19;
            // 
            // lbEngLow
            // 
            lbEngLow.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lbEngLow.AutoSize = true;
            lbEngLow.Location = new Point(448, 180);
            lbEngLow.Name = "lbEngLow";
            lbEngLow.Size = new Size(34, 20);
            lbEngLow.TabIndex = 20;
            lbEngLow.Text = "Min";
            // 
            // txtEngLow
            // 
            txtEngLow.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtEngLow.Location = new Point(448, 203);
            txtEngLow.Name = "txtEngLow";
            txtEngLow.ReadOnly = true;
            txtEngLow.Size = new Size(125, 27);
            txtEngLow.TabIndex = 21;
            // 
            // lbEngHigh
            // 
            lbEngHigh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lbEngHigh.AutoSize = true;
            lbEngHigh.Location = new Point(579, 180);
            lbEngHigh.Name = "lbEngHigh";
            lbEngHigh.Size = new Size(37, 20);
            lbEngHigh.TabIndex = 22;
            lbEngHigh.Text = "Max";
            // 
            // txtEngHigh
            // 
            txtEngHigh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtEngHigh.Location = new Point(579, 203);
            txtEngHigh.Name = "txtEngHigh";
            txtEngHigh.ReadOnly = true;
            txtEngHigh.Size = new Size(125, 27);
            txtEngHigh.TabIndex = 23;
            // 
            // lbSerialLow
            // 
            lbSerialLow.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lbSerialLow.AutoSize = true;
            lbSerialLow.Location = new Point(522, 245);
            lbSerialLow.Name = "lbSerialLow";
            lbSerialLow.Size = new Size(61, 20);
            lbSerialLow.TabIndex = 24;
            lbSerialLow.Text = "Bus Min";
            // 
            // txtSerialLow
            // 
            txtSerialLow.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtSerialLow.Location = new Point(522, 269);
            txtSerialLow.Name = "txtSerialLow";
            txtSerialLow.ReadOnly = true;
            txtSerialLow.Size = new Size(125, 27);
            txtSerialLow.TabIndex = 25;
            // 
            // lbSerialHigh
            // 
            lbSerialHigh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lbSerialHigh.AutoSize = true;
            lbSerialHigh.Location = new Point(676, 245);
            lbSerialHigh.Name = "lbSerialHigh";
            lbSerialHigh.Size = new Size(64, 20);
            lbSerialHigh.TabIndex = 26;
            lbSerialHigh.Text = "Bus Max";
            // 
            // txtSerialHigh
            // 
            txtSerialHigh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtSerialHigh.Location = new Point(676, 269);
            txtSerialHigh.Name = "txtSerialHigh";
            txtSerialHigh.ReadOnly = true;
            txtSerialHigh.Size = new Size(125, 27);
            txtSerialHigh.TabIndex = 27;
            // 
            // comboBoxSerialLine
            // 
            comboBoxSerialLine.FormattingEnabled = true;
            comboBoxSerialLine.Location = new Point(26, 45);
            comboBoxSerialLine.Name = "comboBoxSerialLine";
            comboBoxSerialLine.Size = new Size(207, 28);
            comboBoxSerialLine.TabIndex = 28;
            comboBoxSerialLine.SelectedIndexChanged += comboBoxSerialLine_SelectedIndexChanged;
            // 
            // lbEngUnit
            // 
            lbEngUnit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lbEngUnit.AutoSize = true;
            lbEngUnit.Location = new Point(720, 177);
            lbEngUnit.Name = "lbEngUnit";
            lbEngUnit.Size = new Size(65, 20);
            lbEngUnit.TabIndex = 29;
            lbEngUnit.Text = "Eng Unit";
            // 
            // txtEngUnit
            // 
            txtEngUnit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtEngUnit.Location = new Point(720, 203);
            txtEngUnit.Name = "txtEngUnit";
            txtEngUnit.ReadOnly = true;
            txtEngUnit.Size = new Size(82, 27);
            txtEngUnit.TabIndex = 30;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { toolStripFile, toolStripCommSettings, helpToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(6, 3, 0, 3);
            menuStrip1.Size = new Size(981, 30);
            menuStrip1.TabIndex = 33;
            menuStrip1.Text = "menuStrip1";
            // 
            // toolStripFile
            // 
            toolStripFile.DropDownItems.AddRange(new ToolStripItem[] { importIOListToolStripMenuItem, exitToolStripMenuItem });
            toolStripFile.Name = "toolStripFile";
            toolStripFile.Size = new Size(46, 24);
            toolStripFile.Text = "File";
            // 
            // importIOListToolStripMenuItem
            // 
            importIOListToolStripMenuItem.Name = "importIOListToolStripMenuItem";
            importIOListToolStripMenuItem.Size = new Size(184, 26);
            importIOListToolStripMenuItem.Text = "Import IO-List";
            importIOListToolStripMenuItem.Click += importIOListToolStripMenuItem_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(184, 26);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // toolStripCommSettings
            // 
            toolStripCommSettings.Name = "toolStripCommSettings";
            toolStripCommSettings.Size = new Size(128, 24);
            toolStripCommSettings.Text = "Communication";
            toolStripCommSettings.Click += toolStripCommSettings_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { userManualToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(55, 24);
            helpToolStripMenuItem.Text = "Help";
            // 
            // userManualToolStripMenuItem
            // 
            userManualToolStripMenuItem.Name = "userManualToolStripMenuItem";
            userManualToolStripMenuItem.Size = new Size(174, 26);
            userManualToolStripMenuItem.Text = "User manual";
            userManualToolStripMenuItem.Click += HelpUserManualClick;
            // 
            // btnStartSimulator
            // 
            btnStartSimulator.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnStartSimulator.Location = new Point(830, 59);
            btnStartSimulator.Name = "btnStartSimulator";
            btnStartSimulator.Size = new Size(139, 51);
            btnStartSimulator.TabIndex = 34;
            btnStartSimulator.Text = "Start Simulator";
            btnStartSimulator.UseVisualStyleBackColor = true;
            btnStartSimulator.Click += btnStartSimulator_Click;
            // 
            // btnStopSimulator
            // 
            btnStopSimulator.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnStopSimulator.Location = new Point(830, 125);
            btnStopSimulator.Name = "btnStopSimulator";
            btnStopSimulator.Size = new Size(139, 51);
            btnStopSimulator.TabIndex = 35;
            btnStopSimulator.Text = "Stop Simulator";
            btnStopSimulator.UseVisualStyleBackColor = true;
            btnStopSimulator.Click += btnStopSimulator_Click;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(21, 327);
            label2.Name = "label2";
            label2.Size = new Size(0, 20);
            label2.TabIndex = 36;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label3.AutoSize = true;
            label3.Location = new Point(807, 389);
            label3.Name = "label3";
            label3.Size = new Size(86, 20);
            label3.TabIndex = 37;
            label3.Text = "Test Result :";
            // 
            // btnResultNotOK
            // 
            btnResultNotOK.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnResultNotOK.BackColor = SystemColors.Control;
            btnResultNotOK.ForeColor = Color.Red;
            btnResultNotOK.Location = new Point(884, 412);
            btnResultNotOK.Name = "btnResultNotOK";
            btnResultNotOK.Size = new Size(94, 29);
            btnResultNotOK.TabIndex = 38;
            btnResultNotOK.Text = "X";
            btnResultNotOK.UseVisualStyleBackColor = false;
            btnResultNotOK.Click += btnResultNotOKClick;
            // 
            // btnResultOK
            // 
            btnResultOK.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnResultOK.BackColor = SystemColors.Control;
            btnResultOK.ForeColor = Color.LawnGreen;
            btnResultOK.Location = new Point(784, 412);
            btnResultOK.Name = "btnResultOK";
            btnResultOK.Size = new Size(94, 29);
            btnResultOK.TabIndex = 39;
            btnResultOK.Text = "✔";
            btnResultOK.UseVisualStyleBackColor = false;
            btnResultOK.Click += btnResultOKClick;
            // 
            // txtRawBusValue
            // 
            txtRawBusValue.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtRawBusValue.Location = new Point(522, 340);
            txtRawBusValue.Name = "txtRawBusValue";
            txtRawBusValue.ReadOnly = true;
            txtRawBusValue.Size = new Size(125, 27);
            txtRawBusValue.TabIndex = 36;
            // 
            // lbRawBusValue
            // 
            lbRawBusValue.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lbRawBusValue.AutoSize = true;
            lbRawBusValue.Location = new Point(554, 317);
            lbRawBusValue.Name = "lbRawBusValue";
            lbRawBusValue.Size = new Size(64, 20);
            lbRawBusValue.TabIndex = 37;
            lbRawBusValue.Text = "Raw Bus";
            // 
            // cbPlusRegister
            // 
            cbPlusRegister.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cbPlusRegister.AutoSize = true;
            cbPlusRegister.Location = new Point(305, 245);
            cbPlusRegister.Name = "cbPlusRegister";
            cbPlusRegister.Size = new Size(49, 24);
            cbPlusRegister.TabIndex = 38;
            cbPlusRegister.Text = "+1";
            cbPlusRegister.UseVisualStyleBackColor = true;
            // 
            // cbMinusRegister
            // 
            cbMinusRegister.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cbMinusRegister.AutoSize = true;
            cbMinusRegister.Location = new Point(386, 245);
            cbMinusRegister.Name = "cbMinusRegister";
            cbMinusRegister.Size = new Size(45, 24);
            cbMinusRegister.TabIndex = 39;
            cbMinusRegister.Text = "-1";
            cbMinusRegister.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(316, 449);
            label1.Name = "label1";
            label1.Size = new Size(136, 20);
            label1.TabIndex = 40;
            label1.Text = "WatchDog counter:";
            // 
            // txtWatchDog
            // 
            txtWatchDog.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtWatchDog.Location = new Point(516, 480);
            txtWatchDog.Name = "txtWatchDog";
            txtWatchDog.ReadOnly = true;
            txtWatchDog.Size = new Size(78, 27);
            txtWatchDog.TabIndex = 41;
            // 
            // listViewSignals
            // 
            listViewSignals.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewSignals.BackColor = Color.Silver;
            listViewSignals.Columns.AddRange(new ColumnHeader[] { columnHeaderTag });
            listViewSignals.Location = new Point(11, 79);
            listViewSignals.Name = "listViewSignals";
            listViewSignals.OwnerDraw = true;
            listViewSignals.Size = new Size(271, 469);
            listViewSignals.TabIndex = 40;
            listViewSignals.UseCompatibleStateImageBehavior = false;
            listViewSignals.View = View.Details;
            listViewSignals.SelectedIndexChanged += listViewSignals_SelectedIndexChanged;
            listViewSignals.KeyDown += listViewSignals_KeyDown;
            listViewSignals.MouseClick += listViewSignals_MouseClick;
            // 
            // columnHeaderTag
            // 
            columnHeaderTag.Text = "Tag";
            columnHeaderTag.Width = 268;
            // 
            // comboBoxWatchdogInterval
            // 
            comboBoxWatchdogInterval.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            comboBoxWatchdogInterval.FormattingEnabled = true;
            comboBoxWatchdogInterval.Items.AddRange(new object[] { "100", "200", "400", "600", "800", "1000" });
            comboBoxWatchdogInterval.Location = new Point(516, 520);
            comboBoxWatchdogInterval.Name = "comboBoxWatchdogInterval";
            comboBoxWatchdogInterval.Size = new Size(108, 28);
            comboBoxWatchdogInterval.TabIndex = 42;
            // 
            // btnWatchdogStart
            // 
            btnWatchdogStart.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnWatchdogStart.Location = new Point(600, 480);
            btnWatchdogStart.Name = "btnWatchdogStart";
            btnWatchdogStart.Size = new Size(94, 29);
            btnWatchdogStart.TabIndex = 43;
            btnWatchdogStart.Text = "Start";
            btnWatchdogStart.UseVisualStyleBackColor = true;
            btnWatchdogStart.Click += btnWatchdogStart_Click;
            // 
            // lbWatchdogMs
            // 
            lbWatchdogMs.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lbWatchdogMs.AutoSize = true;
            lbWatchdogMs.Location = new Point(641, 523);
            lbWatchdogMs.Name = "lbWatchdogMs";
            lbWatchdogMs.Size = new Size(28, 20);
            lbWatchdogMs.TabIndex = 44;
            lbWatchdogMs.Text = "ms";
            // 
            // rtuCheckboxConnected
            // 
            rtuCheckboxConnected.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            rtuCheckboxConnected.AutoSize = true;
            rtuCheckboxConnected.Enabled = false;
            rtuCheckboxConnected.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            rtuCheckboxConnected.Location = new Point(830, 203);
            rtuCheckboxConnected.Name = "rtuCheckboxConnected";
            rtuCheckboxConnected.Size = new Size(117, 27);
            rtuCheckboxConnected.TabIndex = 45;
            rtuCheckboxConnected.Text = "Connected";
            rtuCheckboxConnected.UseVisualStyleBackColor = true;
            // 
            // txtWatchdogAddress
            // 
            txtWatchdogAddress.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtWatchdogAddress.Location = new Point(357, 480);
            txtWatchdogAddress.Name = "txtWatchdogAddress";
            txtWatchdogAddress.Size = new Size(125, 27);
            txtWatchdogAddress.TabIndex = 46;
            // 
            // lbWatchdogAddress
            // 
            lbWatchdogAddress.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lbWatchdogAddress.AutoSize = true;
            lbWatchdogAddress.Location = new Point(289, 484);
            lbWatchdogAddress.Name = "lbWatchdogAddress";
            lbWatchdogAddress.Size = new Size(65, 20);
            lbWatchdogAddress.TabIndex = 47;
            lbWatchdogAddress.Text = "Address:";
            // 
            // lbRtuCommunication
            // 
            lbRtuCommunication.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lbRtuCommunication.AutoSize = true;
            lbRtuCommunication.Location = new Point(830, 179);
            lbRtuCommunication.Name = "lbRtuCommunication";
            lbRtuCommunication.Size = new Size(148, 20);
            lbRtuCommunication.TabIndex = 48;
            lbRtuCommunication.Text = "RTU Communication ";
            // 
            // tcpCheckboxConnected
            // 
            tcpCheckboxConnected.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            tcpCheckboxConnected.AutoSize = true;
            tcpCheckboxConnected.Enabled = false;
            tcpCheckboxConnected.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            tcpCheckboxConnected.Location = new Point(830, 254);
            tcpCheckboxConnected.Name = "tcpCheckboxConnected";
            tcpCheckboxConnected.Size = new Size(117, 27);
            tcpCheckboxConnected.TabIndex = 49;
            tcpCheckboxConnected.Text = "Connected";
            tcpCheckboxConnected.UseVisualStyleBackColor = true;
            // 
            // lbTcpCommunication
            // 
            lbTcpCommunication.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lbTcpCommunication.AutoSize = true;
            lbTcpCommunication.Location = new Point(830, 231);
            lbTcpCommunication.Name = "lbTcpCommunication";
            lbTcpCommunication.Size = new Size(142, 20);
            lbTcpCommunication.TabIndex = 50;
            lbTcpCommunication.Text = "TCP Communication";
            // 
            // analogTrackBar
            // 
            analogTrackBar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            analogTrackBar.Location = new Point(522, 413);
            analogTrackBar.Name = "analogTrackBar";
            analogTrackBar.Size = new Size(263, 56);
            analogTrackBar.TabIndex = 51;
            analogTrackBar.Scroll += analogTrackBar_Scroll;
            // 
            // lbMinAnalogValueSlider
            // 
            lbMinAnalogValueSlider.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lbMinAnalogValueSlider.AutoSize = true;
            lbMinAnalogValueSlider.Location = new Point(533, 449);
            lbMinAnalogValueSlider.Name = "lbMinAnalogValueSlider";
            lbMinAnalogValueSlider.Size = new Size(74, 20);
            lbMinAnalogValueSlider.TabIndex = 52;
            lbMinAnalogValueSlider.Text = "Min Value";
            // 
            // lbMaxAnalogValueSlider
            // 
            lbMaxAnalogValueSlider.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lbMaxAnalogValueSlider.AutoSize = true;
            lbMaxAnalogValueSlider.Location = new Point(751, 449);
            lbMaxAnalogValueSlider.Name = "lbMaxAnalogValueSlider";
            lbMaxAnalogValueSlider.Size = new Size(77, 20);
            lbMaxAnalogValueSlider.TabIndex = 53;
            lbMaxAnalogValueSlider.Text = "Max Value";
            // 
            // lbAnalogValueSlider
            // 
            lbAnalogValueSlider.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lbAnalogValueSlider.AutoSize = true;
            lbAnalogValueSlider.Location = new Point(641, 390);
            lbAnalogValueSlider.Name = "lbAnalogValueSlider";
            lbAnalogValueSlider.Size = new Size(75, 20);
            lbAnalogValueSlider.TabIndex = 54;
            lbAnalogValueSlider.Text = "Live Value";
            // 
            // btnPreviousTag
            // 
            btnPreviousTag.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPreviousTag.Location = new Point(770, 480);
            btnPreviousTag.Name = "btnPreviousTag";
            btnPreviousTag.Size = new Size(101, 29);
            btnPreviousTag.TabIndex = 52;
            btnPreviousTag.Text = "Previous Tag";
            btnPreviousTag.UseVisualStyleBackColor = true;
            btnPreviousTag.Click += btnPreviousTag_Click;
            // 
            // btnNextTag
            // 
            btnNextTag.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNextTag.Location = new Point(877, 480);
            btnNextTag.Name = "btnNextTag";
            btnNextTag.Size = new Size(101, 29);
            btnNextTag.TabIndex = 53;
            btnNextTag.Text = "Next Tag";
            btnNextTag.UseVisualStyleBackColor = true;
            btnNextTag.Click += btnNextTag_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(981, 558);
            Controls.Add(lbAnalogValueSlider);
            Controls.Add(lbMaxAnalogValueSlider);
            Controls.Add(lbMinAnalogValueSlider);
            Controls.Add(btnNextTag);
            Controls.Add(btnPreviousTag);
            Controls.Add(analogTrackBar);
            Controls.Add(lbTcpCommunication);
            Controls.Add(tcpCheckboxConnected);
            Controls.Add(lbRtuCommunication);
            Controls.Add(lbWatchdogAddress);
            Controls.Add(txtWatchdogAddress);
            Controls.Add(rtuCheckboxConnected);
            Controls.Add(lbWatchdogMs);
            Controls.Add(btnWatchdogStart);
            Controls.Add(comboBoxWatchdogInterval);
            Controls.Add(txtWatchDog);
            Controls.Add(label1);
            Controls.Add(listViewSignals);
            Controls.Add(btnResultOK);
            Controls.Add(btnResultNotOK);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(cbMinusRegister);
            Controls.Add(cbPlusRegister);
            Controls.Add(lbRawBusValue);
            Controls.Add(txtRawBusValue);
            Controls.Add(btnStopSimulator);
            Controls.Add(btnStartSimulator);
            Controls.Add(txtEngUnit);
            Controls.Add(lbEngUnit);
            Controls.Add(comboBoxSerialLine);
            Controls.Add(txtSerialHigh);
            Controls.Add(lbSerialHigh);
            Controls.Add(txtSerialLow);
            Controls.Add(lbSerialLow);
            Controls.Add(txtEngHigh);
            Controls.Add(lbEngHigh);
            Controls.Add(txtEngLow);
            Controls.Add(lbEngLow);
            Controls.Add(txtAddress);
            Controls.Add(lbAdress);
            Controls.Add(txtDescription);
            Controls.Add(lbDescription);
            Controls.Add(txtTag);
            Controls.Add(lbTag);
            Controls.Add(btnToggleBits);
            Controls.Add(lbRawBus);
            Controls.Add(txtUserEngValue);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Margin = new Padding(3, 4, 3, 4);
            Name = "MainForm";
            Text = "Wartsila Simulator";
            FormClosing += Form1_FormClosing;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)analogTrackBar).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.TextBox txtUserEngValue;
        private System.Windows.Forms.Label lbRawBus;
        private System.Windows.Forms.Button btnToggleBits;
        private System.Windows.Forms.OpenFileDialog FileDialogDB;
        private System.Windows.Forms.Label lbTag;
        private System.Windows.Forms.TextBox txtTag;
        private System.Windows.Forms.Label lbDescription;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label lbAdress;
        private System.Windows.Forms.TextBox txtAddress;
        private System.Windows.Forms.Label lbEngLow;
        private System.Windows.Forms.TextBox txtEngLow;
        private System.Windows.Forms.Label lbEngHigh;
        private System.Windows.Forms.TextBox txtEngHigh;
        private System.Windows.Forms.Label lbSerialLow;
        private System.Windows.Forms.TextBox txtSerialLow;
        private System.Windows.Forms.Label lbSerialHigh;
        private System.Windows.Forms.TextBox txtSerialHigh;
        private System.Windows.Forms.ComboBox comboBoxSerialLine;
        private System.Windows.Forms.Label lbEngUnit;
        private System.Windows.Forms.TextBox txtEngUnit;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripFile;
        private System.Windows.Forms.ToolStripMenuItem importIOListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripCommSettings;
        private System.Windows.Forms.Button btnStartSimulator;
        private System.Windows.Forms.Button btnStopSimulator;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnResultNotOK;
        private System.Windows.Forms.Button btnResultOK;
        private System.Windows.Forms.TextBox txtRawBusValue;
        private System.Windows.Forms.Label lbRawBusValue;
        private System.Windows.Forms.CheckBox cbPlusRegister;
        private System.Windows.Forms.CheckBox cbMinusRegister;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem userManualToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtWatchDog;
        private System.Windows.Forms.ListView listViewSignals;
        private System.Windows.Forms.ColumnHeader columnHeaderTag;
        private System.Windows.Forms.ComboBox comboBoxWatchdogInterval;
        private System.Windows.Forms.Button btnWatchdogStart;
        private System.Windows.Forms.Label lbWatchdogMs;
        private Wartsila_Simulator.UI.RoundCheckbox rtuCheckboxConnected;
        private System.Windows.Forms.TextBox txtWatchdogAddress;
        private System.Windows.Forms.Label lbWatchdogAddress;
        private System.Windows.Forms.Label lbRtuCommunication;
        private Wartsila_Simulator.UI.RoundCheckbox tcpCheckboxConnected;
        private System.Windows.Forms.Label lbTcpCommunication;
        private System.Windows.Forms.TrackBar analogTrackBar;
        private System.Windows.Forms.Label lbMinAnalogValueSlider;
        private System.Windows.Forms.Label lbMaxAnalogValueSlider;
        private System.Windows.Forms.Label lbAnalogValueSlider;
        private System.Windows.Forms.Button btnPreviousTag;
        private System.Windows.Forms.Button btnNextTag;
    }
}