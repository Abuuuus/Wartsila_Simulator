using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Wartsila_Simulator.Managers;
using Wartsila_Simulator.Settings;
using Wartsila_Simulator.UI;
using Modbus.Data;
using Modbus.Device;
using Wartsila_Simulator.Utilities;

namespace Wartsila_Simulator
{
    /// <summary>
    /// Main form for the Wartsila Simulator application.
    /// </summary>
    public partial class MainForm : Form
    {
        #region Private Fields

        // Managers
        private ModbusManager modbusManager;
        private IOListManager ioListManager;
        private WatchdogManager watchdogManager;

        // UI state tracking
        private int selectedSignalIndex = -1;
        private bool isModbusMasterWrite = false;

        //Needed for watchdog and not updating registers written to
        private bool isWatchdogUpdate = false;
        private ushort lastWatchdogAddress = ushort.MaxValue;

        //Needed for updating the connectecheckboxes
        private DateTime lastCommunicationTimeRtu = DateTime.MinValue;
        private DateTime lastCommunicationTimeTcp = DateTime.MinValue;
        private CancellationTokenSource rtuUncheckTokenSource;
        private CancellationTokenSource tcpUncheckTokenSource;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the MainForm class.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            // Configure form
            StartPosition = FormStartPosition.CenterScreen;
            listViewSignals.DrawItem += listViewSignals_DrawItem;
            listViewSignals.DrawSubItem += listViewSignals_DrawSubItem;

            // Initialize managers
            InitializeManagers();

            // Initialize UI state
            UpdateUIState();


        }

        #endregion

        #region Initialization Methods

