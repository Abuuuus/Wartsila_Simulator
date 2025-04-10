using System;
using System.IO.Ports;
using System.Net;

namespace Wartsila_Simulator.Settings
{
    /// <summary>
    /// Manages communication settings for both RTU and TCP protocols.
    /// Stores settings while the application is running.
    /// </summary>
    public class CommunicationSettings
    {
        #region Singleton Pattern

        private static CommunicationSettings _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets the singleton instance of CommunicationSettings
        /// </summary>
        public static CommunicationSettings Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new CommunicationSettings();
                    }
                    return _instance;
                }
            }
        }

        #endregion

        #region RTU Settings

        /// <summary>
        /// Gets or sets the COM port name
        /// </summary>
        public string ComPort { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Modbus slave ID
        /// </summary>
        public byte SlaveID { get; set; } = 1;

        /// <summary>
        /// Gets or sets the baud rate for serial communication
        /// </summary>
        public int BaudRate { get; set; } = 9600;

        /// <summary>
        /// Gets or sets the number of data bits for serial communication
        /// </summary>
        public int DataBits { get; set; } = 8;

        /// <summary>
        /// Gets or sets the stop bits for serial communication
        /// </summary>
        public StopBits StopBits { get; set; } = StopBits.One;

        /// <summary>
        /// Gets or sets the parity for serial communication
        /// </summary>
        public Parity Parity { get; set; } = Parity.None;

        #endregion

        #region TCP Settings

        /// <summary>
        /// Gets or sets the local IP address for TCP communication
        /// </summary>
        public string LocalIPAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets the server IP address as a byte array
        /// </summary>
        public byte[] ServerIPAddressBytes
        {
            get
            {
                if (string.IsNullOrEmpty(LocalIPAddress))
                    return new byte[4];

                try
                {
                    string[] ipParts = LocalIPAddress.Split('.');
                    byte[] ipBytes = new byte[4];
                    for (int i = 0; i < 4; i++)
                    {
                        ipBytes[i] = byte.Parse(ipParts[i]);
                    }
                    return ipBytes;
                }
                catch (Exception)
                {
                    // Return empty array if parsing fails
                    return new byte[4];
                }
            }
        }

        #endregion

        #region Communication Flags

        /// <summary>
        /// Gets or sets whether RTU communication is enabled
        /// </summary>
        public bool RtuCommunicationEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets whether TCP communication is enabled
        /// </summary>
        public bool TcpCommunicationEnabled { get; set; } = false;

        #endregion

        #region Methods

        /// <summary>
        /// Updates communication settings from the COM port settings dialog
        /// </summary>
        /// <param name="settings">The COM port settings dialog containing new settings</param>
        public void UpdateSettings(CommunicationSettingsUI settings)
        {
            if (settings == null)
                return;

            try
            {
                // Update RTU settings
                ComPort = settings.SelectedCOMPort;
                SlaveID = byte.Parse(settings.SlaveID);
                BaudRate = settings.SelectedBaudRate;
                DataBits = settings.SelectedDataBits;
                StopBits = settings.SelectedStopBits;
                Parity = settings.SelectedParity;

                // Update TCP settings
                LocalIPAddress = settings.LocalIPAddress;

                // Update communication flags
                RtuCommunicationEnabled = settings.rtuCommunication;
                TcpCommunicationEnabled = settings.tcpCommunication;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating communication settings: {ex.Message}",
                    "Settings Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Resets communication settings to default values
        /// </summary>
        public void ResetToDefaults()
        {
            ComPort = string.Empty;
            SlaveID = 1;
            BaudRate = 9600;
            DataBits = 8;
            StopBits = StopBits.One;
            Parity = Parity.None;
            LocalIPAddress = string.Empty;
            RtuCommunicationEnabled = false;
            TcpCommunicationEnabled = false;
        }

        #endregion
    }
}