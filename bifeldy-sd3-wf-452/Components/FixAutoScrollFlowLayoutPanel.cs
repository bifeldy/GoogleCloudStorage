/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Custom Toolbox FlowLayoutPanel Yang Auto Scoll Mental Ke Atas
 *              :: Seharusnya Tidak Untuk Didaftarkan Ke DI Container
 *              :: https://stackoverflow.com/questions/6443086/prevent-flowlayoutpanel-scrolling-to-updated-control
 * 
 */

using System.Windows.Forms;

namespace GoogleCloudStorage.Components {

    public sealed class FixAutoScrollFlowLayoutPanel : FlowLayoutPanel {

        protected override System.Drawing.Point ScrollToControl(Control activeControl) {
            // return base.ScrollToControl(activeControl);
            return this.AutoScrollPosition;
        }

    }

}
