using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Wartsila_Simulator.Settings;
using Modbus.Data;
using Modbus.Device;
using System.Diagnostics;

namespace Wartsila_Simulator.Managers
{
    /// <summary>
    /// Manages Modbus RTU and TCP communication.
    /// Handles initialization, starting, and stopping of Modbus slaves.
    /// </summary>
    public class ModbusManager : IDisposable
    {
        #region Events

        /// <summary>
        /// Event raised when a Modbus RTU request is received.
        /// </summary>
        public event EventHandler<ModbusSlaveRequestEventArgs> RtuRequestReceived;

        /// <summary>
        /// Event raised when a Modbus TCP request is received.
        /// </summary>
        public event EventHandler<ModbusSlaveRequestEventArgs> TcpRequestReceived;

        #endregion

        #region Private Fields

        private CancellationTokenSource rtuCts;
        private CancellationTokenSource tcpCts;
        private SerialPort serialPort;
        private TcpListener tcpListener;
        private ModbusTcpSlave tcpSlave;
        private ModbusSlave rtuSlave;

        // Separate datastores for RTU and TCP
        private DataStore rtuDataStore;
        private DataStore tcpDataStore;
        private DataStore dataStore; // For backward compatibility

        private bool isDisposed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the shared data store for Modbus registers (for backward compatibility).
        /// </summary>
        public DataStore DataStore => dataStore;

        /// <summary>
        /// Gets the data store for RTU Modbus registers.
        /// </summary>
        public DataStore RtuDataStore => rtuDataStore;

        /// <summary>
        /// Gets the data store for TCP Modbus registers.
        /// </summary>
        public DataStore TcpDataStore => tcpDataStore;

        /// <summary>
        /// Gets a value indicating whether the RTU slave is running.
        /// </summary>
        public bool IsRtuRunning => rtuSlave != null;

        /// <summary>
        /// Gets a value indicating whether the TCP slave is running.
        /// </summary>
        public bool IsTcpRunning => tcpSlave != null;

        #endregion

        #region Constructor and Initialization

