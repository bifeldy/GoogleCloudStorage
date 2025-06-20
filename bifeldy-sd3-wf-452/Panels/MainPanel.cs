/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Halaman Awal
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Extensions;
using bifeldy_sd3_lib_452.Libraries;
using bifeldy_sd3_lib_452.Models;
using bifeldy_sd3_lib_452.Utilities;

using GoogleCloudStorage.Components;
using GoogleCloudStorage.Forms;
using GoogleCloudStorage.Handlers;
using GoogleCloudStorage.Utilities;

namespace GoogleCloudStorage.Panels {

    public sealed partial class CMainPanel : UserControl {

        private readonly IApp _app;
        private readonly ILogger _logger;
        private readonly IDb _db;
        private readonly IConfig _config;
        private readonly IWinReg _winreg;
        private readonly IConverter _converter;
        private readonly IChiper _chiper;
        private readonly IGoogleCloudStorage _gcs;
        private readonly IBerkas _berkas;
        private readonly ICsv _csv;
        private readonly IPgpRsa _pgpRsa;

        private CMainForm mainForm;

        private bool isInitialized = false;

        public IProgress<string> LogReporter { get; set; } = null;

        private readonly Icon DEFAULT_ICON_FOLDER = DefaultIcons.FolderLarge;
        private readonly Icon DEFAULT_ICON_BUCKET = DefaultIcons.Extract("shell32.dll", 9, true);
        private readonly Icon DEFAULT_ICON_OBJECT = DefaultIcons.Extract("shell32.dll", 69, true);

        private ListViewColumnSorter lvColumnSorter;

        private readonly List<string> ctlExclBusy = new List<string> {
            "btnUpload",
            "btnExportLaporan",
            "btnDownload",
            "btnDdl",
            "btnHome",
            "btnRefresh",
            "txtLog",
            "txtDirPath",
            "txtFilter"
        };

        List<GcsBucket> allBuckets = null;
        List<GcsObject> allObjects = null;

        IProgress<dynamic> onGoingUploadProgress = null;
        IProgress<dynamic> onGoingDownloadProgress = null;
        IProgress<dynamic> onCompleteFailUploadProgress = null;
        IProgress<dynamic> onCompleteFailDownloadProgress = null;
        IProgress<string> onWriteLogProgress = null;

        public CMainPanel(
            IApp app,
            ILogger logger,
            IDb db,
            IConfig config,
            IWinReg winreg,
            IConverter converter,
            IChiper chiper,
            IGoogleCloudStorage gcs,
            IBerkas berkas,
            ICsv csv,
            IPgpRsa pgpRsa
        ) {
            this._app = app;
            this._logger = logger;
            this._db = db;
            this._config = config;
            this._winreg = winreg;
            this._converter = converter;
            this._chiper = chiper;
            this._gcs = gcs;
            this._berkas = berkas;
            this._csv = csv;
            this._pgpRsa = pgpRsa;

            this.InitializeComponent();
            this.OnInit();
        }

        public Label LabelStatus => this.lblStatus;

        public ProgressBar ProgressBarStatus => this.prgrssBrStatus;

        private void OnInit() {
            this.Dock = DockStyle.Fill;

            this.LogReporter = new Progress<string>(log => {
                this.txtLog.Text = log + this.txtLog.Text;
            });
        }

        private void ImgDomar_Click(object sender, EventArgs e) {
            this.mainForm.Width = 800 + 64 + 14;
            this.mainForm.Height = 600 + 64 + 7;
        }

        private async void CMainPanel_Load(object sender, EventArgs e) {
            if (!this.isInitialized) {

                this.mainForm = (CMainForm)this.Parent.Parent;
                this.mainForm.FormBorderStyle = FormBorderStyle.Sizable;
                this.mainForm.MaximizeBox = true;
                this.mainForm.MinimizeBox = true;

                this.appInfo.Text = this._app.AppName;
                string dcKode = null;
                string namaDc = null;
                await Task.Run(async () => {
                    dcKode = await this._db.GetKodeDc();
                    namaDc = await this._db.GetNamaDc();
                });
                this.userInfo.Text = $".: {dcKode} - {namaDc} :: {this._db.LoggedInUsername} :.";

                bool windowsStartup = this._config.Get<bool>("WindowsStartup", bool.Parse(this._app.GetConfig("windows_startup")));
                this.chkWindowsStartup.Checked = windowsStartup;

                this._logger.SetLogReporter(this.LogReporter);

                this.InitializeDataGridUpDownProgressStatus();
                this.InitializeProgressInfoReporter();

                this.lvColumnSorter = new ListViewColumnSorter();
                this.lvRemote.ListViewItemSorter = this.lvColumnSorter;

                this.CheckIsAdmin();
                this.CheckWeeklyUpload();

                this.timerQueue.Enabled = true;

                this.SetIdleBusyStatus(true);

                this.isInitialized = true;
            }

            this.SetIdleBusyStatus(this._app.IsIdle);
        }

        public void SetIdleBusyStatus(bool isIdle) {
            this._app.IsIdle = isIdle;
            this.LabelStatus.Text = $"Program {(isIdle ? "Idle" : "Sibuk")} ...";
            this.ProgressBarStatus.Style = isIdle ? ProgressBarStyle.Continuous : ProgressBarStyle.Marquee;
            this.EnableDisableControl(this.Controls, isIdle);
        }

        private void EnableDisableControl(ControlCollection controls, bool isIdle) {
            foreach (Control control in controls) {
                if (control is Button || control is CheckBox || control is DateTimePicker) {
                    if (!this.ctlExclBusy.Contains(control.Name)) {
                        control.Enabled = isIdle;
                    }
                }
                else if (control is TextBox tb) {
                    if (!this.ctlExclBusy.Contains(control.Name)) {
                        tb.ReadOnly = !isIdle;
                    }
                }
                else if (control is ListView) {
                    control.Enabled = isIdle;
                }
                else {
                    this.EnableDisableControl(control.Controls, isIdle);
                }
            }
        }

        private void ChkWindowsStartup_CheckedChanged(object sender, EventArgs e) {
            var cb = (CheckBox) sender;
            this._config.Set("WindowsStartup", cb.Checked);
            this._winreg.SetWindowsStartup(cb.Checked);
        }

