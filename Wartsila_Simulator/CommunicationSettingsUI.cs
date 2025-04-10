using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Forms;
using Wartsila_Simulator.Settings;

namespace Wartsila_Simulator
{
    /// <summary>
    /// Dialog for configuring COM port and network settings for Modbus communication.
    /// </summary>
    public class CommunicationSettingsUI : Form
    {
        #region Properties

        /// <summary>
        /// Gets the selected slave ID.
        /// </summary>
        public string SlaveID { get; private set; }

        /// <summary>
        /// Gets the selected COM port.
        /// </summary>
        public string SelectedCOMPort { get; private set; }

        /// <summary>
        /// Gets the selected baud rate.
        /// </summary>
        public int SelectedBaudRate { get; private set; }

        /// <summary>
        /// Gets the selected data bits.
        /// </summary>
        public int SelectedDataBits { get; private set; }

        /// <summary>
        /// Gets the selected stop bits.
        /// </summary>
        public StopBits SelectedStopBits { get; private set; }

        /// <summary>
        /// Gets the selected parity.
        /// </summary>
        public Parity SelectedParity { get; private set; }

        /// <summary>
        /// Gets the selected local IP address.
        /// </summary>
        public string LocalIPAddress { get; private set; }

        /// <summary>
        /// Gets a value indicating whether RTU communication is enabled.
        /// </summary>
        public bool rtuCommunication { get; private set; }

        /// <summary>
        /// Gets a value indicating whether TCP communication is enabled.
        /// </summary>
        public bool tcpCommunication { get; private set; }

        #endregion

        #region Private Fields

