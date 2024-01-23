/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Custom Tab Control Tanpa Border
 *              :: Seharusnya Tidak Untuk Didaftarkan Ke DI Container
 *              :: https://stackoverflow.com/questions/4912354/how-do-i-create-a-tabcontrol-with-no-tab-headers
 * 
 */

using System;
using System.Windows.Forms;

namespace GoogleCloudStorage.Components {

    public sealed class NoBorderTab : TabControl {

        private const int TCM_ADJUSTRECT = 0x1328;

        protected override void WndProc(ref Message m) {

            // Hide the tab headers at run-time
            if (m.Msg == TCM_ADJUSTRECT && !DesignMode) {
                m.Result = (IntPtr) 1;
                return;
            }

            // call the base class implementation
            base.WndProc(ref m);
        }

    }

}