        private void InitializeDataGridUpDownProgressStatus() {
            #region Initialize Data Grid View Column

            var dgViewCellStyle = new DataGridViewCellStyle {
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

            _ = this.dgQueue.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                HeaderText = "File Local",
                Name = "dgQueue_FileLocal",
                ReadOnly = true
            });
            _ = this.dgQueue.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                DefaultCellStyle = dgViewCellStyle,
                HeaderText = "Direction",
                Name = "dgQueue_Direction",
                ReadOnly = true
            });
            _ = this.dgQueue.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                HeaderText = "File Remote",
                Name = "dgQueue_FileRemote",
                ReadOnly = true
            });
            // dgQueue.Columns.Add(new DataGridViewButtonColumn {
            //     AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
            //     DefaultCellStyle = dgViewCellStyle,
            //     HeaderText = "Action",
            //     Name = "dgQueue_Cancel",
            //     Text = "Cancel",
            //     ReadOnly = true,
            //     FlatStyle = FlatStyle.Flat,
            //     UseColumnTextForButtonValue = true
            // });

            this.dgQueue.EnableHeadersVisualStyles = false;

            _ = this.dgOnProgress.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                HeaderText = "File Local",
                Name = "dgOnProgress_FileLocal",
                ReadOnly = true
            });
            _ = this.dgOnProgress.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                DefaultCellStyle = dgViewCellStyle,
                HeaderText = "Direction",
                Name = "dgOnProgress_Direction",
                ReadOnly = true
            });
            _ = this.dgOnProgress.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                HeaderText = "File Remote",
                Name = "dgOnProgress_FileRemote",
                ReadOnly = true
            });
            _ = this.dgOnProgress.Columns.Add(new DataGridViewProgressColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                DefaultCellStyle = dgViewCellStyle,
                HeaderText = "Progress",
                MinimumWidth = 100,
                Name = "dgOnProgress_Progress",
                ReadOnly = true
            });
            _ = this.dgOnProgress.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                HeaderText = "Speed",
                Name = "dgOnProgress_Speed",
                ReadOnly = true
            });
            _ = this.dgOnProgress.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                HeaderText = "Status",
                MinimumWidth = 100,
                Name = "dgOnProgress_Status",
                ReadOnly = true
            });
            // dgOnProgress.Columns.Add(new DataGridViewButtonColumn {
            //     AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
            //     DefaultCellStyle = dgViewCellStyle,
            //     HeaderText = "Action",
            //     Name = "dgOnProgress_Cancel",
            //     Text = "Cancel",
            //     ReadOnly = true,
            //     FlatStyle = FlatStyle.Flat,
            //     UseColumnTextForButtonValue = true
            // });

            this.dgOnProgress.EnableHeadersVisualStyles = false;

            _ = this.dgErrorFail.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                HeaderText = "File Local",
                Name = "dgErrorFail_FileLocal",
                ReadOnly = true
            });
            _ = this.dgErrorFail.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                DefaultCellStyle = dgViewCellStyle,
                HeaderText = "Direction",
                Name = "dgErrorFail_Direction",
                ReadOnly = true
            });
            _ = this.dgErrorFail.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                HeaderText = "File Remote",
                Name = "dgErrorFail_FileRemote",
                ReadOnly = true
            });
            _ = this.dgErrorFail.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                HeaderText = "Status",
                MinimumWidth = 100,
                Name = "dgErrorFail_Status",
                ReadOnly = true
            });
            // dgErrorFail.Columns.Add(new DataGridViewButtonColumn {
            //     AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
            //     DefaultCellStyle = dgViewCellStyle,
            //     HeaderText = "Action",
            //     Name = "dgErrorFail_Retry",
            //     Text = "Retry",
            //     ReadOnly = true,
            //     FlatStyle = FlatStyle.Flat,
            //     UseColumnTextForButtonValue = true
            // });

            this.dgErrorFail.EnableHeadersVisualStyles = false;

            _ = this.dgSuccess.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                HeaderText = "File Local",
                Name = "dgSuccess_FileLocal",
                ReadOnly = true
            });
            _ = this.dgSuccess.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                DefaultCellStyle = dgViewCellStyle,
                HeaderText = "Direction",
                Name = "dgSuccess_Direction",
                ReadOnly = true
            });
            _ = this.dgSuccess.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                HeaderText = "File Remote",
                Name = "dgSuccess_FileRemote",
                ReadOnly = true
            });
            _ = this.dgSuccess.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                HeaderText = "Status",
                MinimumWidth = 100,
                Name = "dgSuccess_Status",
                ReadOnly = true
            });

            this.dgSuccess.EnableHeadersVisualStyles = false;

            #endregion
        }

        private void InitializeProgressInfoReporter() {
            this.onGoingUploadProgress = new Progress<dynamic>(obj => {
                DataGridViewRow dataGridViewRow = obj.dgvr;
                FileInfo fileInfo = obj.fi;
                CGcsUploadProgress progressOld = obj.pOld;
                CGcsUploadProgress progressNew = obj.pNew;
                DateTime dateTime = obj.dt;

                DataGridViewCell dgOnProgress_Status = dataGridViewRow.Cells[this.dgOnProgress.Columns["dgOnProgress_Status"].Index];
                dgOnProgress_Status.Value = $"{progressNew.Status} ...";

                if (progressNew.Status == EGcsUploadStatus.Uploading) {
                    TimeSpan diff = DateTime.Now - dateTime;
                    if (progressOld != null) {
                        string transferSpeed = $"0 KB/s";
                        if (diff.TotalMilliseconds > 0) {
                            transferSpeed = $"{(progressNew.BytesSent - progressOld.BytesSent) / diff.TotalMilliseconds:0.00} KB/s";
                        }

                        dataGridViewRow.Cells[this.dgOnProgress.Columns["dgOnProgress_Speed"].Index].Value = transferSpeed;
                    }

                    if (fileInfo.Length > 0) {
                        decimal transferPercentage = decimal.Parse($"{(decimal) 100 * progressNew.BytesSent / fileInfo.Length:0.00}");
                        dataGridViewRow.Cells[this.dgOnProgress.Columns["dgOnProgress_Progress"].Index].Value = transferPercentage;
                    }

                    if (progressNew.BytesSent > 0 && diff.TotalSeconds > 0) {
                        var etaSeconds = TimeSpan.FromSeconds((int)((decimal) fileInfo.Length / progressNew.BytesSent / (decimal) diff.TotalSeconds));
                        dgOnProgress_Status.Value += $" {etaSeconds.ToEtaString()}";
                    }
                }
                else if (progressNew.Status == EGcsUploadStatus.Completed || progressNew.Status == EGcsUploadStatus.Failed) {
                    this.onCompleteFailUploadProgress.Report(new {
                        dgvr = dataGridViewRow,
                        fi = fileInfo,
                        pOld = progressOld,
                        pNew = progressNew,
                        dt = dateTime
                    });
                }

                this.ClearDataGridSelection();
            });

            this.onCompleteFailUploadProgress = new Progress<dynamic>(async obj => {
                DataGridViewRow dataGridViewRow = obj.dgvr;
                CGcsUploadProgress progress = obj.pNew;

                DataGridView dgv = null;
                if (progress.Status == EGcsUploadStatus.Completed) {
                    dgv = this.dgSuccess;
                }
                else {
                    dgv = this.dgErrorFail;
                }

                string errorMessage = string.Empty;
                if (progress.Exception != null) {
                    errorMessage = $" :: {progress.Exception.Message}";
                    this._logger.WriteError(progress.Exception);
                }

                _ = dgv.Rows.Add(
                    dataGridViewRow.Cells[this.dgOnProgress.Columns["dgOnProgress_FileLocal"].Index].Value,
                    dataGridViewRow.Cells[this.dgOnProgress.Columns["dgOnProgress_Direction"].Index].Value,
                    dataGridViewRow.Cells[this.dgOnProgress.Columns["dgOnProgress_FileRemote"].Index].Value,
                    $"{progress.Status}{errorMessage}"
                );
                this.dgOnProgress.Rows.RemoveAt(this.dgOnProgress.Rows.IndexOf(dataGridViewRow));

                if (this.dgOnProgress.Rows.Count <= 0 && this._app.IsIdle) {
                    string path = this.txtDirPath.Text;
                    await this.LoadObjects(path);
                }

                this.ClearDataGridSelection();
            });

            this.onGoingDownloadProgress = new Progress<dynamic>(obj => {
                DataGridViewRow dataGridViewRow = obj.dgvr;
                ulong fileSize = obj.sz;
                CGcsDownloadProgress progressOld = obj.pOld;
                CGcsDownloadProgress progressNew = obj.pNew;
                DateTime dateTime = obj.dt;

                DataGridViewCell dgOnProgress_Status = dataGridViewRow.Cells[this.dgOnProgress.Columns["dgOnProgress_Status"].Index];
                dgOnProgress_Status.Value = $"{progressNew.Status} ...";

                if (progressNew.Status == EGcsDownloadStatus.Downloading) {
                    TimeSpan diff = DateTime.Now - dateTime;
                    if (progressOld != null) {
                        string transferSpeed = $"0 KB/s";
                        if (diff.TotalMilliseconds > 0) {
                            transferSpeed = $"{(progressNew.BytesDownloaded - progressOld.BytesDownloaded) / diff.TotalMilliseconds:0.00} KB/s";
                        }

                        dataGridViewRow.Cells[this.dgOnProgress.Columns["dgOnProgress_Speed"].Index].Value = transferSpeed;
                    }

                    if (fileSize > 0) {
                        decimal transferPercentage = decimal.Parse($"{(decimal) 100 * progressNew.BytesDownloaded / fileSize:0.00}");
                        dataGridViewRow.Cells[this.dgOnProgress.Columns["dgOnProgress_Progress"].Index].Value = transferPercentage;
                    }

                    if (progressNew.BytesDownloaded > 0 && diff.TotalSeconds > 0) {
                        var etaSeconds = TimeSpan.FromSeconds((int)((decimal) fileSize / progressNew.BytesDownloaded / (decimal) diff.TotalSeconds));
                        dgOnProgress_Status.Value += $" {etaSeconds.ToEtaString()}";
                    }
                }
                else if (progressNew.Status == EGcsDownloadStatus.Completed || progressNew.Status == EGcsDownloadStatus.Failed) {
                    this.onCompleteFailDownloadProgress.Report(new {
                        dgvr = dataGridViewRow,
                        sz = fileSize,
                        pOld = progressOld,
                        pNew = progressNew,
                        dt = dateTime
                    });
                }

                this.ClearDataGridSelection();
            });

            this.onCompleteFailDownloadProgress = new Progress<dynamic>(async obj => {
                DataGridViewRow dataGridViewRow = obj.dgvr;
                CGcsDownloadProgress progress = obj.pNew;

                DataGridView dgv = null;
                if (progress.Status == EGcsDownloadStatus.Completed) {
                    dgv = this.dgSuccess;
                }
                else {
                    dgv = this.dgErrorFail;
                }

                string errorMessage = string.Empty;
                if (progress.Exception != null) {
                    errorMessage = $" :: {progress.Exception.Message}";
                    this._logger.WriteError(progress.Exception);
                }

                _ = dgv.Rows.Add(
                    dataGridViewRow.Cells[this.dgOnProgress.Columns["dgOnProgress_FileLocal"].Index].Value,
                    dataGridViewRow.Cells[this.dgOnProgress.Columns["dgOnProgress_Direction"].Index].Value,
                    dataGridViewRow.Cells[this.dgOnProgress.Columns["dgOnProgress_FileRemote"].Index].Value,
                    $"{progress.Status}{errorMessage}"
                );
                this.dgOnProgress.Rows.RemoveAt(this.dgOnProgress.Rows.IndexOf(dataGridViewRow));

                if (this.dgOnProgress.Rows.Count <= 0 && this._app.IsIdle) {
                    string path = this.txtDirPath.Text;
                    await this.LoadObjects(path);
                }

                this.ClearDataGridSelection();
            });

            this.onWriteLogProgress = new Progress<string>(text => {
                this.txtLog.Text += Environment.NewLine + text + Environment.NewLine;
            });
        }

        private async void CheckIsAdmin() {
            try {
                string isAdm = await this._db.SQLite_ExecScalar<string>(@"
                    SELECT isadm FROM users
                    WHERE uname = :uname
                ", new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "uname", VALUE = this._db.LoggedInUsername }
                });
                if (isAdm.ToUpper() == "Y") {
                    this.btnDownload.Visible = true;
                    this.btnDdl.Visible = true;
                    this.lblExp.Visible = true;
                    this.dtpExp.Visible = true;
                }
            }
            catch (Exception ex) {
                this._logger.WriteError(ex);
                _ = MessageBox.Show(ex.Message, "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void CheckWeeklyUpload() {
            try {
                DateTime curr = DateTime.Now;
                int year = curr.Year;
                int week = curr.GetWeekOfYear();
                int month = curr.Month;
                int rowCount = 0;
                int pending = 0;
                int completed = 0;

                string uploadPendingInfo = "List Belum Selesai" + Environment.NewLine;
                string uploadCompleteInfo = "List Sudah Selesai" + Environment.NewLine;

                string fp = this._config.Get<string>("SigNamePattern", this._app.GetConfig("sig_name_pattern"));
                string file_ext = this._config.Get<string>("SelectFileExt", this._app.GetConfig("select_file_ext"));

                using (DbDataReader reader = await this._db.Sqlite_ExecReaderAsync(@"
                    SELECT year, week, dc_kode, file_1_name, file_1_date, file_2_name, file_2_date
                    FROM upload_log
                    WHERE year = :year AND week = :week AND month = :month
                ", new List<CDbQueryParamBind> {
                    new CDbQueryParamBind { NAME = "year", VALUE = year },
                    new CDbQueryParamBind { NAME = "week", VALUE = week },
                    new CDbQueryParamBind { NAME = "month", VALUE = month }
                })) {
                    while (reader.Read()) {
                        string uploadInfo = Environment.NewLine;

                        if (!reader.IsDBNull(2)) {
                            rowCount++;
                            string dc_kode = reader.GetString(2);
                            uploadInfo += $"[#] {dc_kode}";
                            if (!reader.IsDBNull(3) && !reader.IsDBNull(4) && !reader.IsDBNull(5) && !reader.IsDBNull(6)) {
                                completed++;
                                uploadInfo += Environment.NewLine + $"[#] " + reader.GetString(3) + " :: " + new DateTime(long.Parse(reader.GetInt64(4).ToString())).ToString();
                                uploadInfo += Environment.NewLine + $"[#] " + reader.GetString(5) + " :: " + new DateTime(long.Parse(reader.GetInt64(6).ToString())).ToString();
                                uploadInfo += Environment.NewLine;
                                uploadCompleteInfo += uploadInfo;
                            }
                            else {
                                pending++;

                                if (!reader.IsDBNull(3) && !reader.IsDBNull(4)) {
                                    string fn1 = reader.GetString(3);

                                    Match rgx = Regex.Match(fn1, fp);
                                    if (!rgx.Success) {
                                        throw new Exception($"Terjadi kesalahan, mohon fresh install ulang program baru");
                                    }

                                    uploadInfo += Environment.NewLine + $"[#] {fn1} :: " + new DateTime(long.Parse(reader.GetInt64(4).ToString())).ToString();
                                    uploadInfo += Environment.NewLine + $"[#] FILE idm_*table_{rgx.Groups[2].Value}_{rgx.Groups[3].Value}{rgx.Groups[4].Value} BELUM DI UNGGAH";
                                }
                                else if (!reader.IsDBNull(5) && !reader.IsDBNull(6)) {
                                    // -- SAMPE BISA MASUK KE SINI SIH DB DI OTAK ATIK PASTI !!
                                    string fn2 = reader.GetString(5);

                                    Match rgx = Regex.Match(fn2, fp);
                                    if (!rgx.Success) {
                                        throw new Exception($"Terjadi kesalahan, mohon fresh install ulang program baru");
                                    }

                                    uploadInfo += Environment.NewLine + $"[#] FILE idm_metadata_{rgx.Groups[2].Value}_{rgx.Groups[3].Value}{rgx.Groups[4].Value} BELUM DI UNGGAH";
                                    uploadInfo += Environment.NewLine + $"[#] {fn2} :: " + new DateTime(long.Parse(reader.GetInt64(6).ToString())).ToString();
                                }
                                else {
                                    uploadInfo += Environment.NewLine + $"[#] FILE idm_metadata_{dc_kode}_xxxxxx{file_ext} BELUM DI UNGGAH";
                                    uploadInfo += Environment.NewLine + $"[#] FILE idm_*table_{dc_kode}_xxxxxx{file_ext} BELUM DI UNGGAH";
                                }

                                uploadInfo += Environment.NewLine;
                                uploadPendingInfo += uploadInfo;
                            }
                        }
                    }

                    reader.Close();
                }

                this._db.OraPg_MsSqlLiteCloseAllConnection();
                if (rowCount == 0) {
                    string info = "Belum Ada File Yang Di Unggah Minggu Ini";
                    if (this.onWriteLogProgress != null) {
                        this.onWriteLogProgress.Report(info);
                    }

                    _ = MessageBox.Show(info, "Weekly Checker", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else {
                    if (completed > 0) {
                        if (this.onWriteLogProgress != null) {
                            this.onWriteLogProgress.Report(uploadCompleteInfo);
                        }

                        _ = MessageBox.Show(uploadCompleteInfo, "Weekly Checker", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    if (pending > 0) {
                        if (this.onWriteLogProgress != null) {
                            this.onWriteLogProgress.Report(uploadPendingInfo);
                        }

                        _ = MessageBox.Show(uploadPendingInfo, "Weekly Checker", MessageBoxButtons.OK, MessageBoxIcon.Question);
                    }
                }
            }
            catch (Exception ex) {
                this._logger.WriteError(ex);
                _ = MessageBox.Show(ex.Message, "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearDataGridSelection() {
            this.dgQueue.ClearSelection();
            this.dgOnProgress.ClearSelection();
            this.dgSuccess.ClearSelection();
            this.dgErrorFail.ClearSelection();
        }

        private void UpdateLastActivity() {
            DateTime timeStamp = DateTime.Now;
            this.lblDate.Text = $"{timeStamp:dd-MM-yyyy}";
            this.lblTime.Text = $"{timeStamp:HH:mm:ss}";

            this.dtpExp.MaxDate = DateTime.Now.AddDays(5);
            this.dtpExp.MinDate = DateTime.Now.AddHours(1);
            this.dtpExp.Value = this.dtpExp.MinDate;
        }

        private void LvRemote_ColumnClick(object sender, ColumnClickEventArgs e) {
            if (e.Column == this.lvColumnSorter.SortColumn) {
                if (this.lvColumnSorter.Order == SortOrder.Ascending) {
                    this.lvColumnSorter.Order = SortOrder.Descending;
                }
                else {
                    this.lvColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else {
                this.lvColumnSorter.SortColumn = e.Column;
                this.lvColumnSorter.Order = SortOrder.Ascending;
            }

            this.lvRemote.Sort();
        }

        private async Task LoadBuckets() {
            this.SetIdleBusyStatus(false);
            try {
                this.btnHome.Enabled = false;
                this.btnRefresh.Enabled = false;
                this.txtFilter.ReadOnly = true;
                this.btnUpload.Enabled = false;
                this.btnExportLaporan.Enabled = false;
                this.btnDownload.Enabled = false;
                this.btnDdl.Enabled = false;
                await Task.Run(async () => {
                    this.allBuckets = await this._gcs.ListAllBuckets();
                });
                this.imageList.Images.Clear();
                this.imageList.Images.Add(this.DEFAULT_ICON_BUCKET);
                this.lvRemote.SmallImageList = this.imageList;
                this.lvRemote.LargeImageList = this.imageList;
                var columnHeader = new ColumnHeader[] {
                    new ColumnHeader { Text = "Nama Bucket", Width = this.lvRemote.Size.Width - 167 - 10 },
                    new ColumnHeader { Text = "Diperbarui", Width = 167 }
                };
                this.lvRemote.Columns.Clear();
                this.lvRemote.Columns.AddRange(columnHeader);
                ListViewItem[] lvis = this.allBuckets.Where(bckt => {
                    return bckt.Name.ToUpper().Contains(this.txtFilter.Text.ToUpper());
                }).OrderBy(bckt => bckt.Name).Select(bckt => {
                    var lvi = new ListViewItem { Tag = bckt, Text = bckt.Name, ImageIndex = 0 };
                    _ = lvi.SubItems.Add(bckt.Updated.ToString());
                    return lvi;
                }).ToArray();
                this.lvRemote.Items.Clear();
                this.lvRemote.Items.AddRange(lvis);
                this.UpdateLastActivity();
            }
            catch (Exception ex) {
                this._logger.WriteError(ex);
                _ = MessageBox.Show(ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally {
                this.txtDirPath.Text = string.Empty;
                this.btnRefresh.Enabled = true;
                this.txtFilter.ReadOnly = false;
            }

            this.SetIdleBusyStatus(true);
        }

        private async Task LoadObjects(string path) {
            this.SetIdleBusyStatus(false);
            try {
                this.btnHome.Enabled = false;
                this.btnRefresh.Enabled = false;
                this.btnUpload.Enabled = false;
                this.txtFilter.ReadOnly = true;
                this.btnExportLaporan.Enabled = false;
                this.btnDownload.Enabled = false;
                this.btnDdl.Enabled = false;
                await Task.Run(async () => {
                    this.allObjects = await this._gcs.ListAllObjects(path);
                });
                this.imageList.Images.Clear();
                this.imageList.Images.Add(this.DEFAULT_ICON_OBJECT);
                this.lvRemote.SmallImageList = this.imageList;
                this.lvRemote.LargeImageList = this.imageList;
                var columnHeader = new ColumnHeader[] {
                    new ColumnHeader { Text = "Nama Berkas", Width = this.lvRemote.Size.Width - (96 + 192 + (2 * 10)) },
                    new ColumnHeader { Text = "Ukuran", Width = 96 },
                    new ColumnHeader { Text = "Tanggal Selesai Upload", Width = 192 }
                };
                this.lvRemote.Columns.Clear();
                this.lvRemote.Columns.AddRange(columnHeader);
                ListViewItem[] lvis = this.allObjects.Where(obj => {
                    return obj.Name.ToUpper().Contains(this.txtFilter.Text.ToUpper());
                }).OrderByDescending(obj => obj.Updated).Select(obj => {
                    var lvi = new ListViewItem { Tag = obj, Text = obj.Name, ImageIndex = 0 };
                    _ = lvi.SubItems.Add(this._converter.FormatByteSizeHumanReadable((long)obj.Size));
                    _ = lvi.SubItems.Add(obj.Updated.ToString());
                    return lvi;
                }).ToArray();
                this.lvRemote.Items.Clear();
                this.lvRemote.Items.AddRange(lvis);
                this.UpdateLastActivity();
            }
            catch (Exception ex) {
                this._logger.WriteError(ex);
                _ = MessageBox.Show(ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally {
                this.txtDirPath.Text = path;
                this.btnHome.Enabled = true;
                this.btnRefresh.Enabled = true;
                this.txtFilter.ReadOnly = false;
                this.btnUpload.Enabled = true;
                this.btnExportLaporan.Enabled = true;
            }

            this.SetIdleBusyStatus(true);
        }

        private async void BtnConnect_Click(object sender, EventArgs e) {
            try {
                string filePath = null;
                using (var fd = new OpenFileDialog()) {
                    fd.InitialDirectory = this._app.AppLocation;
                    fd.RestoreDirectory = true;
                    fd.CheckFileExists = true;
                    fd.Filter = "credentials (*.txt,*.json)|*.txt;*.json";
                    fd.Title = "Open credentials(.txt|.json)";
                    if (fd.ShowDialog() != DialogResult.OK) {
                        throw new Exception("Gagal memuat file credentials.json");
                    }

                    filePath = fd.FileName;
                }

                this._gcs.LoadCredential(filePath, filePath.ToLower().EndsWith(".txt"));
                if (this._app.IsIdle) {
                    await this.LoadBuckets();
                }
            }
            catch (Exception ex) {
                this._logger.WriteError(ex);
                _ = MessageBox.Show(ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LvRemote_SelectedIndexChanged(object sender, EventArgs e) {
            bool enabled = false;
            if (this.lvRemote.SelectedItems.Count > 0) {
                dynamic item = this.lvRemote.SelectedItems[0].Tag;
                if (item is GcsObject || item.Kind == "storage#object") {
                    enabled = true;
                }
            }

            this.btnDownload.Enabled = enabled;
            this.btnDdl.Enabled = enabled;
        }

        private async void LvRemote_MouseDoubleClick(object sender, MouseEventArgs e) {
            dynamic item = this.lvRemote.SelectedItems[0].Tag;
            if (item is GcsBucket || item.Kind == "storage#bucket") {
                // Name = Id Bucket Sama Aja Kayaknya (?)
                if (this._app.IsIdle) {
                    await LoadObjects(item.Name);
                }
            }
        }

        private async void BtnHome_Click(object sender, EventArgs e) {
            if (this._app.IsIdle) {
                await this.LoadBuckets();
            }
        }

        private async void BtnRefresh_Click(object sender, EventArgs e) {
            string path = this.txtDirPath.Text;
            if (this._app.IsIdle) {
                if (string.IsNullOrEmpty(path)) {
                    await this.LoadBuckets();
                }
                else {
                    await this.LoadObjects(path);
                }
            }
        }

        private void TxtFilter_KeyDown(object sender, KeyEventArgs e) {
            switch (e.KeyCode) {
                case Keys.Enter:
                    this.BtnRefresh_Click(sender, EventArgs.Empty);
                    break;
            }
        }

        private async void BtnExportLaporan_Click(object sender, EventArgs e) {
            this.SetIdleBusyStatus(false);
            bool btnDownloadEnabledBefore = this.btnDownload.Enabled;
            bool btnDdlEnabledBefore = this.btnDdl.Enabled;
            try {
                string path = this.txtDirPath.Text;
                if (!string.IsNullOrEmpty(path)) {
                    this.btnHome.Enabled = false;
                    this.btnRefresh.Enabled = false;
                    this.txtFilter.ReadOnly = true;
                    this.btnUpload.Enabled = false;
                    this.btnExportLaporan.Enabled = false;
                    this.btnDownload.Enabled = false;
                    this.btnDdl.Enabled = false;

                    await Task.Run(async () => {
                        List<GcsObject> objects = await this._gcs.ListAllObjects(path);

                        string fp = this._config.Get<string>("SigNamePattern", this._app.GetConfig("sig_name_pattern"));

                        var file1 = new List<GcsObject>();
                        var file2 = new List<GcsObject>();
                        foreach (GcsObject obj in objects) {
                            Match rgx = Regex.Match(obj.Name.ToLower(), fp);
                            if (!rgx.Success) {
                                string _fp = fp.Replace(".7z.sig", string.Empty);
                                rgx = Regex.Match(obj.Name.ToLower(), _fp);
                            }

                            if (rgx.Success && rgx.Groups.Count == 5) {
                                if (rgx.Groups[1].Value.ToLower() == "metadata") {
                                    file1.Add(obj);
                                }
                                else {
                                    file2.Add(obj);
                                }
                            }
                        }

                        string[] csvColumn = new string[] {
                            "KODE_DC",
                            "TAHUN",
                            "BULAN",
                            "MINGGU",
                            "FILE_1_NAME",
                            "FILE_1_SIZE_BYTES",
                            "FILE_1_DATE_TIME",
                            "FILE_2_NAME",
                            "FILE_2_SIZE_BYTES",
                            "FILE_2_DATE_TIME"
                        };

                        string exportPath = Path.Combine(this._csv.CsvFolderPath, $"{DateTime.Now:yyy-MM-dd_HH-mm-ss}.csv");
                        using (var writer = new StreamWriter(exportPath)) {
                            writer.WriteLine(string.Join("|", csvColumn).ToUpper());
                            foreach (GcsObject f1 in file1.OrderBy(f => f.Name)) {
                                string fileName = f1.Name.ToLower().Replace("\\", "/").Split('/').Last();
                                var fileDate = DateTime.ParseExact(fileName.Split('_').Last().Split('.').First().ToLower(), "yyMMdd", CultureInfo.InvariantCulture);
                                string fn1 = "idm_metadata_g".ToLower();
                                string newLine = string.Empty;
                                if (fileName.StartsWith(fn1)) {
                                    int index = fileName.IndexOf(fn1);
                                    string xxx_xxxxxx = (index < 0) ? fileName : fileName.Remove(index, fn1.Length);
                                    string dc_kode = $"G{xxx_xxxxxx.Substring(0, 3)}".ToUpper();
                                    newLine += $"{dc_kode}|{fileDate.Year}|{fileDate.Month}|{fileDate.GetWeekOfMonth()}|{fileName}|{f1.Size}|{f1.Updated?.ToLocalTime()}|";
                                    GcsObject f2 = file2.Find(f => f.Name.EndsWith(xxx_xxxxxx));
                                    newLine += (f2 is null) ? "||" : $"{f2.Name}|{f2.Size}|{f2.Updated?.ToLocalTime()}";
                                    writer.WriteLine(newLine);
                                }
                            }
                        }

                        _ = Process.Start(new ProcessStartInfo {
                            Arguments = this._csv.CsvFolderPath,
                            FileName = "explorer.exe"
                        });
                    });
                }
            }
            catch (Exception ex) {
                this._logger.WriteError(ex);
                _ = MessageBox.Show(ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally {
                this.btnHome.Enabled = true;
                this.btnRefresh.Enabled = true;
                this.txtFilter.ReadOnly = false;
                this.btnUpload.Enabled = true;
                this.btnExportLaporan.Enabled = true;
                this.btnDownload.Enabled = btnDownloadEnabledBefore;
                this.btnDdl.Enabled = btnDdlEnabledBefore;
            }

            this.SetIdleBusyStatus(true);
        }

        private async void BtnDdl_Click(object sender, EventArgs e) {
            this.SetIdleBusyStatus(false);
            try {
                dynamic item = this.lvRemote.SelectedItems[0].Tag;
                if (item is GcsObject || item.Kind == "storage#object") {
                    string ddl = string.Empty;
                    await Task.Run(async () => {
                        ddl = await this._gcs.CreateDownloadUrlSigned(item, this.dtpExp.Value);
                    });
                    if (string.IsNullOrEmpty(ddl)) {
                        _ = MessageBox.Show("Gagal Membuat URL Unduhan", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else {
                        Clipboard.SetText(ddl);
                        _ = MessageBox.Show(ddl, $"(CopyPaste) Expired :: {this.dtpExp.Value}", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _ = Process.Start(ddl);
                    }
                }
            }
            catch (Exception ex) {
                this._logger.WriteError(ex);
                _ = MessageBox.Show(ex.Message, "Direct Download Link Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            this.SetIdleBusyStatus(true);
        }

        private async void BtnUpload_Click(object sender, EventArgs e) {
            this.SetIdleBusyStatus(false);
            try {
                string file_ext = this._config.Get<string>("SelectFileExt", this._app.GetConfig("select_file_ext"));

                string selectedLocalFilePath = null;
                using (var fd = new OpenFileDialog()) {
                    fd.InitialDirectory = this._app.AppLocation;
                    fd.RestoreDirectory = true;
                    fd.CheckFileExists = true;
                    fd.Filter = $"Sig files (*{file_ext})|*{file_ext}";
                    fd.Title = $"Select idm_metadata_gxxx_xxxxxx{file_ext} | idm_*table_gxxx_xxxxxx{file_ext}";
                    fd.DefaultExt = file_ext;
                    if (fd.ShowDialog() != DialogResult.OK) {
                        throw new Exception($"Gagal membuka file idm_***_gxxx_xxxxxx{file_ext}");
                    }

                    selectedLocalFilePath = fd.FileName;
                }

                DateTime curr = DateTime.Now;
                int year = curr.Year;
                int week = curr.GetWeekOfYear();
                int month = curr.Month;

                string fn = selectedLocalFilePath.Replace("\\", "/").Split('/').Last().ToLower();

                string fp = this._config.Get<string>("SigNamePattern", this._app.GetConfig("sig_name_pattern"));
                Match rgx = Regex.Match(fn, fp);
                if (!rgx.Success) {
                    throw new Exception($"Format nama file {fn} salah, diperbolehkan {fp}");
                }

                if (rgx.Groups[4].Value != file_ext) {
                    throw new Exception($"Hanya file {file_ext} yang diperbolehkan");
                }

                var fileDate = DateTime.ParseExact(rgx.Groups[3].Value, "yyMMdd", CultureInfo.InvariantCulture);
                if (week != fileDate.GetWeekOfYear() || month != fileDate.Month) {
                    throw new Exception($"File harus di minggu & bulan yang sama dengan tanggal hari ini");
                }

                string file1name = string.Empty;
                DateTime file1date = DateTime.MinValue;
                string file2name = string.Empty;
                DateTime file2date = DateTime.MinValue;

                int rowCount = 0;
                using (
                    DbDataReader reader = await this._db.Sqlite_ExecReaderAsync(@"
                        SELECT file_1_name, file_1_date, file_2_name, file_2_date
                        FROM upload_log
                        WHERE year = :year AND week = :week AND month = :month AND dc_kode = :dc_kode
                    ", new List<CDbQueryParamBind> {
                        new CDbQueryParamBind { NAME = "year", VALUE = year },
                        new CDbQueryParamBind { NAME = "week", VALUE = week },
                        new CDbQueryParamBind { NAME = "month", VALUE = month },
                        new CDbQueryParamBind { NAME = "dc_kode", VALUE = rgx.Groups[2].Value }
                    })
                ) {
                    while (reader.Read()) {
                        rowCount++;
                        if (!reader.IsDBNull(0)) {
                            file1name = reader.GetString(0);
                        }

                        if (!reader.IsDBNull(1)) {
                            file1date = new DateTime(long.Parse(reader.GetInt64(1).ToString()));
                        }

                        if (!reader.IsDBNull(2)) {
                            file2name = reader.GetString(2);
                        }

                        if (!reader.IsDBNull(3)) {
                            file2date = new DateTime(long.Parse(reader.GetInt64(3).ToString()));
                        }
                    }

                    reader.Close();
                }

                this._db.OraPg_MsSqlLiteCloseAllConnection();
                if (rowCount == 0) {
                    _ = await this._db.SQLite_ExecQuery(@"
                        INSERT INTO upload_log(year, week, month, dc_kode)
                        VALUES(:year, :week, :month, :dc_kode)
                    ", new List<CDbQueryParamBind> {
                        new CDbQueryParamBind { NAME = "year", VALUE = year },
                        new CDbQueryParamBind { NAME = "week", VALUE = week },
                        new CDbQueryParamBind { NAME = "month", VALUE = month },
                        new CDbQueryParamBind { NAME = "dc_kode", VALUE = rgx.Groups[2].Value }
                    });
                }

                if (!string.IsNullOrEmpty(file1name) && !string.IsNullOrEmpty(file2name)) {
                    string msg = $"Upload minggu ini sudah selesai{Environment.NewLine}{Environment.NewLine}{file1name}{Environment.NewLine}{file1date}{Environment.NewLine}{Environment.NewLine}{file2name}{Environment.NewLine}{file2date}";
                    _ = MessageBox.Show(msg, $"Upload Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (string.IsNullOrEmpty(file1name) && string.IsNullOrEmpty(file2name)) {
                    await this.Upload12(selectedLocalFilePath, rgx);
                }
                else if (string.IsNullOrEmpty(file1name) && !string.IsNullOrEmpty(file2name)) {
                    await this.Upload1(selectedLocalFilePath, file2name.ToLower());
                }
                else if (!string.IsNullOrEmpty(file1name) && string.IsNullOrEmpty(file2name)) {
                    await this.Upload2(selectedLocalFilePath, file1name.ToLower());
                }
                else {
                    _ = MessageBox.Show($"File bermasalah{Environment.NewLine}Silahkan coba lagi dengan file lain", $"File Check Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex) {
                this._logger.WriteError(ex);
                _ = MessageBox.Show(ex.Message, "Upload Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            this.SetIdleBusyStatus(true);
        }

        private async Task Upload12(string selectedLocalFilePath, Match rgx) {
            if (rgx.Groups[1].Value.ToLower() != "metadata") {
                throw new Exception($"Harap memilih file metadata `idm_metadata_gxxx_xxxxxx{rgx.Groups[4].Value}`");
            }

            // Check Nama File 2 :: idm_*table_gxxx_xxxxxx.ext :: Ada Filenya Gak Di 1 Folder Pasangan
            string file2pattern = $"idm_*table_{rgx.Groups[2].Value}_{rgx.Groups[3].Value}{rgx.Groups[4].Value}";
            var fp = Directory.EnumerateFiles(
                Path.GetDirectoryName(selectedLocalFilePath),
                file2pattern,
                SearchOption.TopDirectoryOnly
            ).ToList();

            if (fp.Count <= 0 || fp.Count > 1) {
                throw new Exception($"Pastikan hanya 1 nama file pasangan yang cocok dengan pola {file2pattern}");
            }

            await this.AddQueue(selectedLocalFilePath.Replace("\\", "/"), fp.First().Replace("\\", "/"));
        }

        private async Task Upload1(string selectedLocalFilePath, string file2name) {
            string fp = this._config.Get<string>("SigNamePattern", this._app.GetConfig("sig_name_pattern"));
            Match rgx = Regex.Match(file2name, fp);
            if (!rgx.Success) {
                throw new Exception($"Terjadi kesalahan, mohon fresh install ulang program baru");
            }

            // Check Nama File 1 :: idm_metadata_gxxx_xxxxxx.ext :: Sesuai Tidak Dengan File 2
            string file1name = selectedLocalFilePath.Replace("\\", "/").Split('/').Last().ToLower();
            string target = $"idm_metadata_{rgx.Groups[2].Value}_{rgx.Groups[3].Value}{rgx.Groups[4].Value}".ToLower();
            if (file1name != target) {
                throw new Exception($"File pasangan tidak sesuai, silahkan pilih {target}");
            }

            await this.AddQueue(selectedLocalFilePath.Replace("\\", "/"));
        }

        private async Task Upload2(string selectedLocalFilePath, string file1name) {
            string fp = this._config.Get<string>("SigNamePattern", this._app.GetConfig("sig_name_pattern"));
            Match rgx = Regex.Match(file1name, fp);
            if (!rgx.Success) {
                throw new Exception($"Terjadi kesalahan, mohon fresh install ulang program baru");
            }

            // Check Nama File 2 :: idm_*table_gxxx_xxxxxx.ext :: Sesuai Tidak Dengan File 1
            string file2name = selectedLocalFilePath.Replace("\\", "/").Split('/').Last().ToLower();
            string target = $"table_{rgx.Groups[2].Value}_{rgx.Groups[3].Value}{rgx.Groups[4].Value}".ToLower();
            if (!file2name.EndsWith(target)) {
                throw new Exception($"File pasangan tidak sesuai, silahkan pilih idm_*{target}");
            }

            await this.AddQueue(selectedLocalFilePath.Replace("\\", "/"));
        }

        private bool CheckProgressIsRunning(string localPath, string remotePath) {
            foreach (DataGridViewRow row in this.dgQueue.Rows) {
                if (
                    row.Cells[this.dgQueue.Columns["dgQueue_FileLocal"].Index].Value.ToString().Equals(localPath) &&
                    row.Cells[this.dgQueue.Columns["dgQueue_FileRemote"].Index].Value.ToString().Equals(remotePath)
                ) {
                    return true;
                }
            }

            foreach (DataGridViewRow row in this.dgOnProgress.Rows) {
                if (
                    row.Cells[this.dgOnProgress.Columns["dgOnProgress_FileLocal"].Index].Value.ToString().Equals(localPath) &&
                    row.Cells[this.dgOnProgress.Columns["dgOnProgress_FileRemote"].Index].Value.ToString().Equals(remotePath)
                ) {
                    return true;
                }
            }

            return false;
        }

        private async Task AddQueue(string sigFilePath, string fileNextPath = null) {
            string[] arrRemoteDir = this.txtDirPath.Text.Split('/');
            string targetFolderId = arrRemoteDir[arrRemoteDir.Length - 1];

            string sigFileName = sigFilePath.Replace("\\", "/").Split('/').Last().ToLower();

            string realFilePath = sigFilePath.Replace(".sig", string.Empty);
            string realFileName = realFilePath.Replace("\\", "/").Split('/').Last().ToLower();

            if (!File.Exists(realFilePath)) {
                throw new Exception($"File data {realFileName} tidak ditemukan, harap taruh di dalam folder yang sama dengan file sign {sigFileName}");
            }

            var fileInfo = new FileInfo(realFilePath);

            if (this.CheckProgressIsRunning(realFilePath, $"Google://{targetFolderId}/{fileInfo.Name}")) {
                throw new Exception($"Proses {fileInfo.Name} sedang berjalan");
            }

            // string allowedMime = this._config.Get<string>("AllowedFileMime", this._app.GetConfig("allowed_file_mime"));
            // string selectedMime = this._chiper.GetMime(realFilePath);
            // if (string.IsNullOrEmpty(allowedMime) || selectedMime != allowedMime) {
            //     throw new Exception("Jenis MiMe file salah");
            // }

            string publicKeyFilePath = Path.Combine(this._app.AppLocation, "publicKey.asc");
            if (!this._pgpRsa.IsValidPublicKeyFile(publicKeyFilePath)) {
                throw new Exception("File verifikator tidak valid");
            }

            if (!this._pgpRsa.VerifyFileDetachedWithPublicKeyFile(realFilePath, publicKeyFilePath, sigFilePath)) {
                throw new Exception("File rusak / corrupt / tanda tangan tidak sesuai");
            }

            DialogResult dialogResult = DialogResult.Yes;
            if (!this.cbReplaceIfExist.Checked) {
                foreach (GcsObject obj in this.allObjects) {
                    if (obj.Name.ToLower().Contains(fileInfo.Name.ToLower())) {
                        dialogResult = MessageBox.Show($"Tetap lanjut upload '{fileInfo.Name}' ?", "File Already Exist", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        break;
                    }
                }
            }

            if (dialogResult == DialogResult.Yes) {
                int idx = this.dgQueue.Rows.Add(realFilePath, "===>>>", $"Google://{targetFolderId}/{fileInfo.Name}");
                DataGridViewRow dgvrQueue = this.dgQueue.Rows[idx];

                this._logger.WriteInfo("Antrian",
                    $"\"{dgvrQueue.Cells[this.dgQueue.Columns["dgQueue_FileLocal"].Index].Value}\" " +
                    $"{dgvrQueue.Cells[this.dgQueue.Columns["dgQueue_Direction"].Index].Value} " +
                    $"\"{dgvrQueue.Cells[this.dgQueue.Columns["dgQueue_FileRemote"].Index].Value}\" "
                );
            }

            if (!string.IsNullOrEmpty(fileNextPath)) {
                await this.AddQueue(fileNextPath.Replace("\\", "/"));
            }
        }

        private async void TimerQueue_Tick(object sender, EventArgs e) {
            if ((this.numMaxProcess.Value <= 1 && this.dgOnProgress.Rows.Count > 0) || this.dgQueue.Rows.Count <= 0) {
                return;
            }

            DataGridViewRow dgvrQueue = this.dgQueue.Rows[0];

            string fileLocal = dgvrQueue.Cells[this.dgQueue.Columns["dgQueue_FileLocal"].Index].Value.ToString();
            string fileUploadDownload = dgvrQueue.Cells[this.dgQueue.Columns["dgQueue_Direction"].Index].Value.ToString();
            string fileRemote = dgvrQueue.Cells[this.dgQueue.Columns["dgQueue_FileRemote"].Index].Value.ToString();

            this.dgQueue.Rows.Remove(dgvrQueue);

            int idx = this.dgOnProgress.Rows.Add(fileLocal, fileUploadDownload, fileRemote);
            DataGridViewRow dgvrOnProgress = this.dgOnProgress.Rows[idx];
            dgvrOnProgress.Cells[this.dgOnProgress.Columns["dgOnProgress_Status"].Index].Value = "Checking File ...";

            if (fileUploadDownload == "===>>>") {

                var fileInfo = new FileInfo(fileLocal);

                string googleUrl = string.Empty;
                if (fileRemote.StartsWith("Google://")) {
                    googleUrl = fileRemote.Replace("Google://", "");
                }

                string targetFolderId = googleUrl.Split('/').First();

                await Task.Run(async () => {
                    try {
                        string file_md5 = this._chiper.CalculateMD5File(fileInfo.FullName);
                        CGcsUploadProgress uploaded = null;

                        using (Stream stream = File.OpenRead(fileLocal)) {
                            GcsMediaUpload mediaUpload = this._gcs.GenerateUploadMedia(fileInfo, targetFolderId, stream);
                            Uri uploadSession = null;

                            using (DbDataReader reader = await this._db.Sqlite_ExecReaderAsync(@"
                                SELECT file_md5, file_session, file_date
                                FROM upload_chunk
                                WHERE file_md5 = :file_md5
                            ", new List<CDbQueryParamBind> {
                                new CDbQueryParamBind { NAME = "file_md5", VALUE = file_md5 }
                            })) {
                                while (reader.Read()) {
                                    if (!reader.IsDBNull(2)) {
                                        var file_date = new DateTime(long.Parse(reader.GetInt64(2).ToString()));
                                        if (DateTime.Now.Ticks <= file_date.AddDays(5).Ticks) {
                                            if (!reader.IsDBNull(2)) {
                                                uploadSession = new Uri(reader.GetString(1));
                                            }
                                        }
                                    }
                                }

                                reader.Close();
                            }

                            this._db.OraPg_MsSqlLiteCloseAllConnection();

                            if (uploadSession == null) {
                                _ = await this._db.SQLite_ExecQuery(@"
                                    DELETE FROM upload_chunk
                                    WHERE file_md5 = :file_md5
                                ", new List<CDbQueryParamBind> {
                                    new CDbQueryParamBind { NAME = "file_md5", VALUE = file_md5 }
                                });
                                uploadSession = await this._gcs.CreateUploadUri(mediaUpload);
                                _ = await this._db.SQLite_ExecQuery(@"
                                    INSERT INTO upload_chunk(file_md5, file_session, file_date)
                                    VALUES(:file_md5, :file_session, :file_date)
                                ", new List<CDbQueryParamBind> {
                                    new CDbQueryParamBind { NAME = "file_md5", VALUE = file_md5 },
                                    new CDbQueryParamBind { NAME = "file_session", VALUE = uploadSession.ToString() },
                                    new CDbQueryParamBind { NAME = "file_date", VALUE = DateTime.Now.Ticks }
                                });
                            }

                            CGcsUploadProgress progressOld = null;
                            DateTime dateTime = DateTime.Now;
                            uploaded = await this._gcs.UploadFile(mediaUpload, uploadSession, (progressNew) => {
                                this.onGoingUploadProgress.Report(new {
                                    dgvr = dgvrOnProgress,
                                    fi = fileInfo,
                                    pOld = progressOld,
                                    pNew = progressNew,
                                    dt = dateTime
                                });
                                progressOld = progressNew;
                                dateTime = DateTime.Now;
                            }, true);
                        }

                        if (uploaded.Exception == null && uploaded.Status == EGcsUploadStatus.Completed) {
                            try {
                                string sql = @"UPDATE upload_log SET";
                                var param = new List<CDbQueryParamBind>();

                                string fn = fileInfo.Name.Replace("\\", "/").Split('/').Last().ToLower();

                                string fp = this._config.Get<string>("SigNamePattern", this._app.GetConfig("sig_name_pattern"));
                                fp = fp.Replace(".7z.sig", string.Empty);

                                Match rgx = Regex.Match(fn, fp);
                                if (!rgx.Success) {
                                    throw new Exception($"Terjadi kesalahan, mohon fresh install ulang program baru");
                                }

                                if (rgx.Groups[1].Value.ToLower() == "metadata") {
                                    sql += " file_1_name = :file_1_name, file_1_date = :file_1_date";
                                    param.Add(new CDbQueryParamBind { NAME = "file_1_name", VALUE = fileInfo.Name.ToLower() });
                                    param.Add(new CDbQueryParamBind { NAME = "file_1_date", VALUE = DateTime.Now.Ticks });
                                }
                                else {
                                    sql += " file_2_name = :file_2_name, file_2_date = :file_2_date";
                                    param.Add(new CDbQueryParamBind { NAME = "file_2_name", VALUE = fileInfo.Name.ToLower() });
                                    param.Add(new CDbQueryParamBind { NAME = "file_2_date", VALUE = DateTime.Now.Ticks });
                                }

                                sql += " WHERE year = :year AND week = :week AND month = :month AND dc_kode = :dc_kode";
                                string filedate = fileInfo.Name.Replace("\\", "/").Split('/').Last().Split('_').Last().Split('.').First().ToLower();
                                var fileDate = DateTime.ParseExact(filedate, "yyMMdd", CultureInfo.InvariantCulture);
                                param.Add(new CDbQueryParamBind { NAME = "year", VALUE = fileDate.Year });
                                param.Add(new CDbQueryParamBind { NAME = "week", VALUE = fileDate.GetWeekOfYear() });
                                param.Add(new CDbQueryParamBind { NAME = "month", VALUE = fileDate.Month });
                                param.Add(new CDbQueryParamBind { NAME = "dc_kode", VALUE = rgx.Groups[2].Value });
                                _ = await this._db.SQLite_ExecQuery(sql, param);

                                _ = await this._db.SQLite_ExecQuery(@"
                                    DELETE FROM upload_chunk
                                    WHERE file_md5 = :file_md5
                                ", new List<CDbQueryParamBind> {
                                    new CDbQueryParamBind { NAME = "file_md5", VALUE = file_md5 }
                                });
                            }
                            catch (Exception ex) {
                                this._logger.WriteError(ex);
                                _ = MessageBox.Show(ex.Message, "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }

                            try {
                                DialogResult dialogResult =
                                    this.cbDeleteOnComplete.Checked
                                        ? DialogResult.Yes
                                        : MessageBox.Show(
                                            $"Delete File '{fileInfo.FullName}'",
                                            "Upload Finished",
                                            MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Question
                                        );
                                if (dialogResult == DialogResult.Yes) {
                                    if (fileInfo.Exists) {
                                        fileInfo.Delete();
                                    }
                                }
                            }
                            catch (Exception ex) {
                                this._logger.WriteError(ex);
                                _ = MessageBox.Show(ex.Message, "Delete Local Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (TaskCanceledException ex) {
                        this._logger.WriteError(ex);
                        _ = MessageBox.Show("Koneksi terputus", "Network Timeout", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex) {
                        this._logger.WriteError(ex);
                        _ = MessageBox.Show(ex.Message, "Upload Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                });

            }

            else if (fileUploadDownload == "<<<===") {

                // TODO ::

            }
        }

        private void BtnDownload_Click(object sender, EventArgs e) {
            this.SetIdleBusyStatus(false);
            try {
                dynamic item = this.lvRemote.SelectedItems[0].Tag;
                if (item is GcsObject || item.Kind == "storage#object") {
                    string selectedLocalFilePath = null;
                    string file_ext = this._config.Get<string>("SelectFileExt", this._app.GetConfig("select_file_ext"));

                    file_ext = file_ext.Replace(".sig", string.Empty);

                    using (var fd = new SaveFileDialog()) {
                        fd.InitialDirectory = this._app.AppLocation;
                        fd.RestoreDirectory = true;
                        fd.CheckPathExists = true;
                        fd.FileName = item.Name;
                        fd.Filter = $"Archive files (*{file_ext})|*{file_ext}";
                        fd.Title = $"Simpan {item.Name}";
                        fd.DefaultExt = file_ext;
                        if (fd.ShowDialog() != DialogResult.OK) {
                            throw new Exception($"Gagal menentukan lokasi penyimpanan file {item.Name}");
                        }

                        selectedLocalFilePath = fd.FileName;
                    }

                    string[] arrRemoteDir = this.txtDirPath.Text.Split('/');
                    string folderId = arrRemoteDir[arrRemoteDir.Length - 1];
                    string targetPathLocal = selectedLocalFilePath.Replace("\\", "/");

                    if (this.CheckProgressIsRunning(targetPathLocal, $"Google://{folderId}/{item.Name}")) {
                        throw new Exception($"Proses {item.Name} sedang berjalan");
                    }

                    int idx = this.dgOnProgress.Rows.Add(targetPathLocal, "<<<===", $"Google://{folderId}/{item.Name}");
                    DataGridViewRow dataGridViewRow = this.dgOnProgress.Rows[idx];

                    _ = Task.Run(async () => {
                        try {
                            CGcsDownloadProgress progressOld = null;
                            DateTime dateTime = DateTime.Now;

                            long existingFileSize = 0;
                            string fileTempPath = Path.Combine(this._berkas.DownloadFolderPath, item.Name);
                            if (File.Exists(fileTempPath)) {
                                existingFileSize = new FileInfo(fileTempPath).Length;
                            }

                            await this._gcs.DownloadFile((GcsObject)item, selectedLocalFilePath, (progressNew) => {
                                progressNew.BytesDownloaded += existingFileSize;
                                this.onGoingDownloadProgress.Report(new {
                                    dgvr = dataGridViewRow,
                                    sz = item.Size,
                                    pOld = progressOld,
                                    pNew = progressNew,
                                    dt = dateTime
                                });
                                progressOld = progressNew;
                                dateTime = DateTime.Now;
                            }, true);

                            if (File.Exists(selectedLocalFilePath)) {
                                File.Delete(selectedLocalFilePath);
                            }

                            File.Move(Path.Combine(this._berkas.DownloadFolderPath, item.Name), selectedLocalFilePath);

                            _ = Process.Start(new ProcessStartInfo {
                                Arguments = Path.GetDirectoryName(selectedLocalFilePath),
                                FileName = "explorer.exe"
                            });
                        }
                        catch (TaskCanceledException ex) {
                            this._logger.WriteError(ex);
                            _ = MessageBox.Show("Koneksi terputus", "Network Timeout", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        catch (Exception ex) {
                            this._logger.WriteError(ex);
                            _ = MessageBox.Show(ex.Message, "Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    });
                }
            }
            catch (Exception ex) {
                this._logger.WriteError(ex);
                _ = MessageBox.Show(ex.Message, "Save File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            this.SetIdleBusyStatus(true);
        }

    }

}
