/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Custom Toolbox Date Picker Hanya Bulan Dan Tahun Saja
 *              :: Seharusnya Tidak Untuk Didaftarkan Ke DI Container
 *              :: https://stackoverflow.com/questions/34846455/how-can-i-set-the-datetimepicker-dropdown-to-show-months-only
 * 
 */

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GoogleCloudStorage.Components {

    public sealed class MonthYearPicker : DateTimePicker {

        // initialize Format/CustomFormat to display only month and year.
        public MonthYearPicker() {
            this.Format = DateTimePickerFormat.Custom;
            this.CustomFormat = "MMMM yyyy";
        }

        // override Format to redefine default value (used by designer)
        [DefaultValue(DateTimePickerFormat.Custom)]
        public new DateTimePickerFormat Format {
            get => base.Format;
            set => base.Format = value;
        }

        // override CustomFormat to redefine default value (used by designer)
        [DefaultValue("MMM yyyy")]
        public new string CustomFormat {
            get => base.CustomFormat;
            set => base.CustomFormat = value;
        }

        protected override void WndProc(ref Message m) {
            if (m.Msg == WM_NOFITY) {
                var nmhdr = (NMHDR) Marshal.PtrToStructure(m.LParam, typeof(NMHDR));
                switch (nmhdr.code) {

                    // detect pop-up display and switch view to month selection
                    case -950:
                        var cal = SendMessage(this.Handle, DTM_GETMONTHCAL, IntPtr.Zero, IntPtr.Zero);
                        _ = SendMessage(cal, MCM_SETCURRENTVIEW, IntPtr.Zero, (IntPtr)1);
                        break;

                    // detect month selection and close the pop-up
                    case MCN_VIEWCHANGE:
                        var nmviewchange = (NMVIEWCHANGE) Marshal.PtrToStructure(m.LParam, typeof(NMVIEWCHANGE));
                        if (nmviewchange.dwOldView == 1 && nmviewchange.dwNewView == 0) {
                            _ = SendMessage(this.Handle, DTM_CLOSEMONTHCAL, IntPtr.Zero, IntPtr.Zero);
                        }

                        break;
                }
            }

            base.WndProc(ref m);
        }

        private const int WM_NOFITY = 0x004e;
        private const int DTM_CLOSEMONTHCAL = 0x1000 + 13;
        private const int DTM_GETMONTHCAL = 0x1000 + 8;
        private const int MCM_SETCURRENTVIEW = 0x1000 + 32;
        private const int MCN_VIEWCHANGE = -750;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct NMHDR {
            public IntPtr hwndFrom;
            public IntPtr idFrom;
            public int code;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct NMVIEWCHANGE {
            public NMHDR nmhdr;
            public uint dwOldView;
            public uint dwNewView;
        }

    }

}
