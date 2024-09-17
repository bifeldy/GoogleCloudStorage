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
using System.Windows.Forms;

namespace GoogleCloudStorage.Components {

    /// <summary>
    /// This class is an implementation of the 'IComparer' interface.
    /// </summary>
    public sealed class ListViewColumnSorter : IComparer {
        /// <summary>
        /// Specifies the column to be sorted
        /// </summary>
        private int ColumnToSort;
        /// <summary>
        /// Specifies the order in which to sort (i.e. 'Ascending').
        /// </summary>
        private SortOrder OrderOfSort;
        /// <summary>
        /// Case insensitive comparer object
        /// </summary>
        private readonly CaseInsensitiveComparer ObjectCompare;

        /// <summary>
        /// Class constructor.  Initializes various elements
        /// </summary>
        public ListViewColumnSorter() {
            // Initialize the column to '0'
            this.ColumnToSort = 0;

            // Initialize the sort order to 'none'
            this.OrderOfSort = SortOrder.None;

            // Initialize the CaseInsensitiveComparer object
            this.ObjectCompare = new CaseInsensitiveComparer();
        }

        /// <summary>
        /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
        /// </summary>
        /// <param name="x">First object to be compared</param>
        /// <param name="y">Second object to be compared</param>
        /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
        public int Compare(object x, object y) {
            int compareResult;
            ListViewItem listviewX, listviewY;

            // Cast the objects to be compared to ListViewItem objects
            listviewX = (ListViewItem) x;
            listviewY = (ListViewItem) y;

            decimal num = 0;
            if (decimal.TryParse(listviewX.SubItems[this.ColumnToSort].Text, out num)) {
                compareResult = decimal.Compare(num, Convert.ToDecimal(listviewY.SubItems[this.ColumnToSort].Text));
            }
            else {
                // Compare the two items
                compareResult = this.ObjectCompare.Compare(listviewX.SubItems[this.ColumnToSort].Text, listviewY.SubItems[this.ColumnToSort].Text);
            }

            // Calculate correct return value based on object comparison
            if (this.OrderOfSort == SortOrder.Ascending) {
                // Ascending sort is selected, return normal result of compare operation
                return compareResult;
            }
            else if (this.OrderOfSort == SortOrder.Descending) {
                // Descending sort is selected, return negative result of compare operation
                return -compareResult;
            }
            else {
                // Return '0' to indicate they are equal
                return 0;
            }
        }

        /// <summary>
        /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
        /// </summary>
        public int SortColumn {
            set {
                this.ColumnToSort = value;
            }
            get {
                return this.ColumnToSort;
            }
        }

        /// <summary>
        /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
        /// </summary>
        public SortOrder Order {
            set {
                this.OrderOfSort = value;
            }
            get {
                return this.OrderOfSort;
            }
        }

    }

}