        private TextBox slaveIdTextBox;
        private ComboBox comPortComboBox;
        private ComboBox baudRateComboBox;
        private ComboBox dataBitsComboBox;
        private ComboBox stopBitsComboBox;
        private ComboBox parityComboBox;
        private TextBox ipAddressTextBox;
        private TextBox newIpAddressTextBox;
        private CheckBox rtuCheck;
        private CheckBox tcpCheck;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the COMPortSettingsMessageBox class.
        /// </summary>
        public CommunicationSettingsUI()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes the form components.
        /// </summary>
        private void InitializeComponent()
        {
            // Configure form properties
            this.Text = "Settings";
            this.ClientSize = new Size(550, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Fixed3D;

            // Create slave ID controls
            Label slaveIdLabel = new Label();
            slaveIdLabel.Text = "Slave ID:";
            slaveIdLabel.Location = new Point(10, 40);
            slaveIdLabel.AutoSize = true;

            slaveIdTextBox = new TextBox();
            slaveIdTextBox.Location = new Point(90, 40);
            slaveIdTextBox.Width = 200;
            slaveIdTextBox.Text = "1"; // Default value

            // Create COM port controls
            Label comPortLabel = new Label();
            comPortLabel.Text = "COM Port:";
            comPortLabel.Location = new Point(10, 10);
            comPortLabel.AutoSize = true;

            comPortComboBox = new ComboBox();
            comPortComboBox.Location = new Point(90, 10);
            comPortComboBox.Width = 350;
            comPortComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            PopulateComPortsList();

            // Create baud rate controls
            Label baudRateLabel = new Label();
            baudRateLabel.Text = "Baud Rate:";
            baudRateLabel.Location = new Point(10, 70);
            baudRateLabel.AutoSize = true;

            baudRateComboBox = new ComboBox();
            baudRateComboBox.Location = new Point(90, 70);
            baudRateComboBox.Width = 200;
            baudRateComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            baudRateComboBox.Items.AddRange(new object[] { "300", "600", "1200", "2400", "4800", "9600", "19200", "38400", "57600", "115200" });
            baudRateComboBox.SelectedItem = "9600"; // Default value

            // Create data bits controls
            Label dataBitsLabel = new Label();
            dataBitsLabel.Text = "Data Bits:";
            dataBitsLabel.Location = new Point(10, 100);
            dataBitsLabel.AutoSize = true;

            dataBitsComboBox = new ComboBox();
            dataBitsComboBox.Location = new Point(90, 100);
            dataBitsComboBox.Width = 200;
            dataBitsComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            dataBitsComboBox.Items.AddRange(new object[] { "7", "8" });
            dataBitsComboBox.SelectedItem = "8"; // Default value

            // Create stop bits controls
            Label stopBitsLabel = new Label();
            stopBitsLabel.Text = "Stop Bits:";
            stopBitsLabel.Location = new Point(10, 130);
            stopBitsLabel.AutoSize = true;

            stopBitsComboBox = new ComboBox();
            stopBitsComboBox.Location = new Point(90, 130);
            stopBitsComboBox.Width = 200;
            stopBitsComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            stopBitsComboBox.Items.AddRange(new object[] { "1", "2" });
            stopBitsComboBox.SelectedItem = "1"; // Default value

            // Create parity controls
            Label parityLabel = new Label();
            parityLabel.Text = "Parity:";
            parityLabel.Location = new Point(10, 160);
            parityLabel.AutoSize = true;

            parityComboBox = new ComboBox();
            parityComboBox.Location = new Point(90, 160);
            parityComboBox.Width = 200;
            parityComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            parityComboBox.Items.AddRange(Enum.GetNames(typeof(Parity)));
            parityComboBox.SelectedItem = "None"; // Default value

            // Create IP address controls
            Label ipAddressLabel = new Label();
            ipAddressLabel.Text = "Local IP Address:";
            ipAddressLabel.Location = new Point(10, 200);
            ipAddressLabel.AutoSize = true;

            ipAddressTextBox = new TextBox();
            ipAddressTextBox.Location = new Point(130, 200);
            ipAddressTextBox.Width = 160;
            ipAddressTextBox.ReadOnly = true;
            string localIp = GetLocalIPAddress();
            ipAddressTextBox.Text = localIp; // Gets the local IP address

            Label newIpAddressLabel = new Label();
            newIpAddressLabel.Text = "Set New IP:";
            newIpAddressLabel.Location = new Point(10, 240);
            newIpAddressLabel.AutoSize = true;

            Label newIpAddressLabelOptional = new Label();
            newIpAddressLabelOptional.Text = "Optional if no local address found";
            newIpAddressLabelOptional.Location = new Point(10, 275);
            newIpAddressLabelOptional.AutoSize = true;

            newIpAddressTextBox = new TextBox();
            newIpAddressTextBox.Location = new Point(100, 240);
            newIpAddressTextBox.Width = 160;
            newIpAddressTextBox.PlaceholderText = "xxx.xxx.xxx.xxx"; // Placeholder to display the format

            // Create protocol selection controls
            rtuCheck = new CheckBox();
            rtuCheck.Height = 30;
            rtuCheck.Width = 20;
            rtuCheck.Location = new Point(460, 40);

            Label rtuCheckLabel = new Label();
            rtuCheckLabel.Text = "RTU";
            rtuCheckLabel.Location = new Point(500, 40);
            rtuCheckLabel.AutoSize = true;

            tcpCheck = new CheckBox();
            tcpCheck.Height = 30;
            tcpCheck.Width = 20;
            tcpCheck.Location = new Point(460, 80);

            Label tcpCheckLabel = new Label();
            tcpCheckLabel.Text = "TCP";
            tcpCheckLabel.Location = new Point(500, 80);
            tcpCheckLabel.AutoSize = true;

            // Create OK button
            Button okButton = new Button();
            okButton.Text = "OK";
            okButton.Height = 30;
            okButton.Location = new Point(300, 200);
            okButton.Click += OkButton_Click;

            // Create Cancel button
            Button cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Height = 30;
            cancelButton.Location = new Point(300, 250);
            cancelButton.Click += (sender, e) => this.Close();

            // Add all controls to the form
            this.Controls.Add(slaveIdLabel);
            this.Controls.Add(slaveIdTextBox);
            this.Controls.Add(comPortLabel);
            this.Controls.Add(comPortComboBox);
            this.Controls.Add(baudRateLabel);
            this.Controls.Add(baudRateComboBox);
            this.Controls.Add(dataBitsLabel);
            this.Controls.Add(dataBitsComboBox);
            this.Controls.Add(stopBitsLabel);
            this.Controls.Add(stopBitsComboBox);
            this.Controls.Add(parityLabel);
            this.Controls.Add(parityComboBox);
            this.Controls.Add(ipAddressLabel);
            this.Controls.Add(ipAddressTextBox);
            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);
            this.Controls.Add(newIpAddressLabel);
            this.Controls.Add(newIpAddressLabelOptional);
            this.Controls.Add(newIpAddressTextBox);
            this.Controls.Add(rtuCheck);
            this.Controls.Add(tcpCheck);
            this.Controls.Add(rtuCheckLabel);
            this.Controls.Add(tcpCheckLabel);
        }

