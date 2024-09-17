/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Login User
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Utilities;

using GoogleCloudStorage.Forms;
using GoogleCloudStorage.Handlers;
using GoogleCloudStorage.Utilities;

namespace GoogleCloudStorage.Panels {

    public sealed partial class CLogin : UserControl {

        private readonly IConfig _config;

        private readonly IApp _app;
        private readonly IDb _db;
        private readonly IUpdater _updater;

        private CMainForm mainForm;

        private bool isInitialized = false;

        public CLogin(IConfig config, IApp app, IDb db, IUpdater updater) {
            this._config = config;
            this._app = app;
            this._db = db;
            this._updater = updater;

            this.InitializeComponent();
            this.OnInit();
        }

        private void OnInit() {
            this.Dock = DockStyle.Fill;
        }

        private void CLogin_Load(object sender, EventArgs e) {
            if (!this.isInitialized) {

                this.mainForm = (CMainForm)this.Parent.Parent;

                ((CCekProgram)this.mainForm.PanelContainer.Controls["CCekProgram"]).LoadingInformation.Text = "Harap Menunggu ...";

                this.isInitialized = true;
            }
        }

        private void ShowLoading(bool isShow) {

            // Set State To Loading
            this.ToggleEnableDisableInput(!isShow);
            if (isShow) {
                this.mainForm.PanelContainer.Controls["CCekProgram"].BringToFront();
            }
            else {
                this.BringToFront();
            }
        }

        private void ToggleEnableDisableInput(bool isEnable) {

            // Enable / Disable View
            this.btnLogin.Enabled = isEnable;
            this.txtUserNameNik.Enabled = isEnable;
            this.txtPassword.Enabled = isEnable;
        }

        private void ShowMainPanel() {

            // Change Window Size & Position To Middle Screen
            this.mainForm.Width = 800 + 64 + 14;
            this.mainForm.Height = 600 + 64 + 7;
            this.mainForm.SetDesktopLocation((this._app.ScreenWidth - this.mainForm.Width) / 2, (this._app.ScreenHeight - this.mainForm.Height) / 2);

            // Change Panel To Fully Windowed Mode And Go To `MainPanel`
            this.mainForm.HideLogo();
            this.mainForm.PanelContainer.Dock = DockStyle.Fill;

            // Create & Show `MainPanel`
            try {
                if (!this.mainForm.PanelContainer.Controls.ContainsKey("CMainPanel")) {
                    this.mainForm.PanelContainer.Controls.Add(CProgram.Bifeldyz.Resolve<CMainPanel>());
                }

                this.mainForm.PanelContainer.Controls["CMainPanel"].BringToFront();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "Terjadi Kesalahan! (｡>﹏<｡)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Remove All Other Unused Panels
            this.mainForm.PanelContainer.Controls.RemoveByKey("CDbSelector");
            this.mainForm.PanelContainer.Controls.RemoveByKey("CCekProgram");
            this.mainForm.PanelContainer.Controls.RemoveByKey("CLogin");
        }

        public async void ProcessLogin() {

            // Disable View While Loading
            this.ShowLoading(true);

            // Login Check Credential
            bool resultLogin = false;

            bool bypassLogin = this._config.Get<bool>("BypassLogin", bool.Parse(this._app.GetConfig("bypass_login")));
            if (bypassLogin) {
                this.txtUserNameNik.ReadOnly = true;
                this.txtPassword.ReadOnly = true;
                this._db.LoggedInUsername = this._app.AppName;
                this.txtUserNameNik.Text = this._app.AppName;
                this.txtPassword.Text = this._app.AppName;
                resultLogin = true;
            }
            else {

                // Check User Input
                if (string.IsNullOrEmpty(this.txtUserNameNik.Text) || string.IsNullOrEmpty(this.txtPassword.Text)) {
                    this.ShowLoading(false);
                    MessageBox.Show("Username / NIK Dan Kata Sandi Wajib Diisi!", "User Authentication", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                await Task.Run(async () => {
                    resultLogin = await this._db.LoginUser(this.txtUserNameNik.Text, this.txtPassword.Text);
                });
            }

            if (!resultLogin) {
                this.ShowLoading(false);
                MessageBox.Show("Login Gagal, Kredensial Salah!", "User Authentication", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else {

                // Check IP / MAC
                bool resultCekIpMac = false;

                bool bypassIpMac = this._config.Get<bool>("BypassIpMac", bool.Parse(this._app.GetConfig("bypass_ip_mac")));
                if (bypassIpMac) {
                    resultCekIpMac = true;
                }
                else {
                    await Task.Run(async () => {
                        resultCekIpMac = await this._db.CheckIpMac();
                    });
                }

                if (!resultCekIpMac) {
                    this.ShowLoading(false);
                    MessageBox.Show(
                        $"Alamat IP / MAC Untuk User '{this._db.LoggedInUsername}' Belum Terdaftar!",
                        "User Authentication",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else {
                    this.ShowMainPanel();
                }
            }
        }

        private void CheckKeyboard(object sender, KeyEventArgs e) {
            switch (e.KeyCode) {
                case Keys.Enter:
                    this.ProcessLogin();
                    break;
                default:
                    break;
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e) {
            this.ProcessLogin();
        }

        private void Txt_KeyDown(object sender, KeyEventArgs e) {
            this.CheckKeyboard(sender, e);
        }

        private void BtnPaksaUpdate_Click(object sender, EventArgs e) {
            this._updater.CheckUpdater();
        }

    }

}
