using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Modbus.Data;

namespace Wartsila_Simulator.Managers
{
    /// <summary>
    /// Manages IO list data imported from Access databases.
    /// Handles loading, storing, and updating IO list information.
    /// </summary>
    public class IOListManager
    {
        #region Properties

        /// <summary>
        /// Gets the list of serial line names.
        /// </summary>
        public List<string> SerialLineNames { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the list of tags.
        /// </summary>
        public List<string> Tags { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the list of descriptions.
        /// </summary>
        public List<string> Descriptions { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the list of engineering units.
        /// </summary>
        public List<string> EngUnits { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the list of engineering range low values.
        /// </summary>
        public List<string> EngRangeLow { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the list of engineering range high values.
        /// </summary>
        public List<string> EngRangeHigh { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the list of serial addresses.
        /// </summary>
        public List<string> SerialAddresses { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the list of serial line low values.
        /// </summary>
        public List<string> SerialLineLow { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the list of serial line high values.
        /// </summary>
        public List<string> SerialLineHigh { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the list of verified test results.
        /// </summary>
        public List<string> VerifiedTests { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the list of loop typical values.
        /// </summary>
        public List<string> LoopTypicals { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the list of serial line protocols.
        /// </summary>
        public List<string> SerialLineProtocols { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the list of tag colors.
        /// </summary>
        public List<Color> TagColors { get; private set; } = new List<Color>();

        /// <summary>
        /// Gets the current database file path.
        /// </summary>
        public string CurrentFilePath { get; private set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether an IO list is loaded.
        /// </summary>
        public bool IsIOListLoaded => !string.IsNullOrEmpty(CurrentFilePath) && Tags.Count > 0;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the IOListManager class.
        /// </summary>
        public IOListManager()
        {
            InitializeLists();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Imports IO list data from an Access database.
        /// </summary>
        /// <param name="filePath">The path to the Access database file.</param>
        /// <param name="modbusManager">The ModbusManager to initialize with the imported data.</param>
        /// <returns>True if import was successful, otherwise false.</returns>
        public bool ImportIOList(string filePath, ModbusManager modbusManager)
        {
            try
            {
                // Clear existing data
                ClearIOList();
                CurrentFilePath = filePath;

                // Clear holding registers in both datastores
                if (modbusManager != null)
                {
                    // Clear RTU datastore
                    for (int i = 1; i < modbusManager.RtuDataStore.HoldingRegisters.Count; i++)
                    {
                        modbusManager.RtuDataStore.HoldingRegisters[i] = 0;
                    }

                    // Clear TCP datastore
                    for (int i = 1; i < modbusManager.TcpDataStore.HoldingRegisters.Count; i++)
                    {
                        modbusManager.TcpDataStore.HoldingRegisters[i] = 0;
                    }

                    // Clear legacy datastore for backward compatibility
                    for (int i = 1; i < modbusManager.DataStore.HoldingRegisters.Count; i++)
                    {
                        modbusManager.DataStore.HoldingRegisters[i] = 0;
                    }
                }

                // Import data from the Access database
                using (OleDbConnection connection = CreateDatabaseConnection(filePath))
                {
                    connection.Open();
                    using (OleDbCommand command = new OleDbCommand(GetIOListQuery(), connection))
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        // Read data from the database
                        while (reader.Read())
                        {
                            ProcessDatabaseRow(reader);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing IO list: {ex.Message}",
                    "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Updates the test result for a signal in the IO list.
        /// </summary>
        /// <param name="serialAddress">The serial address of the signal.</param>
        /// <param name="isOK">True if test result is OK, false if Not OK.</param>
        /// <returns>True if update was successful, otherwise false.</returns>
        public bool UpdateTestResult(string serialAddress, bool isOK)
        {
            try
            {
                if (string.IsNullOrEmpty(CurrentFilePath) || string.IsNullOrEmpty(serialAddress))
                {
                    MessageBox.Show("Cannot update test result without a loaded IO list or valid address.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Get the index of the serial address in the list
                int index = SerialAddresses.IndexOf(serialAddress);
                if (index < 0)
                {
                    MessageBox.Show("Serial address not found in IO list.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Update the database
                string testResult = isOK ? "OK" : "Not OK";
                string updateQuery = "UPDATE Io_List SET W_Citect_Test = @newValue WHERE S_Serial_Line_Address = @serialLineAddress;";

                using (OleDbConnection connection = CreateDatabaseConnection(CurrentFilePath))
                using (OleDbCommand cmd = new OleDbCommand(updateQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@newValue", testResult);
                    cmd.Parameters.AddWithValue("@serialLineAddress", serialAddress);

                    connection.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    // Update the local list
                    VerifiedTests[index] = testResult;
                    TagColors[index] = isOK ? Color.LimeGreen : Color.Red;

                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating test result: {ex.Message}",
                    "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Gets the serial line names for populating the serial line combo box.
        /// </summary>
        /// <returns>A list of unique serial line names.</returns>
        public List<string> GetUniqueSerialLineNames()
        {
            List<string> uniqueNames = new List<string>();

            foreach (string name in SerialLineNames)
            {
                if (!uniqueNames.Contains(name))
                {
                    uniqueNames.Add(name);
                }
            }

            return uniqueNames;
        }

        /// <summary>
        /// Gets the signals for a specific serial line.
        /// </summary>
        /// <param name="serialLineName">The name of the serial line.</param>
        /// <returns>A list of ListViewItems representing the signals.</returns>
        public List<ListViewItem> GetSignalsForSerialLine(string serialLineName)
        {
            List<ListViewItem> items = new List<ListViewItem>();

            for (int i = 0; i < Tags.Count; i++)
            {
                if (SerialLineNames[i] == serialLineName)
                {
                    // Build the text string from tag and loop typical
                    string text = $"{Tags[i]} {LoopTypicals[i]}";

                    // Create a ListViewItem with the appropriate color
                    ListViewItem item = new ListViewItem(text);
                    item.BackColor = TagColors[i];

                    items.Add(item);
                }
            }

            return items;
        }

        /// <summary>
        /// Gets the protocol for a signal by index.
        /// </summary>
        /// <param name="index">The index of the signal in the lists.</param>
        /// <returns>The protocol string ("Modbus RTU" or "Modbus TCP"), or empty string if index is invalid.</returns>
        public string GetProtocolForSignal(int index)
        {
            if (index >= 0 && index < SerialLineProtocols.Count)
            {
                return SerialLineProtocols[index];
            }

            Debug.WriteLine($"WARNING: Invalid index {index} for GetProtocolForSignal");
            return string.Empty;
        }

        /// <summary>
        /// Gets the protocol for a signal by its serial address.
        /// </summary>
        /// <param name="serialAddress">The serial address of the signal.</param>
        /// <returns>The protocol string ("Modbus RTU" or "Modbus TCP"), or empty string if address is not found.</returns>
        public string GetProtocolForAddress(string serialAddress)
        {
            int index = SerialAddresses.IndexOf(serialAddress);
            if (index >= 0)
            {
                return GetProtocolForSignal(index);
            }

            Debug.WriteLine($"WARNING: Serial address {serialAddress} not found");
            return string.Empty;
        }

        /// <summary>
        /// Checks if a signal is digital.
        /// </summary>
        /// <param name="index">The index of the signal in the lists.</param>
        /// <returns>True if the signal is digital, otherwise false.</returns>
        public bool IsDigitalSignal(int index)
        {
            if (index < 0 || index >= LoopTypicals.Count)
                return false;

            return LoopTypicals[index].Contains("DI") || LoopTypicals[index].Contains("DO");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes the lists to store IO list data.
        /// </summary>
        private void InitializeLists()
        {
            SerialLineNames = new List<string>();
            Tags = new List<string>();
            Descriptions = new List<string>();
            EngUnits = new List<string>();
            EngRangeLow = new List<string>();
            EngRangeHigh = new List<string>();
            SerialAddresses = new List<string>();
            SerialLineLow = new List<string>();
            SerialLineHigh = new List<string>();
            VerifiedTests = new List<string>();
            LoopTypicals = new List<string>();
            SerialLineProtocols = new List<string>();
            TagColors = new List<Color>();
        }

        /// <summary>
        /// Clears all IO list data.
        /// </summary>
        private void ClearIOList()
        {
            SerialLineNames.Clear();
            Tags.Clear();
            Descriptions.Clear();
            EngUnits.Clear();
            EngRangeLow.Clear();
            EngRangeHigh.Clear();
            SerialAddresses.Clear();
            SerialLineLow.Clear();
            SerialLineHigh.Clear();
            VerifiedTests.Clear();
            LoopTypicals.Clear();
            SerialLineProtocols.Clear();
            TagColors.Clear();
        }

        /// <summary>
        /// Creates a database connection to the Access database.
        /// </summary>
        /// <param name="filePath">The path to the Access database file.</param>
        /// <returns>An OleDbConnection object.</returns>
        private OleDbConnection CreateDatabaseConnection(string filePath)
        {
            string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};Persist Security Info=False;";
            return new OleDbConnection(connectionString);
        }

        /// <summary>
        /// Gets the SQL query for retrieving IO list data.
        /// </summary>
        /// <returns>The SQL query string.</returns>
        private string GetIOListQuery()
        {
            return "SELECT Io_List.S_Serial_Line_Name, Io_List.S_Instrument_Tag, Io_List.S_Description, " +
                   "Io_List.S_Serial_Line_Address, Io_List.S_Eng_Units, Io_List.S_Eng_Range_Low, " +
                   "Io_List.S_Eng_Range_High, Io_List.S_Serial_Line_Range_Low, Io_List.S_Serial_Line_Range_High, " +
                   "Io_List.W_Citect_Test, Io_List.S_Loop_Typical, Io_List.S_Serial_line_protocol " +
                   "FROM Io_List " +
                   "WHERE (((Io_List.S_Serial_Line_Name) Is Not Null)) " +
                   "ORDER BY Io_List.S_Instrument_Tag ASC;";
        }

        /// <summary>
        /// Processes a row from the database and adds it to the lists.
        /// </summary>
        /// <param name="reader">The OleDbDataReader containing the row data.</param>
        private void ProcessDatabaseRow(OleDbDataReader reader)
        {
            try
            {
                // Add serial line name
                SerialLineNames.Add(reader.GetString(0));

                // Add tag (or empty string if null)
                Tags.Add(reader.IsDBNull(1) ? "" : reader.GetString(1));

                // Add description (or empty string if null)
                Descriptions.Add(reader.IsDBNull(2) ? "" : reader.GetString(2));

                // Add serial address (or empty string if null)
                string serialAddress = reader.IsDBNull(3) ? "" : reader.GetString(3);
                SerialAddresses.Add(serialAddress);

                // Add test result (or empty string if null)
                string testResult = reader.IsDBNull(9) ? "" : reader.GetString(9);
                VerifiedTests.Add(testResult);

                // Add loop typical (or empty string if null)
                LoopTypicals.Add(reader.IsDBNull(10) ? "" : reader.GetString(10));

                // Add serial line protocol (or empty string if null)
                string protocol = reader.IsDBNull(11) ? "" : reader.GetString(11);

                // Normalize protocol value for consistency
                if (string.IsNullOrEmpty(protocol))
                {
                    protocol = "Modbus RTU"; // Default to RTU if protocol is not specified
                }
                else if (!protocol.StartsWith("Modbus ", StringComparison.OrdinalIgnoreCase))
                {
                    protocol = "Modbus " + protocol; // Ensure protocol has "Modbus " prefix
                }

                SerialLineProtocols.Add(protocol);

                // Set tag color based on test result
                if (testResult == "OK")
                {
                    TagColors.Add(Color.LimeGreen);
                }
                else if (testResult == "Not OK")
                {
                    TagColors.Add(Color.Red);
                }
                else
                {
                    TagColors.Add(Color.Silver);
                }

                // Special handling for dot addresses
                if (serialAddress.Contains("."))
                {
                    EngUnits.Add("");
                    EngRangeLow.Add("0");
                    EngRangeHigh.Add("1");
                    SerialLineLow.Add("0");
                    SerialLineHigh.Add("1");
                }
                else
                {
                    // Add regular engineering units and ranges
                    EngUnits.Add(reader.IsDBNull(4) ? "" : reader.GetString(4));
                    EngRangeLow.Add(reader.IsDBNull(5) ? "" : reader.GetString(5));
                    EngRangeHigh.Add(reader.IsDBNull(6) ? "" : reader.GetString(6));
                    SerialLineLow.Add(reader.IsDBNull(7) ? "" : reader.GetString(7));
                    SerialLineHigh.Add(reader.IsDBNull(8) ? "" : reader.GetString(8));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing database row: {ex.Message}");
                // Continue processing other rows
            }
        }

        #endregion
    }
}