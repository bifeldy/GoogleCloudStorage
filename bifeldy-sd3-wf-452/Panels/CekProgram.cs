/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Cek Versi, IP, etc.
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Utilities;

using GoogleCloudStorage.Forms;
using GoogleCloudStorage.Handlers;
using GoogleCloudStorage.Utilities;

namespace GoogleCloudStorage.Panels {

    public sealed partial class CCekProgram : UserControl {

        private readonly IUpdater _updater;
        private readonly IConfig _config;

        private readonly IApp _app;
        private readonly IDb _db;

        private CMainForm mainForm;

        private bool isInitialized = false;

        public CCekProgram(IUpdater updater, IApp app, IDb db, IConfig config) {
            _updater = updater;
            _config = config;
            _app = app;
            _db = db;

            InitializeComponent();
            OnInit();
        }

        public Label LoadingInformation => loadingInformation;

        private void OnInit() {
            loadingInformation.Text = "Sedang Mengecek Program ...";
            Dock = DockStyle.Fill;
        }

        private void CCekProgram_Load(object sender, EventArgs e) {
            if (!isInitialized) {

                mainForm = (CMainForm) Parent.Parent;

                CheckProgram();

                isInitialized = true;
            }
        }

        private async void CheckProgram() {
            if (_db.LocalDbOnly) {
                await Task.Run(async () => {
                    try {
                        await _updater.UpdateSqliteDatabase();
                    }
                    catch (Exception ex) {
                        MessageBox.Show(ex.Message, "Program Checker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _app.Exit();
                    }
                });
            }

            bool autoDb = _config.Get<bool>("AutoDb", bool.Parse(_app.GetConfig("auto_db")));

            // First DB Run + Check Connection
            bool dbAvailable = false;
            // Check Jenis DC
            string jenisDc = null;
            await Task.Run(async () => {
                try {
                    jenisDc = await _db.GetJenisDc();
                    dbAvailable = true;
                }
                catch (Exception ex1) {
                    if (autoDb) {
                        _app.IsUsingPostgres = false;
                        try {
                            jenisDc = await _db.GetJenisDc();
                            dbAvailable = true;
                        }
                        catch (Exception ex2) {
                            MessageBox.Show(ex2.Message, "Auto Run Checker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else {
                        MessageBox.Show(ex1.Message, "Program Checker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            });
            if (dbAvailable) {
                string dbInfo = _db.LocalDbOnly ? "[SQLT]" : $"[{(_app.IsUsingPostgres ? "PG" : "ORCL")}+MSSQL]";

                mainForm.StatusStripContainer.Items["statusStripDbName"].Text = _db.DbName;
                mainForm.Text = $"{dbInfo} {mainForm.Text}";

                if (_app.ListDcCanUse.Count == 0 || _app.ListDcCanUse.Contains(jenisDc)) {

                    // Check Version
                    string responseCekProgram = null;
                    await Task.Run(async () => {
                        responseCekProgram = await _db.CekVersi();
                    });
                    if (responseCekProgram.ToUpper() == "OKE") {
                        ShowLoginPanel();
                    }
                    else if (responseCekProgram.ToUpper().Contains("VERSI")) {
                        loadingInformation.Text = "Memperbarui Otomatis ...";
                        bool updated = false;
                        if (!_app.IsSkipUpdate) {
                            string verStr = responseCekProgram.Split('=').Last().Trim().Split('v').Last();
                            int verInt = int.Parse(verStr);
                            await Task.Run(() => {
                                updated = _updater.CheckUpdater(verInt);
                            });
                        }
                        if (!updated) {
                            MessageBox.Show(
                                "Gagal Update Otomatis" + Environment.NewLine + "Silahkan Hubungi IT SSD 03 Untuk Ambil Program Baru" + Environment.NewLine + Environment.NewLine + responseCekProgram,
                                "Program Checker",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            _app.Exit();
                        }
                    }
                    else {
                        MessageBox.Show(responseCekProgram, "Program Checker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _app.Exit();
                    }
                }
                else {
                    MessageBox.Show(
                        $"Program Hanya Dapat Di Jalankan Di DC {Environment.NewLine}{string.Join(", ", _app.ListDcCanUse.ToArray())}",
                        "Program Checker",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    _app.Exit();
                }
            }
            else {
                _app.Exit();
            }
        }

        private void ShowLoginPanel() {

            // Create & Show `Login` Panel
            try {
                CLogin login = CProgram.Bifeldyz.ResolveClass<CLogin>();
                if (!mainForm.PanelContainer.Controls.ContainsKey("CLogin")) {
                    mainForm.PanelContainer.Controls.Add(CProgram.Bifeldyz.ResolveClass<CLogin>());
                }
                mainForm.PanelContainer.Controls["CLogin"].BringToFront();
                bool bypassLogin = _config.Get<bool>("BypassLogin", bool.Parse(_app.GetConfig("bypass_login")));
                if (bypassLogin) {
                    login.ProcessLogin();
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "Terjadi Kesalahan! (｡>﹏<｡)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }

}