        /// <summary>
        /// Initializes the application managers.
        /// </summary>
        private void InitializeManagers()
        {
            try
            {
                // Initialize Modbus manager
                modbusManager = new ModbusManager();

                // Initialize IO list manager
                ioListManager = new IOListManager();

                // Initialize watchdog manager
                watchdogManager = new WatchdogManager(modbusManager);
                watchdogManager.CounterUpdated += OnWatchdogCounterUpdated;
                watchdogManager.AlarmRaised += OnAlarmRaised;
                watchdogManager.RestartNeeded += OnRestartNeeded;
                watchdogManager.WatchdogUpdateStarting += (s, e) => isWatchdogUpdate = true;
                watchdogManager.WatchdogUpdateCompleted += (s, e) => isWatchdogUpdate = false;

                // Subscribe to Modbus events
                modbusManager.RtuRequestReceived += OnRtuRequestReceived;
                modbusManager.TcpRequestReceived += OnTcpRequestReceived;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing application: {ex.Message}",
                    "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Updates the UI state based on application state.
        /// </summary>
        private void UpdateUIState()
        {
            // Disable controls that require data or active connections
            btnStopSimulator.Enabled = false;
            analogTrackBar.Enabled = false;
            btnToggleBits.Enabled = false;
            btnResultNotOK.Enabled = false;
            btnResultOK.Enabled = false;
        }

        #endregion

        #region Event Handlers - Modbus Communication

        /// <summary>
        /// Checks if the register that was written to by the Modbus master corresponds to the selected tag
        /// and updates the UI if needed.
        /// </summary>
        private void CheckSelectedTagRegisterChanged()
        {
            try
            {
                // Exit if no tag is selected
                if (selectedSignalIndex < 0 || selectedSignalIndex >= ioListManager.SerialAddresses.Count)
                    return;

                //Fetch the serial address and protocol (RTU or TCP) for the selected tag
                string serialAddress = ioListManager.SerialAddresses[selectedSignalIndex];
                string protocol = ioListManager.GetProtocolForSignal(selectedSignalIndex);

                // Get the register address for the currently selected tag
                int baseAddress;
                int bitPosition = -1;
                if (serialAddress.Contains("."))
                {
                    // For bit addresses
                    baseAddress = int.Parse(serialAddress.Substring(1, serialAddress.IndexOf(".") - 1));
                    bitPosition = int.Parse(serialAddress.Substring(serialAddress.IndexOf(".") + 1));
                }
                else
                {
                    // For regular addresses
                    baseAddress = int.Parse(serialAddress.Substring(1));
                }

                // Apply register offset if needed
                int adjustedAddress = baseAddress;
                if (cbPlusRegister?.Checked == true && cbMinusRegister?.Checked == false)
                {
                    adjustedAddress += 1;
                }
                else if (cbMinusRegister?.Checked == true && cbPlusRegister?.Checked == false)
                {
                    adjustedAddress -= 1;
                }

                // Get the current value from the appropriate register
                ushort currentValue = modbusManager.SafeGetRegisterValue((ushort)adjustedAddress, protocol);

                // Apply the appropriate transformations based on the signal type
                if (bitPosition >= 0)
                {
                    // For digital signals, extract the bit value
                    bool bitState = (currentValue & (1 << bitPosition)) != 0;
                    UpdateUserEngValueText(bitState ? "1" : "0");
                }
                else
                {
                    // For analog signals, scale the raw value to engineering value
                    long rawValue = currentValue;

                    // Handle negative values (two's complement)
                    if (rawValue > 32767)
                    {
                        rawValue = rawValue - 65536;
                    }

                    // Scale using the engineering ranges
                    long engScaleLow = long.Parse(ioListManager.EngRangeLow[selectedSignalIndex]);
                    long engScaleHigh = long.Parse(ioListManager.EngRangeHigh[selectedSignalIndex]);
                    long busScaleLow = long.Parse(ioListManager.SerialLineLow[selectedSignalIndex]);
                    long busScaleHigh = long.Parse(ioListManager.SerialLineHigh[selectedSignalIndex]);

                    // Calculate the scaled engineering value
                    long scaledValue = (long)engScaleLow + ((rawValue - busScaleLow) * (engScaleHigh - engScaleLow) / (busScaleHigh - busScaleLow));

                    // Update the text box
                    UpdateUserEngValueText(scaledValue.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking selected tag register: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the Start Simulator button click event.
        /// </summary>
        private void btnStartSimulator_Click(object sender, EventArgs e)
        {
            try
            {
                var settings = CommunicationSettings.Instance;

                if (settings.TcpCommunicationEnabled || settings.RtuCommunicationEnabled)
                {
                    bool started = modbusManager.StartModbusSlaves();

                    if (started)
                    {
                        btnStartSimulator.Enabled = false;
                        btnStopSimulator.Enabled = true;
                    }
                }
                else
                {
                    MessageBox.Show("Please enable at least one communication protocol in settings.",
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting simulator: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the Stop Simulator button click event.
        /// </summary>
        private void btnStopSimulator_Click(object sender, EventArgs e)
        {
            try
            {
                modbusManager.StopModbusSlaves();
                watchdogManager.Stop();
                btnStartSimulator.Enabled = true;
                btnStopSimulator.Enabled = false;
                txtWatchdogAddress.ReadOnly = false;
                btnWatchdogStart.Text = "Start";

                // Update connected indicators
                rtuCheckboxConnected.Checked = false;
                tcpCheckboxConnected.Checked = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping simulator: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles Modbus RTU request received events.
        /// </summary>
        private void OnRtuRequestReceived(object sender, ModbusSlaveRequestEventArgs e)
        {
            try
            {
                // Check if this is a write request from the Modbus master
                isModbusMasterWrite = IsWriteRequest(e);

                //Updating time since last request
                lastCommunicationTimeRtu = DateTime.Now;

                // Update watchdog status
                watchdogManager.SetRtuRequestReceived();

                // Update UI on the UI thread
                if (!IsHandleCreated || IsDisposed) return;

                try
                {
                    Invoke(new Action(() =>
                    {
                        if (!IsDisposed)
                        {
                            rtuCheckboxConnected.Checked = true;
                        }
                    }));
                }
                catch (ObjectDisposedException) { }

                // Start delayed uncheck task to clear the indicator after 2 seconds
                StartRtuDelayedUncheck();

                // If this was a write request, check if we need to update txtUserEngValue
                if (isModbusMasterWrite)
                {
                    // Add a delay to ensure the register has been updated
                    Task.Delay(50).ContinueWith(_ => {
                        if (!IsDisposed)
                        {
                            CheckSelectedTagRegisterChanged();
                        }
                    });
                }

                // Reset the flag after processing
                isModbusMasterWrite = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling RTU request: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles Modbus TCP request received events.
        /// </summary>
        private void OnTcpRequestReceived(object sender, ModbusSlaveRequestEventArgs e)
        {
            try
            {
                // Check if this is a write request from the Modbus master
                isModbusMasterWrite = IsWriteRequest(e);

                //Updating time since last request
                lastCommunicationTimeTcp = DateTime.Now;

                // Update watchdog status
                watchdogManager.SetTcpRequestReceived();

                // Update UI on the UI thread
                if (!IsHandleCreated || IsDisposed) return;

                try
                {
                    Invoke(new Action(() =>
                    {
                        if (!IsDisposed)
                        {
                            tcpCheckboxConnected.Checked = true;
                        }
                    }));
                }
                catch (ObjectDisposedException) { }

                // Start delayed uncheck task to clear the indicator after 2 seconds
                StartTcpDelayedUncheck();

                // If this was a write request, check if we need to update txtUserEngValue
                if (isModbusMasterWrite)
                {
                    // Add a delay to ensure the register has been updated
                    Task.Delay(50).ContinueWith(_ => {
                        if (!IsDisposed)
                        {
                            CheckSelectedTagRegisterChanged();
                        }
                    });
                }

                // Reset the flag after processing
                isModbusMasterWrite = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling TCP request: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if a Modbus request is a write request.
        /// </summary>
        /// <param name="e">The Modbus request event arguments.</param>
        /// <returns>True if the request is a write request, otherwise false.</returns>
        private bool IsWriteRequest(ModbusSlaveRequestEventArgs e)
        {
            try
            {
                if (e?.Message == null)
                    return false;

                // These are the standard Modbus function codes for write requests
                byte[] writeFunctionCodes = new byte[]
                {
                    6,  // Write Single Register
                    16, // Write Multiple Registers
                    23  // Read/Write Multiple Registers (has write component)
                };


                // Check if the message's function code is in the write function codes
                return Array.IndexOf(writeFunctionCodes, e.Message.FunctionCode) >= 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking if request is write: {e.Message.FunctionCode}, {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Event Handlers - Communication Settings

        /// <summary>
        /// Handles the Communication menu item click event.
        /// </summary>
        private void toolStripCommSettings_Click(object sender, EventArgs e)
        {
            try
            {
                CommunicationSettingsUI comSettings = new CommunicationSettingsUI();
                if (comSettings.ShowDialog() == DialogResult.OK)
                {
                    // Update settings in the singleton instance
                    CommunicationSettings.Instance.UpdateSettings(comSettings);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error configuring communication settings: {ex.Message}",
                    "Settings Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Event Handlers - IO List

        /// <summary>
        /// Handles the Import IO-List menu item click event.
        /// </summary>
        private void importIOListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                FileDialogDB.InitialDirectory = "C:\\Marine\\Projects";
                FileDialogDB.Filter = "Select Database(*.mdb)|*.mdb";

                if (FileDialogDB.ShowDialog() == DialogResult.OK)
                {

                    string filePath = FileDialogDB.FileName;
                    bool imported = ioListManager.ImportIOList(filePath, modbusManager);

                    if (imported)
                    {
                        // Clear UI
                        comboBoxSerialLine.Items.Clear();
                        comboBoxSerialLine.Text = "";
                        listViewSignals.Items.Clear();

                        // Populate serial line dropdown
                        List<string> serialLines = ioListManager.GetUniqueSerialLineNames();
                        comboBoxSerialLine.Items.AddRange(serialLines.ToArray());

                        // Enable result buttons
                        btnResultOK.Enabled = true;
                        btnResultNotOK.Enabled = true;

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing IO list: {ex.Message}",
                    "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the Serial Line combo box selection change event.
        /// </summary>
        private void comboBoxSerialLine_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {

                // Clear the signals list
                listViewSignals.Items.Clear();

                // Get the selected serial line
                string serialLineSelected = comboBoxSerialLine.Text;

                // Get signals for the selected serial line
                List<ListViewItem> signals = ioListManager.GetSignalsForSerialLine(serialLineSelected);

                // Add signals to the list view
                listViewSignals.Items.AddRange(signals.ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading signals for serial line: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Event Handlers - Signal Management

        /// <summary>
        /// Handles the ListView signals selection change event.
        /// </summary>
        private void listViewSignals_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update signal information when selection changes
            UpdateSignalInformation();

            // Refresh the list view
            listViewSignals.Invalidate();
        }

        /// <summary>
        /// Handles the ListView signals mouse click event.
        /// </summary>
        private void listViewSignals_MouseClick(object sender, MouseEventArgs e)
        {
            // Update signal information when a signal is clicked
            UpdateSignalInformation();
        }

        /// <summary>
        /// Handles the ListView signals key down event.
        /// </summary>
        private void listViewSignals_KeyDown(object sender, KeyEventArgs e)
        {
            // Update signal information when up/down arrow keys are used
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {

                UpdateSignalInformation();
            }
        }

        /// <summary>
        /// Handles the Previous Tag button click event.
        /// </summary>
        private void btnPreviousTag_Click(object sender, EventArgs e)
        {
            try
            {
                if (listViewSignals.Items.Count > 0)
                {

                    int currentIndex = listViewSignals.SelectedIndices.Count > 0
                        ? listViewSignals.SelectedIndices[0]
                        : 0;

                    // Move to previous item, wrapping around to the end if at the start
                    int newIndex = (currentIndex - 1 + listViewSignals.Items.Count) % listViewSignals.Items.Count;

                    // Update selection
                    listViewSignals.Items[currentIndex].Selected = false;
                    listViewSignals.Items[newIndex].Selected = true;
                    listViewSignals.EnsureVisible(newIndex);

                    // Update signal information
                    UpdateSignalInformation();

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to previous tag: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the Next Tag button click event.
        /// </summary>
        private void btnNextTag_Click(object sender, EventArgs e)
        {
            try
            {
                if (listViewSignals.Items.Count > 0)
                {

                    int currentIndex = listViewSignals.SelectedIndices.Count > 0
                        ? listViewSignals.SelectedIndices[0]
                        : -1;

                    // Move to next item, wrapping around to the start if at the end
                    int newIndex = (currentIndex + 1) % listViewSignals.Items.Count;

                    // Update selection
                    if (currentIndex != -1)
                    {
                        listViewSignals.Items[currentIndex].Selected = false;
                    }
                    listViewSignals.Items[newIndex].Selected = true;
                    listViewSignals.EnsureVisible(newIndex);

                    // Update signal information
                    UpdateSignalInformation();

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to next tag: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the signal information in the UI based on the selected signal.
        /// </summary>
        private void UpdateSignalInformation()
        {
            try
            {
                if (listViewSignals.SelectedItems.Count <= 0)
                    return;

                // Get the selected item
                ListViewItem selectedItem = listViewSignals.SelectedItems[0];
                string textHighlighted = selectedItem.Text;

                // Extract tag from the text (remove loop typical)
                string tagHighlighted = textHighlighted.Contains(" ")
                    ? textHighlighted.Substring(0, textHighlighted.IndexOf(" "))
                    : textHighlighted;

                // Find the tag in the manager
                int position = ioListManager.Tags.IndexOf(tagHighlighted);
                if (position == -1)
                {
                    // Tag not found
                    MessageBox.Show("Tag not found in the list.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Save the selected signal index
                selectedSignalIndex = position;

                // Reset analog value display
                lbAnalogValueSlider.Text = "0";

                // Update UI with signal information
                txtAddress.Text = ioListManager.SerialAddresses[position];
                txtDescription.Text = ioListManager.Descriptions[position];
                txtEngHigh.Text = ioListManager.EngRangeHigh[position];
                txtEngLow.Text = ioListManager.EngRangeLow[position];
                txtSerialHigh.Text = ioListManager.SerialLineHigh[position];
                txtSerialLow.Text = ioListManager.SerialLineLow[position];
                txtTag.Text = tagHighlighted;
                txtEngUnit.Text = ioListManager.EngUnits[position];

                // Update current value
                UpdateSignalCurrentValue(position);

                // Configure analog trackbar
                SetupAnalogTrackBar(position);

                // Update UI based on signal type
                bool isDigital = ioListManager.IsDigitalSignal(position);
                if (isDigital)
                {
                    btnToggleBits.Enabled = true;
                    analogTrackBar.Value = 0;
                    analogTrackBar.Enabled = false;
                    lbMaxAnalogValueSlider.Visible = false;
                    lbMinAnalogValueSlider.Visible = false;
                    lbAnalogValueSlider.Visible = false;
                }
                else
                {
                    btnToggleBits.Enabled = false;
                    analogTrackBar.Enabled = true;
                    lbMaxAnalogValueSlider.Visible = true;
                    lbMinAnalogValueSlider.Visible = true;
                    lbAnalogValueSlider.Visible = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating signal information: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the current value display for the selected signal.
        /// </summary>
        /// <param name="position">The index of the signal in the lists.</param>
        private void UpdateSignalCurrentValue(int position)
        {
            try
            {
                bool isDigital = ioListManager.IsDigitalSignal(position);
                string protocol = ioListManager.GetProtocolForSignal(position);

                if (isDigital)
                {
                    // Parse the address to get register address and bit position
                    string serialAddress = ioListManager.SerialAddresses[position];
                    int address = int.Parse(serialAddress.Substring(1, serialAddress.IndexOf(".") - 1));
                    int bitNumber = int.Parse(serialAddress.Substring(serialAddress.IndexOf(".") + 1));

                    // Get the current value from the appropriate datastore
                    ushort byteValue = modbusManager.SafeGetRegisterValue((ushort)address, protocol);
                    ushort mask = (ushort)(1 << bitNumber);

                    // Set the text box value based on the bit state
                    txtUserEngValue.Text = (byteValue & mask) != 0 ? "1" : "0";
                }
                else
                {
                    // Parse the address to get register address
                    string serialAddress = ioListManager.SerialAddresses[position];
                    int address = int.Parse(serialAddress.Substring(1));

                    // Get the current value from the appropriate datastore
                    long previousValue = modbusManager.SafeGetRegisterValue((ushort)address, protocol);

                    // Handle negative values (stored as two's complement)
                    if (previousValue > 32767)
                    {
                        previousValue = previousValue - 65536;
                    }

                    // Scale the value from bus range to engineering range
                    long engScaleLow = long.Parse(ioListManager.EngRangeLow[position]);
                    long engScaleHigh = long.Parse(ioListManager.EngRangeHigh[position]);
                    long busScaleLow = long.Parse(ioListManager.SerialLineLow[position]);
                    long busScaleHigh = long.Parse(ioListManager.SerialLineHigh[position]);

                    long scaledValue = engScaleLow + ((previousValue - busScaleLow) * (engScaleHigh - engScaleLow) / (busScaleHigh - busScaleLow));

                    // Update the text boxes
                    txtUserEngValue.Text = scaledValue.ToString();
                    lbAnalogValueSlider.Text = scaledValue.ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating signal current value: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets up the analog trackbar for the selected signal.
        /// </summary>
        /// <param name="position">The index of the signal in the lists.</param>
        private void SetupAnalogTrackBar(int position)
        {
            try
            {
                if (!ioListManager.IsDigitalSignal(position))
                {
                    // Parse range values
                    if (long.TryParse(ioListManager.EngRangeHigh[position], out long maxValue) &&
                        long.TryParse(ioListManager.EngRangeLow[position], out long minValue))
                    {
                        // Configure trackbar
                        analogTrackBar.Maximum = (int)maxValue;
                        analogTrackBar.Minimum = (int)minValue;

                        // Get current value from appropriate holding register
                        string serialAddress = ioListManager.SerialAddresses[position];
                        int address = int.Parse(serialAddress.Substring(1));
                        string protocol = ioListManager.GetProtocolForSignal(position);

                        long currentValue = modbusManager.SafeGetRegisterValue((ushort)address, protocol);

                        // Scale to engineering value
                        long busScaleLow = long.Parse(ioListManager.SerialLineLow[position]);
                        long busScaleHigh = long.Parse(ioListManager.SerialLineHigh[position]);
                        long scaledValue = minValue + ((currentValue - busScaleLow) * (maxValue - minValue) / (busScaleHigh - busScaleLow));

                        // Update trackbar and labels
                        analogTrackBar.Value = Math.Max((int)minValue, Math.Min((int)maxValue, (int)scaledValue));
                        lbMaxAnalogValueSlider.Text = maxValue.ToString();
                        lbMinAnalogValueSlider.Text = minValue.ToString();
                        lbAnalogValueSlider.Text = analogTrackBar.Value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting up analog trackbar: {ex.Message}");
            }
        }

        #endregion

        #region Event Handlers - Signal Value Manipulation

        /// <summary>
        /// Handles the Toggle button click event to toggle the bit value of a digital signal.
        /// </summary>
        private void btnToggleValue(object sender, EventArgs e)
        {
            try
            {
                // Get protocol for the selected signal
                string protocol = ioListManager.GetProtocolForSignal(selectedSignalIndex);

                // Check if simulator is running
                if (modbusManager.DataStore?.HoldingRegisters == null)
                {
                    MessageBox.Show("Simulator has not been started, please start the simulator so the holding registers are initialized",
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get the register address
                string registerAddressRaw = txtAddress.Text;

                // Parse the address
                int dotAddress = int.Parse(registerAddressRaw.Substring(registerAddressRaw.IndexOf(".") + 1));
                string registerAddressSplit = registerAddressRaw.Substring(0, registerAddressRaw.IndexOf("."));
                int registerAddress = int.Parse(registerAddressSplit.Substring(1));

                // Apply register offset if needed
                if (cbPlusRegister.Checked && !cbMinusRegister.Checked)
                {
                    registerAddress += 1;
                }
                else if (cbMinusRegister.Checked && !cbPlusRegister.Checked)
                {
                    registerAddress -= 1;
                }

                // Convert to register address
                ushort regAddress = (ushort)registerAddress;

                // Get current value from the appropriate datastore
                ushort currentValue = modbusManager.SafeGetRegisterValue(regAddress, protocol);

                // Create bitmask for the specific bit
                ushort mask = (ushort)(1 << dotAddress);

                // Toggle the bit using XOR operation
                ushort newValue = (ushort)(currentValue ^ mask);

                // Update the datastore with the new value
                modbusManager.SafeSetRegisterValue(regAddress, newValue, protocol);

                // Update the display
                UpdateSignalCurrentValue(selectedSignalIndex);
            }
            catch (FormatException)
            {
                MessageBox.Show("The address must be a valid number.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (OverflowException)
            {
                MessageBox.Show("The address is out of range.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Handles the holding value text box key press event to update register values.
        /// </summary>
        private void txtHoldingValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only process when Enter key is pressed
            if (e.KeyChar == (char)Keys.Enter)
            {
                try
                {
                    // Get the register address
                    string registerAddressRaw = txtAddress.Text;
                    string registerAddressSubstring;
                    int registerAddress = 0;

                    // Parse the address
                    if (registerAddressRaw.Contains(".")) //Parsing for digital signals
                    {
                        registerAddressSubstring = registerAddressRaw.Substring(0, registerAddressRaw.IndexOf("."));
                        registerAddress = int.Parse(registerAddressSubstring.Substring(1));
                    }
                    else //Parsing for analog signals
                    {
                        registerAddressSubstring = registerAddressRaw;
                        registerAddress = int.Parse(registerAddressSubstring.Substring(1));
                    }

                    // Apply register offset if needed
                    if (cbPlusRegister.Checked && !cbMinusRegister.Checked)
                    {
                        registerAddress += 1;
                    }
                    else if (cbMinusRegister.Checked && !cbPlusRegister.Checked)
                    {
                        registerAddress -= 1;
                    }

                    // Get the position in the IO list
                    int position = ioListManager.Tags.IndexOf(txtTag.Text);
                    if (position < 0)
                    {
                        MessageBox.Show("Tag not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Get the protocol for the signal
                    string protocol = ioListManager.GetProtocolForSignal(position);

                    // Get the address and value
                    ushort tagRegisterAddress = (ushort)registerAddress;
                    string addressValue = txtUserEngValue.Text;
                    long userValue = Convert.ToInt64(addressValue);
                    bool isBitAddress = registerAddressRaw.Contains(".");

                    // Get scaling values
                    long engScaleLow = long.Parse(txtEngLow.Text);
                    long engScaleHigh = long.Parse(txtEngHigh.Text);
                    long busScaleLow = long.Parse(txtSerialLow.Text);
                    long busScaleHigh = long.Parse(txtSerialHigh.Text);

                    // Calculate register value
                    ushort registerValue = 0;
                    ushort previousValue = 0;

                    // Check if simulator is running
                    if (modbusManager.IsRtuRunning || modbusManager.IsTcpRunning)
                    {
                        if (isBitAddress)
                        {
                            // Handle bit value update
                            int dotAddress = int.Parse(registerAddressRaw.Substring(registerAddressRaw.IndexOf(".") + 1));
                            BitCounter bittCounter = new BitCounter(dotAddress, (int)userValue);
                            registerValue = bittCounter.BitValue;
                            previousValue = modbusManager.SafeGetRegisterValue((ushort)registerAddress, protocol);

                            if (userValue == 1)
                            {
                                // Set bit high
                                registerValue = (ushort)(previousValue | registerValue);
                            }
                            else if (userValue == 0)
                            {
                                // Set bit low
                                ushort mask = (ushort)~(1 << dotAddress);
                                registerValue = (ushort)(previousValue & mask);
                            }
                        }
                        else
                        {
                            // Handle analog value update
                            long rawBusValue = busScaleLow + ((userValue - engScaleLow) * (busScaleHigh - busScaleLow) / (engScaleHigh - engScaleLow));

                            if (rawBusValue < 0)
                            {
                                // Handle negative values
                                rawBusValue = rawBusValue + 65536;
                            }

                            registerValue = (ushort)rawBusValue;
                        }

                        // Update appropriate datastore
                        modbusManager.SafeSetRegisterValue((ushort)registerAddress, registerValue, protocol);
                    }

                    e.Handled = true;
                }
                catch (FormatException)
                {
                    MessageBox.Show("The address must be a valid integer.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (OverflowException)
                {
                    MessageBox.Show("The address is too large or too small.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Unexpected Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Handles the holding value text change event to update raw bus value display.
        /// </summary>
        private void txtHoldingValue_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Clear raw bus value if holding value is empty
                if (string.IsNullOrWhiteSpace(txtUserEngValue.Text))
                {
                    txtRawBusValue.Clear();
                    return;
                }

                // Parse scaling values
                if (long.TryParse(txtSerialLow.Text, out long busScaleLow) &&
                    long.TryParse(txtEngLow.Text, out long engScaleLow) &&
                    long.TryParse(txtSerialHigh.Text, out long busScaleHigh) &&
                    long.TryParse(txtEngHigh.Text, out long engScaleHigh) &&
                    long.TryParse(txtUserEngValue.Text, out long userValue))
                {
                    // Calculate raw bus value using interpolation
                    long rawBusValue = busScaleLow + ((userValue - engScaleLow) * (busScaleHigh - busScaleLow) / (engScaleHigh - engScaleLow));
                    txtRawBusValue.Text = rawBusValue.ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating raw bus value: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the analog trackbar scroll event to update register values.
        /// </summary>
        private void analogTrackBar_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (selectedSignalIndex < 0 || ioListManager.IsDigitalSignal(selectedSignalIndex))
                    return;

                // Get the protocol for the signal
                string protocol = ioListManager.GetProtocolForSignal(selectedSignalIndex);

                // Get the value from the trackbar
                long analogValue = analogTrackBar.Value;

                // Parse the address, considering +1/-1 checkboxes
                string serialAddress = ioListManager.SerialAddresses[selectedSignalIndex];
                int baseAddress = int.Parse(serialAddress.Substring(1));

                // Apply register offset if needed
                int address = baseAddress;
                if (cbPlusRegister.Checked && !cbMinusRegister.Checked)
                {
                    address += 1;
                }
                else if (cbMinusRegister.Checked && !cbPlusRegister.Checked)
                {
                    address -= 1;
                }

                // Get scaling values
                long engScaleLow = long.Parse(ioListManager.EngRangeLow[selectedSignalIndex]);
                long engScaleHigh = long.Parse(ioListManager.EngRangeHigh[selectedSignalIndex]);
                long busScaleLow = long.Parse(ioListManager.SerialLineLow[selectedSignalIndex]);
                long busScaleHigh = long.Parse(ioListManager.SerialLineHigh[selectedSignalIndex]);

                // Calculate raw bus value using interpolation
                long rawBusValue = busScaleLow + ((analogValue - engScaleLow) * (busScaleHigh - busScaleLow) / (engScaleHigh - engScaleLow));

                // Handle negative values
                if (rawBusValue < 0)
                {
                    rawBusValue += 65536;
                }

                // Update the register value in the appropriate datastore
                modbusManager.SafeSetRegisterValue((ushort)address, (ushort)rawBusValue, protocol);

                //Convert back to raw bus value to update UI
                if (rawBusValue > 32768)
                {
                    rawBusValue -= 65536;
                }

                // Update the UI
                lbAnalogValueSlider.Text = analogValue.ToString();
                txtUserEngValue.Text = analogValue.ToString();
                txtRawBusValue.Text = rawBusValue.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating value from trackbar: {ex.Message}");
            }
        }

        #endregion

        #region Event Handlers - Test Results

        /// <summary>
        /// Handles the Result OK button click event to mark a signal as OK.
        /// </summary>
        private void btnResultOKClick(object sender, EventArgs e)
        {
            try
            {
                // Get the selected serial address
                string selectedSerialAddress = txtAddress.Text;

                if (string.IsNullOrEmpty(selectedSerialAddress))
                {
                    MessageBox.Show("No row selected", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Update the test result
                bool updated = ioListManager.UpdateTestResult(selectedSerialAddress, true);

                if (updated)
                {
                    // Update the list view item color
                    if (listViewSignals.SelectedItems.Count > 0)
                    {
                        ListViewItem selectedItem = listViewSignals.SelectedItems[0];
                        selectedItem.BackColor = Color.LimeGreen;
                    }

                    // Move to the next item
                    MoveToNextSignal();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error marking result as OK: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the Result Not OK button click event to mark a signal as Not OK.
        /// </summary>
        private void btnResultNotOKClick(object sender, EventArgs e)
        {
            try
            {
                // Get the selected serial address
                string selectedSerialAddress = txtAddress.Text;

                if (string.IsNullOrEmpty(selectedSerialAddress))
                {
                    MessageBox.Show("No row selected", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Update the test result
                bool updated = ioListManager.UpdateTestResult(selectedSerialAddress, false);

                if (updated)
                {
                    // Update the list view item color
                    if (listViewSignals.SelectedItems.Count > 0)
                    {
                        ListViewItem selectedItem = listViewSignals.SelectedItems[0];
                        selectedItem.BackColor = Color.Red;
                    }

                    // Move to the next item
                    MoveToNextSignal();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error marking result as Not OK: {ex.Message}");
            }
        }

        /// <summary>
        /// Moves to the next signal in the list view.
        /// </summary>
        private void MoveToNextSignal()
        {
            try
            {
                if (listViewSignals.SelectedItems.Count <= 0)
                    return;

                int currentIndex = listViewSignals.SelectedItems[0].Index;
                int nextIndex = currentIndex + 1;

                if (nextIndex < listViewSignals.Items.Count)
                {
                    // Update selection
                    listViewSignals.Items[currentIndex].Selected = false;
                    listViewSignals.Items[currentIndex].Focused = false;
                    listViewSignals.Items[nextIndex].Selected = true;
                    listViewSignals.Items[nextIndex].Focused = true;
                    listViewSignals.EnsureVisible(nextIndex);

                    // Update signal information
                    UpdateSignalInformation();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error moving to next signal: {ex.Message}");
            }
        }

        #endregion

        #region Event Handlers - Watchdog

        /// <summary>
        /// Handles the watchdog start button click event.
        /// </summary>
        private void btnWatchdogStart_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnWatchdogStart.Text == "Start")
                {
                    // Parse the interval
                    if (!int.TryParse(comboBoxWatchdogInterval.Text, out int interval))
                    {
                        MessageBox.Show("You must select a valid number for the interval.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Parse the address
                    if (!ushort.TryParse(txtWatchdogAddress.Text, out ushort address))
                    {
                        MessageBox.Show("The register address value is not valid, please input a valid address",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Set the watchdog address
                    watchdogManager.WatchdogAddress = address;
                    lastWatchdogAddress = address;

                    // Start the watchdog
                    bool started = watchdogManager.Start(interval);

                    if (started)
                    {
                        btnWatchdogStart.Text = "Stop";
                        txtWatchdogAddress.ReadOnly = true;
                    }
                }
                else
                {
                    lastWatchdogAddress = watchdogManager.WatchdogAddress;
                    // Stop the watchdog
                    watchdogManager.Stop();

                    // Reset UI
                    btnWatchdogStart.Text = "Start";
                    txtWatchdogAddress.ReadOnly = false;
                    txtWatchDog.Text = "0";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error managing watchdog: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles watchdog counter update events.
        /// </summary>
        private void OnWatchdogCounterUpdated(int counter)
        {
            try
            {
                //Invoking if needed to access UI thread
                if (txtWatchDog.InvokeRequired)
                {
                    txtWatchDog.Invoke(new Action(() =>
                    {
                        txtWatchDog.Text = counter.ToString();
                    }));
                }
                else
                {
                    txtWatchDog.Text = counter.ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating watchdog counter: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles watchdog alarm events.
        /// </summary>
        private void OnAlarmRaised(string message)
        {
            try
            {
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error displaying alarm: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles watchdog restart events.
        /// </summary>
        private void OnRestartNeeded(object sender, EventArgs e)
        {
            try
            {
                var settings = CommunicationSettings.Instance;
                modbusManager.RestartModbusSlaves(settings.TcpCommunicationEnabled, settings.RtuCommunicationEnabled);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error restarting Modbus slaves: {ex.Message}");
            }
        }

        #endregion

        #region Event Handlers - Miscellaneous

        /// <summary>
        /// Handles the Exit menu item click event.
        /// </summary>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the User Manual menu item click event.
        /// </summary>
        private void HelpUserManualClick(object sender, EventArgs e)
        {
            try
            {
                string pdfPath = Path.Combine(Application.StartupPath, "User-Manual.pdf");

                if (File.Exists(pdfPath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(pdfPath) { UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show("User manual not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening user manual: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles form closing event to clean up resources.
        /// </summary>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // Stop the watchdog
                if (watchdogManager != null)
                {
                    watchdogManager.Stop();
                }

                // Stop Modbus slaves
                if (modbusManager != null)
                {
                    modbusManager.StopModbusSlaves();
                }

                // Cancel delayed uncheck tasks
                rtuUncheckTokenSource?.Cancel();
                tcpUncheckTokenSource?.Cancel();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during form closing: {ex.Message}");
            }
        }

        #endregion

        #region ListView Drawing Methods

        /// <summary>
        /// Handles the ListView DrawItem event to customize item appearance.
        /// </summary>
        private void listViewSignals_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            try
            {
                // Clear the area to prevent artifacts
                e.Graphics.FillRectangle(new SolidBrush(listViewSignals.BackColor), e.Bounds);

                // Draw selected item with special styling
                if (e.Item.Selected)
                {
                    // Draw the background
                    using (SolidBrush backgroundBrush = new SolidBrush(e.Item.BackColor))
                    {
                        e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
                    }

                    // Draw selection border
                    Rectangle borderRect = new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
                    using (Pen highlightPen = new Pen(Color.DarkBlue, 6))
                    {
                        e.Graphics.DrawRectangle(highlightPen, borderRect);
                    }

                    // Draw the text
                    e.Graphics.DrawString(e.Item.Text, e.Item.Font, Brushes.Black, e.Bounds);
                }
                else
                {
                    // Draw normal item
                    using (SolidBrush backgroundBrush = new SolidBrush(e.Item.BackColor))
                    {
                        e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
                    }

                    e.Graphics.DrawString(e.Item.Text, e.Item.Font, new SolidBrush(e.Item.ForeColor), e.Bounds);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error drawing list view item: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the ListView DrawSubItem event to customize subitem appearance.
        /// </summary>
        private void listViewSignals_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            try
            {
                e.Graphics.DrawString(e.SubItem.Text, e.Item.Font, Brushes.Black, e.Bounds);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error drawing list view subitem: {ex.Message}");
            }
        }

        #endregion

        #region Private Methods - UI Updates

        /// <summary>
        /// Updates the txtUserEngValue text on the UI thread.
        /// </summary>
        /// <param name="value">The new value to display.</param>
        private void UpdateUserEngValueText(string value)
        {
            if (!IsHandleCreated || IsDisposed)
                return;

            try
            {
                //Invoke if required to access UI thread
                if (txtUserEngValue.InvokeRequired)
                {
                    BeginInvoke(new Action(() => {
                        if (!IsDisposed && txtUserEngValue != null)
                            txtUserEngValue.Text = value;
                    }));
                }
                else if (!IsDisposed && txtUserEngValue != null)
                {
                    txtUserEngValue.Text = value;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating txtUserEngValue: {ex.Message}");
            }
        }

        private CancellationTokenSource _cancelUncheckTokenSource;

        /// <summary>
        /// Starts a delayed task to uncheck the TCP connection indicator after 2 seconds of inactivity.
        /// </summary>
        private void StartTcpDelayedUncheck()
        {
            try
            {
                // Cancel any previous TCP uncheck task
                tcpUncheckTokenSource?.Cancel();
                tcpUncheckTokenSource = new CancellationTokenSource();
                var token = tcpUncheckTokenSource.Token;

                //Adding a delay of 2.5 seconds to ensure that checkbox for connected gets unchecked if no connection after 2 seconds
                Task.Delay(2500, token)
                    .ContinueWith(t =>
                    {
                        if (t.IsCanceled) return;

                        //Calculating time since last request
                        TimeSpan timeSinceLastTcp = DateTime.Now - lastCommunicationTimeTcp;

                        //If more than 2 seconds since last request uncheck the connected checkbox
                        if (timeSinceLastTcp.TotalSeconds >= 2)
                        {
                            if (!IsHandleCreated || IsDisposed) return;

                            try
                            {
                                BeginInvoke(new Action(() =>
                                {
                                    if (!IsDisposed && tcpCheckboxConnected != null)
                                    {
                                        tcpCheckboxConnected.Checked = false;
                                    }
                                }));
                            }
                            catch (ObjectDisposedException) { }
                        }
                    }, TaskScheduler.Default);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting TCP delayed uncheck: {ex.Message}");
            }
        }

        /// <summary>
        /// Starts a delayed task to uncheck the RTU connection indicator after 2 seconds of inactivity.
        /// </summary>
        private void StartRtuDelayedUncheck()
        {
            try
            {
                // Cancel any previous RTU uncheck task
                rtuUncheckTokenSource?.Cancel();
                rtuUncheckTokenSource = new CancellationTokenSource();
                var token = rtuUncheckTokenSource.Token;

                //Adding a delay of 2.5 seconds to ensure that checkbox for connected gets unchecked if no connection after 2 seconds
                Task.Delay(2500, token)
                    .ContinueWith(t =>
                    {
                        if (t.IsCanceled) return;

                        //Calculating time since last request
                        TimeSpan timeSinceLastRtu = DateTime.Now - lastCommunicationTimeRtu;

                        //If more than 2 seconds since last request uncheck the connected checkbox
                        if (timeSinceLastRtu.TotalSeconds >= 2)
                        {
                            if (!IsHandleCreated || IsDisposed) return;

                            try
                            {
                                BeginInvoke(new Action(() =>
                                {
                                    if (!IsDisposed)
                                    {
                                        rtuCheckboxConnected.Checked = false;
                                    }
                                }));
                            }
                            catch (ObjectDisposedException) { }
                        }
                    }, TaskScheduler.Default);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting RTU delayed uncheck: {ex.Message}");
            }
        }

        #endregion
    }
}