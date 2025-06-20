/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Untuk Sorting List View
 * 
 */

using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace GoogleCloudStorage.Components {

    public sealed class ListViewColumnSorter : IComparer {

        public int SortColumn { set; get; }
        public SortOrder Order { set; get; }

        private readonly CaseInsensitiveComparer ObjectCompare;

        public ListViewColumnSorter() {
            this.SortColumn = 0;
            this.Order = SortOrder.None;
            this.ObjectCompare = new CaseInsensitiveComparer();
        }

        private bool IsHumanReadableSize(string text) {
            return Regex.IsMatch(text, @"^\d+(\.\d+)?\s?(B|KB|MB|GB|TB)$", RegexOptions.IgnoreCase);
        }

        private long ParseSizeStringToBytes(string sizeStr) {
            Match match = Regex.Match(sizeStr.Trim(), @"(?i)^(?<value>\d+(\.\d+)?)\s?(?<unit>B|KB|MB|GB|TB)$");

            if (!match.Success) {
                return 0;
            }

            double value = double.Parse(match.Groups["value"].Value);
            string unit = match.Groups["unit"].Value.ToUpper();

            switch (unit) {
                case "TB":
                    value *= 1024;
                    goto case "GB";
                case "GB":
                    value *= 1024;
                    goto case "MB";
                case "MB":
                    value *= 1024;
                    goto case "KB";
                case "KB":
                    value *= 1024;
                    break;
                case "B":
                default:
                    break;
            }

            return (long)value;
        }

        public int Compare(object x, object y) {
            int compareResult = 0;

            var listviewX = (ListViewItem)x;
            var listviewY = (ListViewItem)y;

            string textX = listviewX.SubItems[this.SortColumn].Text;
            string textY = listviewY.SubItems[this.SortColumn].Text;

            if (decimal.TryParse(textX, out decimal numX) && decimal.TryParse(textY, out decimal numY)) {
                compareResult = decimal.Compare(numX, numY);
            }
            else if (DateTime.TryParse(textX, out DateTime dateX) && DateTime.TryParse(textY, out DateTime dateY)) {
                compareResult = DateTime.Compare(dateX, dateY);
            }
            else if (this.IsHumanReadableSize(textX) && this.IsHumanReadableSize(textY)) {
                long sizeX = this.ParseSizeStringToBytes(textX);
                long sizeY = this.ParseSizeStringToBytes(textY);
                compareResult = sizeX.CompareTo(sizeY);
            }
            else {
                compareResult = this.ObjectCompare.Compare(textX, textY);
            }

            return this.Order == SortOrder.Ascending ? compareResult :
                   this.Order == SortOrder.Descending ? -compareResult : 0;
        }

    }

}
