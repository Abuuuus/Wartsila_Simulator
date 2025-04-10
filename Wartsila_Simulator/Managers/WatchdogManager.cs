using System;
using System.Windows.Forms;
using Modbus.Data;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Wartsila_Simulator.Managers
{
    /// <summary>
    /// Delegate for watchdog counter update events.
    /// </summary>
    /// <param name="counter">The current watchdog counter value.</param>
    public delegate void WatchdogCounterUpdatedEventHandler(int counter);

    /// <summary>
    /// Delegate for alarm notification events.
    /// </summary>
    /// <param name="message">The alarm message.</param>
    public delegate void AlarmNotificationEventHandler(string message);

    /// <summary>
    /// Manages the watchdog functionality for detecting and handling communication problems.
    /// </summary>
    public class WatchdogManager
    {
        #region Events

        /// <summary>
        /// Event raised when the watchdog counter is updated.
        /// </summary>
        public event WatchdogCounterUpdatedEventHandler CounterUpdated;

        /// <summary>
        /// Event raised before watchdog updates a register.
        /// </summary>
        public event EventHandler WatchdogUpdateStarting;

        /// <summary>
        /// Event raised after watchdog updates a register.
        /// </summary>
        public event EventHandler WatchdogUpdateCompleted;

        /// <summary>
        /// Event raised when an alarm condition is detected.
        /// </summary>
        public event AlarmNotificationEventHandler AlarmRaised;

        /// <summary>
        /// Event raised when a restart is needed.
        /// </summary>
        public event EventHandler RestartNeeded;

        #endregion

        #region Private Fields

        private readonly ModbusManager modbusManager;
        private readonly System.Windows.Forms.Timer watchdogTimer;
        private readonly System.Windows.Forms.Timer restartTimer;

        private DateTime lastCommunicationRtu;
        private DateTime lastCommunicationTcp;

        private int watchdogCounter;
        private ushort watchdogAddress;
        private bool isRunning;
        private bool isRestarting;
        private bool alarmDisplayed;
        private int restartAttempts;

        private bool rtuRequestReceived;
        private bool tcpRequestReceived;

        private const int CommunicationTimeout = 5000; // 5 seconds
        private const int RestartDelay = 3000; // 3 seconds
        private const int MaxRestartAttempts = 3;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the watchdog counter value.
        /// </summary>
        public int WatchdogCounter
        {
            get => watchdogCounter;
            private set
            {
                watchdogCounter = value;
                CounterUpdated?.Invoke(watchdogCounter);
            }
        }

        /// <summary>
        /// Gets or sets the watchdog register address.
        /// </summary>
        public ushort WatchdogAddress
        {
            get => watchdogAddress;
            set => watchdogAddress = value;
        }

        /// <summary>
        /// Gets a value indicating whether the watchdog is running.
        /// </summary>
        public bool IsRunning => isRunning;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the WatchdogManager class.
        /// </summary>
        /// <param name="modbusManager">The ModbusManager instance.</param>
        public WatchdogManager(ModbusManager modbusManager)
        {
            this.modbusManager = modbusManager ?? throw new ArgumentNullException(nameof(modbusManager));

            // Initialize timers
            watchdogTimer = new System.Windows.Forms.Timer();
            watchdogTimer.Tick += WatchdogTimer_Tick;

            restartTimer = new System.Windows.Forms.Timer();
            restartTimer.Interval = RestartDelay;
            restartTimer.Tick += RestartTimer_Tick;

            // Subscribe to Modbus events
            this.modbusManager.RtuRequestReceived += OnRtuRequestReceived;
            this.modbusManager.TcpRequestReceived += OnTcpRequestReceived;

            // Initialize timestamps
            lastCommunicationRtu = DateTime.Now;
            lastCommunicationTcp = DateTime.Now;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the watchdog with the specified interval.
        /// </summary>
        /// <param name="intervalMs">The watchdog interval in milliseconds.</param>
        /// <returns>True if started successfully, otherwise false.</returns>
        public bool Start(int intervalMs)
        {
            if (!ValidateSettings())
                return false;

            try
            {
                // Reset counter and flags
                WatchdogCounter = 0;
                isRunning = true;
                isRestarting = false;
                alarmDisplayed = false;
                restartAttempts = 0;
                rtuRequestReceived = false;
                tcpRequestReceived = false;

                // Update timestamps
                lastCommunicationRtu = DateTime.Now;
                lastCommunicationTcp = DateTime.Now;

                // Start the timer
                watchdogTimer.Interval = intervalMs;
                watchdogTimer.Start();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting watchdog: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Stops the watchdog.
        /// </summary>
        public void Stop()
        {
            try
            {
                watchdogTimer.Stop();
                restartTimer.Stop();
                isRunning = false;

                // Reset counter
                WatchdogCounter = 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping watchdog: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets the RTU request received flag.
        /// </summary>
        public void SetRtuRequestReceived()
        {
            rtuRequestReceived = true;
            lastCommunicationRtu = DateTime.Now;
        }

        /// <summary>
        /// Sets the TCP request received flag.
        /// </summary>
        public void SetTcpRequestReceived()
        {
            tcpRequestReceived = true;
            lastCommunicationTcp = DateTime.Now;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Validates the watchdog settings.
        /// </summary>
        /// <returns>True if settings are valid, otherwise false.</returns>
        private bool ValidateSettings()
        {
            // Check if either RTU or TCP slaves are running
            if (!modbusManager.IsRtuRunning && !modbusManager.IsTcpRunning)
            {
                MessageBox.Show("Cannot start the watchdog. Communication channels have not been opened.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Handles Modbus RTU request received events.
        /// </summary>
        private void OnRtuRequestReceived(object sender, Modbus.Device.ModbusSlaveRequestEventArgs e)
        {
            SetRtuRequestReceived();
        }

        /// <summary>
        /// Handles Modbus TCP request received events.
        /// </summary>
        private void OnTcpRequestReceived(object sender, Modbus.Device.ModbusSlaveRequestEventArgs e)
        {
            SetTcpRequestReceived();
        }

        /// <summary>
        /// Handles the watchdog timer tick event.
        /// </summary>
        private void WatchdogTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                ProcessWatchdogTick();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in watchdog timer: {ex.Message}");
            }
            finally
            {
                // Reset flags after processing
                rtuRequestReceived = false;
                tcpRequestReceived = false;
            }
        }

        /// <summary>
        /// Processes the watchdog tick logic.
        /// </summary>
        private void ProcessWatchdogTick()
        {
            var settings = Settings.CommunicationSettings.Instance;
            bool rtuEnabled = settings.RtuCommunicationEnabled;
            bool tcpEnabled = settings.TcpCommunicationEnabled;
            bool communicationOk = false;

            // Check communication status based on enabled protocols
            if (rtuEnabled && !tcpEnabled)
            {
                communicationOk = rtuRequestReceived;
            }
            else if (tcpEnabled && !rtuEnabled)
            {
                communicationOk = tcpRequestReceived;
            }
            else if (rtuEnabled && tcpEnabled)
            {
                communicationOk = rtuRequestReceived && tcpRequestReceived;
            }

            if (communicationOk)
            {
                // Communication is OK, increment counter and update register
                IncrementWatchdogCounter();

                // Reset alarm and restart flags
                alarmDisplayed = false;
                isRestarting = false;
                restartAttempts = 0;
                restartTimer.Stop();
            }
            else
            {
                // Check for communication timeout
                bool rtuTimeout = rtuEnabled && (DateTime.Now - lastCommunicationRtu).TotalMilliseconds > CommunicationTimeout;
                bool tcpTimeout = tcpEnabled && (DateTime.Now - lastCommunicationTcp).TotalMilliseconds > CommunicationTimeout;

                if ((rtuEnabled && rtuTimeout) || (tcpEnabled && tcpTimeout))
                {
                    HandleCommunicationTimeout();
                }
            }
        }

        /// <summary>
        /// Increments the watchdog counter and updates the registers in both RTU and TCP datastores.
        /// </summary>
        public void IncrementWatchdogCounter()
        {
            try
            {
                // Signal that this is a watchdog update
                WatchdogUpdateStarting?.Invoke(this, EventArgs.Empty);

                // Increment counter
                WatchdogCounter++;

                // Update the register in both datastores to ensure consistency
                modbusManager.SafeSetRegisterValue(watchdogAddress, (ushort)WatchdogCounter, "Modbus RTU");
                modbusManager.SafeSetRegisterValue(watchdogAddress, (ushort)WatchdogCounter, "Modbus TCP");

                // Also update the legacy datastore for backward compatibility
                modbusManager.SafeSetRegisterValue(watchdogAddress, (ushort)WatchdogCounter);

                // Signal that the watchdog update is complete
                WatchdogUpdateCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating watchdog register: {ex.Message}");
            }
        }


        /// <summary>
        /// Handles communication timeout conditions.
        /// </summary>
        private void HandleCommunicationTimeout()
        {
            if (!isRestarting && restartAttempts < MaxRestartAttempts)
            {
                // Attempt restart
                isRestarting = true;
                restartAttempts++;

                // Notify that restart is needed
                RestartNeeded?.Invoke(this, EventArgs.Empty);

                // Start restart timer
                restartTimer.Start();

                // Update timestamps
                lastCommunicationRtu = DateTime.Now;
                lastCommunicationTcp = DateTime.Now;
            }
            else if (restartAttempts >= MaxRestartAttempts && !alarmDisplayed)
            {
                // Maximum restart attempts reached, show alarm
                alarmDisplayed = true;

                string alarmMessage = "Alarm: No Modbus requests received after multiple restart attempts.";
                AlarmRaised?.Invoke(alarmMessage);
            }
        }

        /// <summary>
        /// Handles the restart timer tick event.
        /// </summary>
        private void RestartTimer_Tick(object sender, EventArgs e)
        {
            if (isRestarting)
            {
                isRestarting = false;
                restartTimer.Stop();
            }
        }

        #endregion
    }
}