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
            this._updater = updater;
            this._config = config;
            this._app = app;
            this._db = db;

            this.InitializeComponent();
            this.OnInit();
        }

        public Label LoadingInformation => this.loadingInformation;

        private void OnInit() {
            this.loadingInformation.Text = "Sedang Mengecek Program ...";
            this.Dock = DockStyle.Fill;
        }

        private void CCekProgram_Load(object sender, EventArgs e) {
            if (!this.isInitialized) {

                this.mainForm = (CMainForm)this.Parent.Parent;

                this.CheckProgram();

                this.isInitialized = true;
            }
        }

        private async void CheckProgram() {
            if (this._db.LocalDbOnly) {
                await Task.Run(async () => {
                    try {
                        await this._updater.UpdateSqliteDatabase();
                    }
                    catch (Exception ex) {
                        MessageBox.Show(ex.Message, "Program Checker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this._app.Exit();
                    }
                });
            }

            bool autoDb = this._config.Get<bool>("AutoDb", bool.Parse(this._app.GetConfig("auto_db")));

            // First DB Run + Check Connection
            bool dbAvailable = false;
            // Check Jenis DC
            string jenisDc = null;
            await Task.Run(async () => {
                try {
                    jenisDc = await this._db.GetJenisDc();
                    dbAvailable = true;
                }
                catch (Exception ex1) {
                    if (autoDb) {
                        this._app.IsUsingPostgres = false;
                        try {
                            jenisDc = await this._db.GetJenisDc();
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
                string dbInfo = this._db.LocalDbOnly ? "[SQLT]" : $"[{(this._app.IsUsingPostgres ? "PG" : "ORCL")}+MSSQL]";

                this.mainForm.StatusStripContainer.Items["statusStripDbName"].Text = this._db.DbName;
                this.mainForm.Text = $"{dbInfo} {this.mainForm.Text}";

                if (this._app.ListDcCanUse.Count == 0 || this._app.ListDcCanUse.Contains(jenisDc)) {

                    // Check Version
                    string responseCekProgram = null;
                    await Task.Run(async () => {
                        responseCekProgram = await this._db.CekVersi();
                    });
                    if (responseCekProgram.ToUpper() == "OKE") {
                        this.ShowLoginPanel();
                    }
                    else if (responseCekProgram.ToUpper().Contains("VERSI")) {
                        this.loadingInformation.Text = "Memperbarui Otomatis ...";
                        bool updated = false;
                        if (!this._app.IsSkipUpdate) {
                            string verStr = responseCekProgram.Split('=').Last().Trim().Split('v').Last();
                            int verInt = int.Parse(verStr);
                            await Task.Run(() => {
                                updated = this._updater.CheckUpdater(verInt);
                            });
                        }

                        if (!updated) {
                            MessageBox.Show(
                                "Gagal Update Otomatis" + Environment.NewLine + "Silahkan Hubungi IT SSD 03 Untuk Ambil Program Baru" + Environment.NewLine + Environment.NewLine + responseCekProgram,
                                "Program Checker",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            this._app.Exit();
                        }
                    }
                    else {
                        MessageBox.Show(responseCekProgram, "Program Checker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this._app.Exit();
                    }
                }
                else {
                    MessageBox.Show(
                        $"Program Hanya Dapat Di Jalankan Di DC {Environment.NewLine}{string.Join(", ", this._app.ListDcCanUse.ToArray())}",
                        "Program Checker",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    this._app.Exit();
                }
            }
            else {
                this._app.Exit();
            }
        }

        private void ShowLoginPanel() {

            // Create & Show `Login` Panel
            try {
                CLogin login = CProgram.Bifeldyz.Resolve<CLogin>();
                if (!this.mainForm.PanelContainer.Controls.ContainsKey("CLogin")) {
                    this.mainForm.PanelContainer.Controls.Add(CProgram.Bifeldyz.Resolve<CLogin>());
                }

                this.mainForm.PanelContainer.Controls["CLogin"].BringToFront();
                bool bypassLogin = this._config.Get<bool>("BypassLogin", bool.Parse(this._app.GetConfig("bypass_login")));
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
