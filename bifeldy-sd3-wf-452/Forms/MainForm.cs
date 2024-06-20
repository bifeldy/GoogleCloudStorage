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
            _app = app;
            _db = db;
            _config = config;

            InitializeComponent();
            OnInit();
        }

        public Panel PanelContainer => panelContainer;

        public StatusStrip StatusStripContainer => statusStripContainer;

        private void OnInit() {

            // Reclaim Space From Right StatusBar Grip
            statusStripContainer.Padding = new Padding(
                statusStripContainer.Padding.Left,
                statusStripContainer.Padding.Top,
                statusStripContainer.Padding.Left,
                statusStripContainer.Padding.Bottom
            );

            Text = _app.AppName;

            sysTrayNotifyIcon.Text = _app.AppName;
            sysTrayNotifyIcon.DoubleClick += SysTray_DoubleClick;

            sysTrayToolStripMenuItemApp.Text = $"{_app.CurrentProcess.Id} (0x{_app.CurrentProcess.MainModule.BaseAddress})";
            sysTrayToolStripMenuItemNICs.Image = SystemIcons.Question.ToBitmap();
            sysTrayToolStripMenuItemDatabases.Image = SystemIcons.Warning.ToBitmap();
        }

        private async void CMainForm_Load(object sender, EventArgs e) {
            if (!isInitialized) {

                statusStripContainer.Items["statusStripIpAddress"].Text = "- .: " + string.Join(", ", _app.GetAllIpAddress()) + " :. -";
                statusStripContainer.Items["statusStripAppVersion"].Text = $"v{_app.AppVersion}";

                ShowDbSelectorPanel();
                await Task.Run((Action) ShakeForm);

                isInitialized = true;
            }
        }

        private void CMainForm_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.UserClosing) {
                bool minimizeOnClose = _config.Get<bool>("MinimizeOnClose", bool.Parse(_app.GetConfig("minimize_on_close")));
                if (!minimizeOnClose) {
                    SysTray_MenuExit(sender, EventArgs.Empty);
                }
                else {
                    lastFormWindowState = WindowState;
                    sysTrayNotifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                    sysTrayNotifyIcon.BalloonTipTitle = "Aplikasi Berjalan Di Belakang Layar";
                    sysTrayNotifyIcon.BalloonTipText = "Klik Icon 2x Untuk Membuka Kembali";
                    sysTrayNotifyIcon.BalloonTipClicked += SysTray_DoubleClick;
                    sysTrayNotifyIcon.ShowBalloonTip(500);
                    WindowState = FormWindowState.Minimized;
                    ShowInTaskbar = false;
                    Hide();
                    e.Cancel = true;
                }
            }
        }

        private void ShowDbSelectorPanel() {

            // Create And Show `DbSelector` Panel
            try {
                CDbSelector dbSelector = null;
                if (!panelContainer.Controls.ContainsKey("CDbSelector")) {
                    dbSelector = CProgram.Bifeldyz.ResolveClass<CDbSelector>();
                    panelContainer.Controls.Add(dbSelector);
                }
                panelContainer.Controls["CDbSelector"].BringToFront();
                if (_app.ListDcCanUse.Count == 1 && _app.ListDcCanUse.Contains("HO")) {
                    dbSelector.DchoOnlyBypass(this, EventArgs.Empty);
                }
                else {
                    bool autoDb = _config.Get<bool>("AutoDb", bool.Parse(_app.GetConfig("auto_db")));
                    if (autoDb) {
                        dbSelector.AutoRunModeDefaultPostgre(this, EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "Terjadi Kesalahan! (｡>﹏<｡)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void HideLogo() {
            imgLogo.Visible = false;
        }

        private void StatusStripIpAddress_Click(object sender, EventArgs e) {
            string[] ipsMacs = _app.GetIpMacAddress()
                .Select(d => $"{d.DESCRIPTION}\r\n{d.MAC_ADDRESS}\r\n{d.IP_V4_ADDRESS}\r\n{d.IP_V6_ADDRESS}\r\n\r\n")
                .ToArray();
            string ipMac = string.Join(Environment.NewLine, ipsMacs).Replace("\r\n\r\n", "\r\n");
            MessageBox.Show(ipMac, "Network Interface Card", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void SysTray_DoubleClick(object sender, EventArgs e) {
            Show();
            ShowInTaskbar = true;
            WindowState = lastFormWindowState;
            Activate();
        }

        public void SysTray_MenuExit(object sender, EventArgs e) {
            string title = "Good Bye~ (｡>﹏<｡)";
            string msg = _app.Author + Environment.NewLine + "© 2023 :: IT S/SD 03";
            MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            sysTrayNotifyIcon.Dispose();
            _app.Exit();
        }

        private void TimerIpAddress_Tick(object sender, EventArgs e) {
            string fullText = statusStripContainer.Items["statusStripIpAddress"].Text;
            string firstLetter = fullText.Substring(0, 1);
            string startText = fullText.Substring(1, fullText.Length - 1);
            string finalText = startText + firstLetter;
            statusStripContainer.Items["statusStripIpAddress"].Text = finalText;
        }

        private void ShakeForm() {
            Thread.Sleep(60);
            if (InvokeRequired) {
                Invoke(new DelegateFunction(ShakeForm));
            }
            else {
                Point original = Location;
                Random rnd = new Random(1337);
                const int shake_amplitude = 10;
                for (int i = 0; i < 40; i++) {
                    Location = new Point(
                        original.X + rnd.Next(-shake_amplitude, shake_amplitude),
                        original.Y + rnd.Next(-shake_amplitude, shake_amplitude)
                    );
                    Thread.Sleep(20);
                }
                Location = original;
            }
        }

        private void StatusStripDbName_Click(object sender, EventArgs e) {
            if (_app.DebugMode) {
                MessageBox.Show(
                    _db.GetAllAvailableDbConnectionsString(),
                    "Koneksi Database",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

    }

}
