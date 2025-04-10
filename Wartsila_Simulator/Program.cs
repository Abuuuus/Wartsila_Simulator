using System;
using System.Windows.Forms;

namespace Wartsila_Simulator
{
    /// <summary>
    /// Main entry point for the Wartsila Simulator application.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Create and run the main form
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                // Last-resort error handling to prevent application crashes
                MessageBox.Show($"An unexpected error occurred: {ex.Message}\n\nThe application will now exit.",
                    "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}