        /// <summary>
        /// Initializes a new instance of the ModbusManager class.
        /// </summary>
        public ModbusManager()
        {
            // Initialize separate data stores for RTU and TCP
            rtuDataStore = DataStoreFactory.CreateDefaultDataStore();
            tcpDataStore = DataStoreFactory.CreateDefaultDataStore();

            // For backward compatibility
            dataStore = DataStoreFactory.CreateDefaultDataStore();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the appropriate DataStore based on the specified protocol.
        /// </summary>
        /// <param name="protocol">The Modbus protocol ("Modbus RTU" or "Modbus TCP").</param>
        /// <returns>The DataStore for the specified protocol, or the shared DataStore if the protocol is not recognized.</returns>
        public DataStore GetDataStoreByProtocol(string protocol)
        {
            //If the strings in IO-list gets changed at some point this needs to change to match it
            if (string.Equals(protocol, "Modbus RTU", StringComparison.OrdinalIgnoreCase))
                return rtuDataStore;
            else if (string.Equals(protocol, "Modbus TCP", StringComparison.OrdinalIgnoreCase))
                return tcpDataStore;
            else
            {
                Debug.WriteLine($"WARNING: Unknown protocol '{protocol}', defaulting to shared DataStore");
                return dataStore; // Default to shared DataStore for unknown protocols
            }
        }

        /// <summary>
        /// Starts the Modbus slave(s) based on the enabled communication types.
        /// </summary>
        /// <returns>True if at least one slave was started successfully, otherwise false.</returns>
        public bool StartModbusSlaves()
        {
            var settings = CommunicationSettings.Instance;
            bool anyStarted = false;

            try
            {
                if (settings.TcpCommunicationEnabled)
                {
                    bool tcpStarted = StartTcpSlave();
                    anyStarted |= tcpStarted;
                }

                if (settings.RtuCommunicationEnabled)
                {
                    bool rtuStarted = StartRtuSlave();
                    anyStarted |= rtuStarted;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting Modbus slaves: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return anyStarted;
        }

        /// <summary>
        /// Stops all running Modbus slaves.
        /// </summary>
        public void StopModbusSlaves()
        {
            try
            {
                StopTcpSlave();
                StopRtuSlave();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping Modbus slaves: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Restarts a specific Modbus slave or all slaves.
        /// </summary>
        /// <param name="restartTcp">True to restart TCP slave, otherwise false.</param>
        /// <param name="restartRtu">True to restart RTU slave, otherwise false.</param>
        public void RestartModbusSlaves(bool restartTcp, bool restartRtu)
        {
            try
            {
                StopModbusSlaves();

                if (restartTcp)
                {
                    StartTcpSlave();
                }

                if (restartRtu)
                {
                    StartRtuSlave();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error restarting Modbus slaves: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Safely gets a value from the appropriate holding registers with bounds checking.
        /// </summary>
        /// <param name="address">The register address to access.</param>
        /// <param name="protocol">The Modbus protocol ("Modbus RTU" or "Modbus TCP"). If null, uses the shared DataStore for backward compatibility.</param>
        /// <returns>The register value, or 0 if the address is out of range.</returns>
        public ushort SafeGetRegisterValue(ushort address, string protocol = null)
        {
            try
            {
                // Determine which DataStore to use
                DataStore targetDataStore = protocol != null
                    ? GetDataStoreByProtocol(protocol)
                    : dataStore;

                lock (targetDataStore)
                {
                    if (address < targetDataStore.HoldingRegisters.Count)
                    {
                        return targetDataStore.HoldingRegisters[address];
                    }
                    Debug.WriteLine($"WARNING: Attempted to access out-of-range register: {address} for {protocol ?? "unknown protocol"}");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error accessing register {address} for {protocol ?? "unknown protocol"}: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Safely sets a value in the appropriate holding registers with bounds checking.
        /// </summary>
        /// <param name="address">The register address to access.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="protocol">The Modbus protocol ("Modbus RTU" or "Modbus TCP"). If null, uses the shared DataStore for backward compatibility.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool SafeSetRegisterValue(ushort address, ushort value, string protocol = null)
        {
            try
            {
                bool success = false;

                // If protocol is specified, update the corresponding DataStore
                if (protocol != null)
                {
                    DataStore targetDataStore = GetDataStoreByProtocol(protocol);
                    lock (targetDataStore)
                    {
                        if (address < targetDataStore.HoldingRegisters.Count)
                        {
                            targetDataStore.HoldingRegisters[address] = value;
                            success = true;
                        }
                        else
                        {
                            Debug.WriteLine($"WARNING: Register address {address} is out of range for {protocol} DataStore");
                        }
                    }
                }
                else
                {
                    // Update all datastores to maintain consistency when no specific protocol is specified
                    lock (dataStore)
                    {
                        if (address < dataStore.HoldingRegisters.Count)
                        {
                            dataStore.HoldingRegisters[address] = value;
                            success = true;
                        }
                        else
                        {
                            Debug.WriteLine($"WARNING: Register address {address} is out of range for shared DataStore");
                        }
                    }
                }

                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting register {address} for {protocol ?? "unknown protocol"}: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Starts the Modbus TCP slave.
        /// </summary>
        /// <returns>True if started successfully, otherwise false.</returns>
        private bool StartTcpSlave()
        {
            try
            {
                StopTcpSlave();

                var settings = CommunicationSettings.Instance;
                var ipBytes = settings.ServerIPAddressBytes;

                if (ipBytes == null || ipBytes.Length != 4)
                {
                    MessageBox.Show("Invalid IP address format. Please check communication settings.",
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                IPAddress address = new IPAddress(ipBytes);
                int port = 502; // Standard Modbus TCP port

                // Initialize cancellation token source
                tcpCts = new CancellationTokenSource();

                // Start the TCP listener
                tcpListener = new TcpListener(address, port);
                tcpListener.Start();

                // Create the Modbus TCP slave
                tcpSlave = ModbusTcpSlave.CreateTcp(settings.SlaveID, tcpListener);
                tcpSlave.DataStore = tcpDataStore; // Use TCP DataStore
                tcpSlave.ModbusSlaveRequestReceived += OnTcpSlaveRequestReceived;

                // Listen for incoming requests on a separate thread
                Task.Run(() =>
                {
                    try
                    {
                        tcpSlave.Listen();
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected during shutdown, no need to show error
                    }
                    catch (Exception ex)
                    {
                        // Only show message box if not disposed or cancellation requested
                        if (!isDisposed && (tcpCts == null || !tcpCts.IsCancellationRequested))
                        {
                            MessageBox.Show($"TCP Slave error: {ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                });

                return true;
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"Socket error starting TCP slave: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting TCP slave: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Starts the Modbus RTU slave.
        /// </summary>
        /// <returns>True if started successfully, otherwise false.</returns>
        private bool StartRtuSlave()
        {
            try
            {
                StopRtuSlave();

                var settings = CommunicationSettings.Instance;

                if (string.IsNullOrEmpty(settings.ComPort))
                {
                    MessageBox.Show("COM port not configured.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // Create and open the serial port
                serialPort = new SerialPort(settings.ComPort)
                {
                    BaudRate = settings.BaudRate,
                    DataBits = settings.DataBits,
                    Parity = settings.Parity,
                    StopBits = settings.StopBits
                };
                serialPort.Open();

                // Create the RTU slave
                rtuSlave = ModbusSerialSlave.CreateRtu(settings.SlaveID, serialPort);
                rtuSlave.DataStore = rtuDataStore; // Use RTU DataStore
                rtuSlave.ModbusSlaveRequestReceived += OnRtuSlaveRequestReceived;

                rtuCts = new CancellationTokenSource();

                // Listen for incoming requests on a separate thread
                Task.Run(() =>
                {
                    try
                    {
                        rtuSlave.Listen();
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected during shutdown, no need to show error
                    }
                    catch (Exception ex)
                    {
                        // Only show message box if not disposed or cancellation requested
                        if (!isDisposed && (rtuCts == null || !rtuCts.IsCancellationRequested))
                        {
                            MessageBox.Show($"RTU Slave error: {ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting RTU slave: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Stops the Modbus TCP slave.
        /// </summary>
        private void StopTcpSlave()
        {
            try
            {
                // Cancel the listening task
                if (tcpCts != null)
                {
                    tcpCts.Cancel();
                    tcpCts.Dispose();
                    tcpCts = null;
                }

                // Dispose the Modbus TCP slave
                if (tcpSlave != null)
                {
                    tcpSlave.ModbusSlaveRequestReceived -= OnTcpSlaveRequestReceived;
                    tcpSlave.Dispose();
                    tcpSlave = null;
                }

                // Stop the TCP listener
                if (tcpListener != null)
                {
                    tcpListener.Stop();
                    tcpListener = null;
                }
            }
            catch (SocketException ex)
            {
                if (!isDisposed)
                {
                    Debug.WriteLine($"Socket error stopping TCP slave: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                if (!isDisposed)
                {
                    Debug.WriteLine($"Error stopping TCP slave: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Stops the Modbus RTU slave.
        /// </summary>
        private void StopRtuSlave()
        {
            try
            {
                // Cancel the listening task
                if (rtuCts != null)
                {
                    rtuCts.Cancel();
                    rtuCts.Dispose();
                    rtuCts = null;
                }

                // Dispose the Modbus RTU slave
                if (rtuSlave != null)
                {
                    rtuSlave.ModbusSlaveRequestReceived -= OnRtuSlaveRequestReceived;
                    rtuSlave.Dispose();
                    rtuSlave = null;
                }

                // Close and dispose the serial port
                if (serialPort != null)
                {
                    if (serialPort.IsOpen)
                    {
                        serialPort.Close();
                    }
                    serialPort.Dispose();
                    serialPort = null;
                }
            }
            catch (Exception ex)
            {
                if (!isDisposed)
                {
                    Debug.WriteLine($"Error stopping RTU slave: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Handles Modbus RTU slave request received events.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments containing the Modbus request details.</param>
        private void OnRtuSlaveRequestReceived(object sender, ModbusSlaveRequestEventArgs e)
        {
            try
            {
                RtuRequestReceived?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling RTU slave request: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles Modbus TCP slave request received events.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments containing the Modbus request details.</param>
        private void OnTcpSlaveRequestReceived(object sender, ModbusSlaveRequestEventArgs e)
        {
            try
            {
                TcpRequestReceived?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling TCP slave request: {ex.Message}");
            }
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Disposes resources used by the ModbusManager.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes resources used by the ModbusManager.
        /// </summary>
        /// <param name="disposing">True if called from Dispose(), false if called from finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
                return;

            if (disposing)
            {
                // Stop all Modbus slaves
                StopTcpSlave();
                StopRtuSlave();
            }

            isDisposed = true;
        }

        #endregion
    }
}