        /// <summary>
        /// Populates the COM ports list with available ports.
        /// </summary>
        private void PopulateComPortsList()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%(COM%'");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string name = obj["Name"]?.ToString();
                    if (!string.IsNullOrEmpty(name))
                    {
                        comPortComboBox.Items.Add(name);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving COM ports: {ex.Message}",
                    null, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Gets the local IP address of the Ethernet adapter.
        /// </summary>
        /// <returns>The local IP address as a string, or empty string if not found.</returns>
        private string GetLocalIPAddress()
        {
            try
            {
                foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                        networkInterface.OperationalStatus == OperationalStatus.Up &&
                        !networkInterface.Description.Contains("Virtual") &&
                        !networkInterface.Name.Contains("VMware") &&
                        !networkInterface.Description.Contains("Hyper-V"))
                    {
                        var ipProperties = networkInterface.GetIPProperties();
                        foreach (var ip in ipProperties.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                return ip.Address.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting local IP address: {ex.Message}");
            }

            return "";
        }

        /// <summary>
        /// Loads the current settings from the settings manager.
        /// </summary>
        private void LoadCurrentSettings()
        {
            try
            {
                var settings = CommunicationSettings.Instance;

                // Set slave ID
                if (!string.IsNullOrEmpty(settings.SlaveID.ToString()))
                {
                    slaveIdTextBox.Text = settings.SlaveID.ToString();
                }

                // Set COM port if it exists in the list
                if (!string.IsNullOrEmpty(settings.ComPort))
                {
                    foreach (var item in comPortComboBox.Items)
                    {
                        if (item.ToString().Contains(settings.ComPort))
                        {
                            comPortComboBox.SelectedItem = item;
                            break;
                        }
                    }
                }

                // Set baud rate
                if (settings.BaudRate > 0 && baudRateComboBox.Items.Contains(settings.BaudRate.ToString()))
                {
                    baudRateComboBox.SelectedItem = settings.BaudRate.ToString();
                }

                // Set data bits
                if (settings.DataBits > 0 && dataBitsComboBox.Items.Contains(settings.DataBits.ToString()))
                {
                    dataBitsComboBox.SelectedItem = settings.DataBits.ToString();
                }

                // Set stop bits
                string stopBitsStr = ((int)settings.StopBits).ToString();
                if (stopBitsComboBox.Items.Contains(stopBitsStr))
                {
                    stopBitsComboBox.SelectedItem = stopBitsStr;
                }

                // Set parity
                string parityStr = settings.Parity.ToString();
                if (parityComboBox.Items.Contains(parityStr))
                {
                    parityComboBox.SelectedItem = parityStr;
                }

                // Set communication flags
                rtuCheck.Checked = settings.RtuCommunicationEnabled;
                tcpCheck.Checked = settings.TcpCommunicationEnabled;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading settings: {ex.Message}");
                // Continue with default values if there's an error
            }
        }

        /// <summary>
        /// Handles the OK button click event.
        /// </summary>
        private void OkButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Set the slave ID
                SlaveID = slaveIdTextBox.Text;

                // Get the COM port if selected
                if (comPortComboBox.SelectedItem != null)
                {
                    string selectedCOMPortRaw = comPortComboBox.SelectedItem.ToString();
                    int startLocation = selectedCOMPortRaw.IndexOf("(") + 1;
                    int endLocation = selectedCOMPortRaw.IndexOf(")");

                    if (startLocation > 0 && endLocation > startLocation)
                    {
                        string selectedCOMPortName = selectedCOMPortRaw.Substring(startLocation, endLocation - startLocation);
                        SelectedCOMPort = selectedCOMPortName;

                        // Parse numeric values
                        SelectedBaudRate = int.Parse(baudRateComboBox.SelectedItem.ToString());
                        SelectedDataBits = int.Parse(dataBitsComboBox.SelectedItem.ToString());
                        SelectedStopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBitsComboBox.SelectedItem.ToString());
                        SelectedParity = (Parity)Enum.Parse(typeof(Parity), parityComboBox.SelectedItem.ToString());
                    }
                }

                // Set communication flags
                rtuCommunication = rtuCheck.Checked;
                tcpCommunication = tcpCheck.Checked;

                // Set the IP address
                if (tcpCheck.Checked)
                {
                    if (string.IsNullOrEmpty(newIpAddressTextBox.Text))
                    {
                        LocalIPAddress = ipAddressTextBox.Text;
                    }
                    else
                    {
                        LocalIPAddress = newIpAddressTextBox.Text;
                    }
                }

                // Update the global settings
                UpdateGlobalSettings();

                // Close the dialog with OK result
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying settings: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Updates the global communication settings.
        /// </summary>
        private void UpdateGlobalSettings()
        {
            var settings = CommunicationSettings.Instance;

            settings.SlaveID = byte.Parse(SlaveID);
            settings.ComPort = SelectedCOMPort;

            if (SelectedBaudRate > 0)
                settings.BaudRate = SelectedBaudRate;

            if (SelectedDataBits > 0)
                settings.DataBits = SelectedDataBits;

            settings.StopBits = SelectedStopBits;
            settings.Parity = SelectedParity;
            settings.RtuCommunicationEnabled = rtuCommunication;
            settings.TcpCommunicationEnabled = tcpCommunication;
        }

        #endregion
    }
}