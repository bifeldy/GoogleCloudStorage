/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Window Utama
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Utilities;

using GoogleCloudStorage.Handlers;
using GoogleCloudStorage.Panels;
using GoogleCloudStorage.Utilities;

namespace GoogleCloudStorage.Forms {

    public sealed partial class CMainForm : Form {

        private delegate void DelegateFunction();

        private readonly IApp _app;
        private readonly IDb _db;
        private readonly IConfig _config;

        private FormWindowState lastFormWindowState;

        private bool isInitialized = false;

        public CMainForm(IApp app, IDb db, IConfig config) {
            this._app = app;
            this._db = db;
            this._config = config;

            this.InitializeComponent();
            this.OnInit();
        }

        public Panel PanelContainer => this.panelContainer;

        public StatusStrip StatusStripContainer => this.statusStripContainer;

        private void OnInit() {

            // Reclaim Space From Right StatusBar Grip
            this.statusStripContainer.Padding = new Padding(
                this.statusStripContainer.Padding.Left,
                this.statusStripContainer.Padding.Top,
                this.statusStripContainer.Padding.Left,
                this.statusStripContainer.Padding.Bottom
            );

            this.Text = this._app.AppName;

            this.sysTrayNotifyIcon.Text = this._app.AppName;
            this.sysTrayNotifyIcon.DoubleClick += this.SysTray_DoubleClick;

            this.sysTrayToolStripMenuItemApp.Text = $"{this._app.CurrentProcess.Id} (0x{this._app.CurrentProcess.MainModule.BaseAddress})";
            this.sysTrayToolStripMenuItemNICs.Image = SystemIcons.Question.ToBitmap();
            this.sysTrayToolStripMenuItemDatabases.Image = SystemIcons.Warning.ToBitmap();
        }

        private async void CMainForm_Load(object sender, EventArgs e) {
            if (!this.isInitialized) {

                this.statusStripContainer.Items["statusStripIpAddress"].Text = "- .: " + string.Join(", ", this._app.GetAllIpAddress()) + " :. -";
                this.statusStripContainer.Items["statusStripAppVersion"].Text = $"v{this._app.AppVersion}";

                this.ShowDbSelectorPanel();
                await Task.Run((Action)this.ShakeForm);

                this.isInitialized = true;
            }
        }

        private void CMainForm_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.UserClosing) {
                bool minimizeOnClose = this._config.Get<bool>("MinimizeOnClose", bool.Parse(this._app.GetConfig("minimize_on_close")));
                if (!minimizeOnClose) {
                    this.SysTray_MenuExit(sender, EventArgs.Empty);
                }
                else {
                    _ = MessageBox.Show("Untuk keluar silahkan klik kanan pada icon di kanan bawah SYSTEM TRAY, jangan lupa pastikan tab SEDANG BERJALAN sudah kosong", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.lastFormWindowState = this.WindowState;
                    this.sysTrayNotifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                    this.sysTrayNotifyIcon.BalloonTipTitle = "Aplikasi Berjalan Di Belakang Layar";
                    this.sysTrayNotifyIcon.BalloonTipText = "Klik Icon 2x Untuk Membuka Kembali";
                    this.sysTrayNotifyIcon.BalloonTipClicked += this.SysTray_DoubleClick;
                    this.sysTrayNotifyIcon.ShowBalloonTip(500);
                    this.WindowState = FormWindowState.Minimized;
                    // this.ShowInTaskbar = false;
                    // this.Hide();
                    e.Cancel = true;
                }
            }
        }

        private void ShowDbSelectorPanel() {

            // Create And Show `DbSelector` Panel
            try {
                CDbSelector dbSelector = null;
                if (!this.panelContainer.Controls.ContainsKey("CDbSelector")) {
                    dbSelector = CProgram.Bifeldyz.Resolve<CDbSelector>();
                    this.panelContainer.Controls.Add(dbSelector);
                }

                this.panelContainer.Controls["CDbSelector"].BringToFront();
                if (this._app.ListDcCanUse.Count == 1 && this._app.ListDcCanUse.Contains("HO")) {
                    dbSelector.DchoOnlyBypass(this, EventArgs.Empty);
                }
                else {
                    bool autoDb = this._config.Get<bool>("AutoDb", bool.Parse(this._app.GetConfig("auto_db")));
                    if (autoDb) {
                        dbSelector.AutoRunModeDefaultPostgre(this, EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex) {
                _ = MessageBox.Show(ex.Message, "Terjadi Kesalahan! (｡>﹏<｡)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void HideLogo() {
            this.imgLogo.Visible = false;
        }

        private void StatusStripIpAddress_Click(object sender, EventArgs e) {
            string[] ipsMacs = this._app.GetIpMacAddress()
                .Select(d => $"{d.DESCRIPTION}\r\n{d.MAC_ADDRESS}\r\n{d.IP_V4_ADDRESS}\r\n{d.IP_V6_ADDRESS}\r\n\r\n")
                .ToArray();
            string ipMac = string.Join(Environment.NewLine, ipsMacs).Replace("\r\n\r\n", "\r\n");
            _ = MessageBox.Show(ipMac, "Network Interface Card", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void SysTray_DoubleClick(object sender, EventArgs e) {
            this.Show();
            this.ShowInTaskbar = true;
            this.WindowState = this.lastFormWindowState;
            this.Activate();
        }

        public void SysTray_MenuExit(object sender, EventArgs e) {
            string title = "Good Bye~ (｡>﹏<｡)";
            string msg = this._app.Author + Environment.NewLine + "© 2023 :: IT S/SD 03";
            _ = MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.sysTrayNotifyIcon.Dispose();
            this._app.Exit();
        }

        private void TimerIpAddress_Tick(object sender, EventArgs e) {
            string fullText = this.statusStripContainer.Items["statusStripIpAddress"].Text;
            string firstLetter = fullText.Substring(0, 1);
            string startText = fullText.Substring(1, fullText.Length - 1);
            string finalText = startText + firstLetter;
            this.statusStripContainer.Items["statusStripIpAddress"].Text = finalText;
        }

        private void ShakeForm() {
            Thread.Sleep(60);
            if (this.InvokeRequired) {
                _ = this.Invoke(new DelegateFunction(this.ShakeForm));
            }
            else {
                Point original = this.Location;
                var rnd = new Random(1337);
                const int shake_amplitude = 10;
                for (int i = 0; i < 40; i++) {
                    this.Location = new Point(
                        original.X + rnd.Next(-shake_amplitude, shake_amplitude),
                        original.Y + rnd.Next(-shake_amplitude, shake_amplitude)
                    );
                    Thread.Sleep(20);
                }

                this.Location = original;
            }
        }

        private void StatusStripDbName_Click(object sender, EventArgs e) {
            if (this._app.DebugMode) {
                _ = MessageBox.Show(
                    this._db.GetAllAvailableDbConnectionsString(),
                    "Koneksi Database",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

    }

}
