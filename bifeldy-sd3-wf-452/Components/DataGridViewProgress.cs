/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Custom Toolbox ProgressBar Data Grid View
 * 
 */

using System;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace GoogleCloudStorage.Components {

    public class DataGridViewProgressColumn : DataGridViewImageColumn {

        public DataGridViewProgressColumn() {
            CellTemplate = new DataGridViewProgressCell();
        }

    }

    public sealed class DataGridViewProgressCell : DataGridViewImageCell {

        // Used to make custom cell consistent with a DataGridViewImageCell
        static Image emptyImage;

        static DataGridViewProgressCell() {
            emptyImage = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }

        public DataGridViewProgressCell() {
            this.ValueType = typeof(double);
        }

        // Method required to make the Progress Cell consistent with the default Image Cell. 
        // The default Image Cell assumes an Image as a value, although the value of the Progress Cell is an int.
        protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context) {
            return emptyImage;
        }

        protected override void Paint(Graphics g, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts) {
            try {
                if (!double.TryParse(value?.ToString(), out double progressVal)) {
                    progressVal = 0;
                }

                // Need to convert to float before division; otherwise C# returns int which is 0 for anything but 100%.
                double percentage = progressVal / 100.0d;

                Brush backColorBrush = new SolidBrush(cellStyle.BackColor);
                Brush foreColorBrush = new SolidBrush(cellStyle.ForeColor);

                // Draws the cell grid
                base.Paint(g, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, (paintParts & ~DataGridViewPaintParts.ContentForeground));
                if (percentage > 0.0) {

                    // Draw the progress bar and the text
                    g.FillRectangle(new SolidBrush(Color.FromArgb(203, 235, 108)), cellBounds.X + 2, cellBounds.Y + 2, Convert.ToInt32((percentage * cellBounds.Width - 4)), cellBounds.Height - 4);
                    g.DrawString(progressVal.ToString() + "%", cellStyle.Font, foreColorBrush, cellBounds.X + (cellBounds.Width / 2) - 5, cellBounds.Y + 2);
                }
                else {

                    // draw the text
                    if (this.DataGridView.CurrentRow.Index == rowIndex) {
                        g.DrawString(progressVal.ToString() + "%", cellStyle.Font, new SolidBrush(cellStyle.SelectionForeColor), cellBounds.X + 6, cellBounds.Y + 2);
                    }
                    else {
                        g.DrawString(progressVal.ToString() + "%", cellStyle.Font, foreColorBrush, cellBounds.X + 6, cellBounds.Y + 2);
                    }
                }
            }
            catch (Exception ex) {
                // MessageBox.Show(ex.Message, "ProgressBar Paint", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
    }

}
