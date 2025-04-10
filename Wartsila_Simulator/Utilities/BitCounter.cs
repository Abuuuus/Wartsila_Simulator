using System;
using System.Diagnostics;

namespace Wartsila_Simulator.Utilities
{
    /// <summary>
    /// Utility class for bit manipulation and calculation of bit values.
    /// </summary>
    internal class BitCounter
    {
        /// <summary>
        /// Gets the calculated bit value.
        /// </summary>
        public ushort BitValue { get; private set; }

        /// <summary>
        /// Initializes a new instance of the BittCounter class.
        /// </summary>
        /// <param name="bitNumber">The bit position (0-15).</param>
        /// <param name="value">The value to set (0 or 1).</param>
        public BitCounter(int bitNumber, int value)
        {
            try
            {
                // Validate input parameters
                if (bitNumber < 0 || bitNumber > 15)
                {
                    BitValue = 0;
                    return;
                }

                // Optimized calculation using bit shifting - cleaner and more efficient
                // than the original switch statement
                BitValue = value == 1 ? (ushort)(1 << bitNumber) : (ushort)0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in BittCounter: {ex.Message}");
                BitValue = 0;
            }
        }
    }
}