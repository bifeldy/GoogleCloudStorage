/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Database Selector
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Windows.Forms;

using GoogleCloudStorage.Forms;
using GoogleCloudStorage.Utilities;

namespace GoogleCloudStorage.Panels {

    public sealed partial class CDbSelector : UserControl {

        private readonly IApp _app;

        private CMainForm mainForm;

        private bool isInitialized = false;

        public CDbSelector(IApp app) {
            this._app = app;

            this.InitializeComponent();
            this.OnInit();
        }

        private void OnInit() {
            this.Dock = DockStyle.Fill;
        }

        private void CDbSelector_Load(object sender, EventArgs e) {
            if (!this.isInitialized) {

                this.mainForm = (CMainForm)this.Parent.Parent;

                this.isInitialized = true;
            }
        }

        private void ShowCheckProgramPanel() {
            this.btnOracle.Enabled = false;
            this.btnPostgre.Enabled = false;

            // Create & Show `CekProgram` Panel
            try {
                if (!this.mainForm.PanelContainer.Controls.ContainsKey("CCekProgram")) {
                    this.mainForm.PanelContainer.Controls.Add(CProgram.Bifeldyz.Resolve<CCekProgram>());
                }

                this.mainForm.PanelContainer.Controls["CCekProgram"].BringToFront();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "Terjadi Kesalahan! (｡>﹏<｡)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOracle_Click(object sender, EventArgs e) {
            this._app.IsUsingPostgres = false;
            this.ShowCheckProgramPanel();
        }

        private void BtnPostgre_Click(object sender, EventArgs e) {
            this._app.IsUsingPostgres = true;
            this.ShowCheckProgramPanel();
        }

        public void DchoOnlyBypass(object sender, EventArgs e) {
            this.BtnOracle_Click(sender, e);
        }

        public void AutoRunModeDefaultPostgre(object sender, EventArgs e) {
            this.BtnPostgre_Click(sender, e);
        }

    }

}
