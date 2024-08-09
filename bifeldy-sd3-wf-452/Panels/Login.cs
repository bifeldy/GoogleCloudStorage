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
            _config = config;
            _app = app;
            _db = db;
            _updater = updater;

            InitializeComponent();
            OnInit();
        }

        private void OnInit() {
            Dock = DockStyle.Fill;
        }

        private void CLogin_Load(object sender, EventArgs e) {
            if (!isInitialized) {

                mainForm = (CMainForm) Parent.Parent;

                ((CCekProgram) mainForm.PanelContainer.Controls["CCekProgram"]).LoadingInformation.Text = "Harap Menunggu ...";

                isInitialized = true;
            }
        }

        private void ShowLoading(bool isShow) {

            // Set State To Loading
            ToggleEnableDisableInput(!isShow);
            if (isShow) {
                mainForm.PanelContainer.Controls["CCekProgram"].BringToFront();
            }
            else {
                BringToFront();
            }
        }

        private void ToggleEnableDisableInput(bool isEnable) {

            // Enable / Disable View
            btnLogin.Enabled = isEnable;
            txtUserNameNik.Enabled = isEnable;
            txtPassword.Enabled = isEnable;
        }

        private void ShowMainPanel() {

            // Change Window Size & Position To Middle Screen
            mainForm.Width = 800 + 64 + 14;
            mainForm.Height = 600 + 64 + 7;
            mainForm.SetDesktopLocation((_app.ScreenWidth - mainForm.Width) / 2, (_app.ScreenHeight - mainForm.Height) / 2);

            // Change Panel To Fully Windowed Mode And Go To `MainPanel`
            mainForm.HideLogo();
            mainForm.PanelContainer.Dock = DockStyle.Fill;

            // Create & Show `MainPanel`
            try {
                if (!mainForm.PanelContainer.Controls.ContainsKey("CMainPanel")) {
                    mainForm.PanelContainer.Controls.Add(CProgram.Bifeldyz.Resolve<CMainPanel>());
                }
                mainForm.PanelContainer.Controls["CMainPanel"].BringToFront();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "Terjadi Kesalahan! (｡>﹏<｡)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Remove All Other Unused Panels
            mainForm.PanelContainer.Controls.RemoveByKey("CDbSelector");
            mainForm.PanelContainer.Controls.RemoveByKey("CCekProgram");
            mainForm.PanelContainer.Controls.RemoveByKey("CLogin");
        }

        public async void ProcessLogin() {

            // Disable View While Loading
            ShowLoading(true);

            // Login Check Credential
            bool resultLogin = false;

            bool bypassLogin = _config.Get<bool>("BypassLogin", bool.Parse(_app.GetConfig("bypass_login")));
            if (bypassLogin) {
                txtUserNameNik.ReadOnly = true;
                txtPassword.ReadOnly = true;
                _db.LoggedInUsername = _app.AppName;
                txtUserNameNik.Text = _app.AppName;
                txtPassword.Text = _app.AppName;
                resultLogin = true;
            }
            else {

                // Check User Input
                if (string.IsNullOrEmpty(txtUserNameNik.Text) || string.IsNullOrEmpty(txtPassword.Text)) {
                    ShowLoading(false);
                    MessageBox.Show("Username / NIK Dan Kata Sandi Wajib Diisi!", "User Authentication", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                await Task.Run(async () => {
                    resultLogin = await _db.LoginUser(txtUserNameNik.Text, txtPassword.Text);
                });
            }

            if (!resultLogin) {
                ShowLoading(false);
                MessageBox.Show("Login Gagal, Kredensial Salah!", "User Authentication", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else {

                // Check IP / MAC
                bool resultCekIpMac = false;

                bool bypassIpMac = _config.Get<bool>("BypassIpMac", bool.Parse(_app.GetConfig("bypass_ip_mac")));
                if (bypassIpMac) {
                    resultCekIpMac = true;
                }
                else {
                    await Task.Run(async () => {
                        resultCekIpMac = await _db.CheckIpMac();
                    });
                }

                if (!resultCekIpMac) {
                    ShowLoading(false);
                    MessageBox.Show(
                        $"Alamat IP / MAC Untuk User '{_db.LoggedInUsername}' Belum Terdaftar!",
                        "User Authentication",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else {
                    ShowMainPanel();
                }
            }
        }

        private void CheckKeyboard(object sender, KeyEventArgs e) {
            switch (e.KeyCode) {
                case Keys.Enter:
                    ProcessLogin();
                    break;
                default:
                    break;
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e) {
            ProcessLogin();
        }

        private void Txt_KeyDown(object sender, KeyEventArgs e) {
            CheckKeyboard(sender, e);
        }

        private void BtnPaksaUpdate_Click(object sender, EventArgs e) {
            _updater.CheckUpdater();
        }

    }

}
