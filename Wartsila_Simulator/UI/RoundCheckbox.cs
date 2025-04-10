using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Wartsila_Simulator.UI
{
    /// <summary>
    /// Custom checkbox control that renders as a round checkbox with green background when checked.
    /// </summary>
    public class RoundCheckbox : CheckBox
    {
        // Cache the brushes and pens to improve performance
        private static readonly Brush _checkedBrush = new SolidBrush(Color.Green);
        private static readonly Pen _uncheckedPen = new Pen(Color.Gray, 2);

        // Constants
        private const int CHECK_BOX_SIZE = 16;
        private const int TEXT_OFFSET = 2;

        /// <summary>
        /// Initializes a new instance of the RoundCheckbox class.
        /// </summary>
        public RoundCheckbox()
        {
            // Set double buffering to reduce flicker
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);
        }

        /// <summary>
        /// Overrides the OnPaint method to draw a custom round checkbox.
        /// </summary>
        /// <param name="e">Paint event arguments.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                // Clear the background
                e.Graphics.Clear(this.BackColor);

                // Set up anti-aliasing for smooth edges
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Define the checkbox rectangle
                Rectangle rect = new Rectangle(0, 0, CHECK_BOX_SIZE, CHECK_BOX_SIZE);

                // Draw checked or unchecked state
                if (this.Checked)
                {
                    e.Graphics.FillEllipse(_checkedBrush, rect);
                }
                else
                {
                    e.Graphics.DrawEllipse(_uncheckedPen, rect);
                }

                // Draw the text
                TextRenderer.DrawText(
                    e.Graphics,
                    this.Text,
                    this.Font,
                    new Point(CHECK_BOX_SIZE + TEXT_OFFSET, (this.Height - this.Font.Height) / 2),
                    this.ForeColor
                );
            }
            catch (Exception ex)
            {
                // Fallback to default rendering in case of error
                Debug.WriteLine($"Error rendering RoundCheckbox: {ex.Message}");
                base.OnPaint(e);
            }
        }

        /// <summary>
        /// Releases used resources.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}