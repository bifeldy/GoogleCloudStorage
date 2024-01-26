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
using System.Text;
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

        private CMainForm mainForm;

        private bool isInitialized = false;

        public IProgress<string> LogReporter { get; set; } = null;

        Icon DEFAULT_ICON_FOLDER = DefaultIcons.FolderLarge;
        Icon DEFAULT_ICON_BUCKET = DefaultIcons.Extract("shell32.dll", 9, true);
        Icon DEFAULT_ICON_OBJECT = DefaultIcons.Extract("shell32.dll", 69, true);

        private List<string> ctlExclBusy = new List<string> {
            "btnConnect",
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
            IBerkas berkas
        ) {
            _app = app;
            _logger = logger;
            _db = db;
            _config = config;
            _winreg = winreg;
            _converter = converter;
            _chiper = chiper;
            _gcs = gcs;
            _berkas = berkas;

            InitializeComponent();
            OnInit();
        }

        public Label LabelStatus => lblStatus;

        public ProgressBar ProgressBarStatus => prgrssBrStatus;

        private void OnInit() {
            Dock = DockStyle.Fill;

            LogReporter = new Progress<string>(log => {
                txtLog.Text = log + txtLog.Text;
            });
        }

        private void ImgDomar_Click(object sender, EventArgs e) {
            mainForm.Width = 800 + 64 + 14;
            mainForm.Height = 600 + 64 + 7;
        }

        private async void CMainPanel_Load(object sender, EventArgs e) {
            if (!isInitialized) {

                mainForm = (CMainForm) Parent.Parent;
                mainForm.FormBorderStyle = FormBorderStyle.Sizable;
                mainForm.MaximizeBox = true;
                mainForm.MinimizeBox = true;

                appInfo.Text = _app.AppName;
                string dcKode = null;
                string namaDc = null;
                await Task.Run(async () => {
                    dcKode = await _db.GetKodeDc();
                    namaDc = await _db.GetNamaDc();
                });
                userInfo.Text = $".: {dcKode} - {namaDc} :: {_db.LoggedInUsername} :.";

                bool windowsStartup = _config.Get<bool>("WindowsStartup", bool.Parse(_app.GetConfig("windows_startup")));
                chkWindowsStartup.Checked = windowsStartup;

                _logger.SetLogReporter(LogReporter);

                InitializeDataGridUpDownProgressStatus();
                InitializeProgressInfoReporter();

                CheckWeeklyUpload();

                timerQueue.Enabled = true;

                SetIdleBusyStatus(true);

                isInitialized = true;
            }

            SetIdleBusyStatus(_app.IsIdle);
        }

        public void SetIdleBusyStatus(bool isIdle) {
            _app.IsIdle = isIdle;
            LabelStatus.Text = $"Program {(isIdle ? "Idle" : "Sibuk")} ...";
            ProgressBarStatus.Style = isIdle ? ProgressBarStyle.Continuous : ProgressBarStyle.Marquee;
            EnableDisableControl(Controls, isIdle);
        }

        private void EnableDisableControl(ControlCollection controls, bool isIdle) {
            foreach (Control control in controls) {
                if (control is Button || control is CheckBox || control is DateTimePicker) {
                    if (!ctlExclBusy.Contains(control.Name)) {
                        control.Enabled = isIdle;
                    }
                }
                else if (control is TextBox) {
                    if (!ctlExclBusy.Contains(control.Name)) {
                        ((TextBox) control).ReadOnly = !isIdle;
                    }
                }
                else if (control is ListView) {
                    control.Enabled = isIdle;
                }
                else {
                    EnableDisableControl(control.Controls, isIdle);
                }
            }
        }

        private void ChkWindowsStartup_CheckedChanged(object sender, EventArgs e) {
            CheckBox cb = (CheckBox) sender;
            _config.Set("WindowsStartup", cb.Checked);
            _winreg.SetWindowsStartup(cb.Checked);
        }

        private void InitializeDataGridUpDownProgressStatus() {
            #region Initialize Data Grid View Column

            DataGridViewCellStyle dgViewCellStyle = new DataGridViewCellStyle {
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

            dgQueue.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                HeaderText = "File Local",
                Name = "dgQueue_FileLocal",
                ReadOnly = true
            });
            dgQueue.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                DefaultCellStyle = dgViewCellStyle,
                HeaderText = "Direction",
                Name = "dgQueue_Direction",
                ReadOnly = true
            });
            dgQueue.Columns.Add(new DataGridViewTextBoxColumn {
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

            dgQueue.EnableHeadersVisualStyles = false;

            dgOnProgress.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                HeaderText = "File Local",
                Name = "dgOnProgress_FileLocal",
                ReadOnly = true
            });
            dgOnProgress.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                DefaultCellStyle = dgViewCellStyle,
                HeaderText = "Direction",
                Name = "dgOnProgress_Direction",
                ReadOnly = true
            });
            dgOnProgress.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                HeaderText = "File Remote",
                Name = "dgOnProgress_FileRemote",
                ReadOnly = true
            });
            dgOnProgress.Columns.Add(new DataGridViewProgressColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                DefaultCellStyle = dgViewCellStyle,
                HeaderText = "Progress",
                MinimumWidth = 100,
                Name = "dgOnProgress_Progress",
                ReadOnly = true
            });
            dgOnProgress.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                HeaderText = "Speed",
                Name = "dgOnProgress_Speed",
                ReadOnly = true
            });
            dgOnProgress.Columns.Add(new DataGridViewTextBoxColumn {
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

            dgOnProgress.EnableHeadersVisualStyles = false;

            dgErrorFail.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                HeaderText = "File Local",
                Name = "dgErrorFail_FileLocal",
                ReadOnly = true
            });
            dgErrorFail.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                DefaultCellStyle = dgViewCellStyle,
                HeaderText = "Direction",
                Name = "dgErrorFail_Direction",
                ReadOnly = true
            });
            dgErrorFail.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                HeaderText = "File Remote",
                Name = "dgErrorFail_FileRemote",
                ReadOnly = true
            });
            dgErrorFail.Columns.Add(new DataGridViewTextBoxColumn {
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

            dgErrorFail.EnableHeadersVisualStyles = false;

            dgSuccess.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                HeaderText = "File Local",
                Name = "dgSuccess_FileLocal",
                ReadOnly = true
            });
            dgSuccess.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                DefaultCellStyle = dgViewCellStyle,
                HeaderText = "Direction",
                Name = "dgSuccess_Direction",
                ReadOnly = true
            });
            dgSuccess.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                HeaderText = "File Remote",
                Name = "dgSuccess_FileRemote",
                ReadOnly = true
            });
            dgSuccess.Columns.Add(new DataGridViewTextBoxColumn {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
                HeaderText = "Status",
                MinimumWidth = 100,
                Name = "dgSuccess_Status",
                ReadOnly = true
            });

            dgSuccess.EnableHeadersVisualStyles = false;

            #endregion
        }

        private void InitializeProgressInfoReporter() {
            onGoingUploadProgress = new Progress<dynamic>(obj => {
                DataGridViewRow dataGridViewRow = obj.dgvr;
                FileInfo fileInfo = obj.fi;
                CGcsUploadProgress progressOld = obj.pOld;
                CGcsUploadProgress progressNew = obj.pNew;
                DateTime dateTime = obj.dt;

                DataGridViewCell dgOnProgress_Status = dataGridViewRow.Cells[dgOnProgress.Columns["dgOnProgress_Status"].Index];
                dgOnProgress_Status.Value = $"{progressNew.Status} ...";

                if (progressNew.Status == EGcsUploadStatus.Uploading) {
                    string transferSpeed = $"0 KB/s";
                    TimeSpan diff = DateTime.Now - dateTime;
                    if (progressOld != null) {
                        transferSpeed = $"{((progressNew.BytesSent - progressOld.BytesSent) / diff.TotalMilliseconds):0.00} KB/s";
                    }
                    decimal transferPercentage = decimal.Parse($"{((decimal) 100 * progressNew.BytesSent / fileInfo.Length):0.00}");
                    dataGridViewRow.Cells[dgOnProgress.Columns["dgOnProgress_Progress"].Index].Value = transferPercentage;
                    dataGridViewRow.Cells[dgOnProgress.Columns["dgOnProgress_Speed"].Index].Value = transferSpeed;
                    TimeSpan etaSeconds = TimeSpan.FromSeconds((int)(((decimal) fileInfo.Length / progressNew.BytesSent) / (decimal)diff.TotalSeconds));
                    dgOnProgress_Status.Value += $" {etaSeconds.ToEtaString()}";
                }
                else if (progressNew.Status == EGcsUploadStatus.Completed || progressNew.Status == EGcsUploadStatus.Failed) {
                    onCompleteFailUploadProgress.Report(new {
                        dgvr = dataGridViewRow,
                        fi = fileInfo,
                        pOld = progressOld,
                        pNew = progressNew,
                        dt = dateTime
                    });
                }

                ClearDataGridSelection();
            });

            onCompleteFailUploadProgress = new Progress<dynamic>(async obj => {
                DataGridViewRow dataGridViewRow = obj.dgvr;
                CGcsUploadProgress progress = obj.pNew;

                DataGridView dgv = null;
                if (progress.Status == EGcsUploadStatus.Completed) {
                    dgv = dgSuccess;
                }
                else {
                    dgv = dgErrorFail;
                }

                string errorMessage = string.Empty;
                if (progress.Exception != null) {
                    errorMessage = $" :: {progress.Exception.Message}";
                    _logger.WriteError(progress.Exception);
                }

                dgv.Rows.Add(
                    dataGridViewRow.Cells[dgOnProgress.Columns["dgOnProgress_FileLocal"].Index].Value,
                    dataGridViewRow.Cells[dgOnProgress.Columns["dgOnProgress_Direction"].Index].Value,
                    dataGridViewRow.Cells[dgOnProgress.Columns["dgOnProgress_FileRemote"].Index].Value,
                    $"{progress.Status}{errorMessage}"
                );
                dgOnProgress.Rows.RemoveAt(dgOnProgress.Rows.IndexOf(dataGridViewRow));

                string path = txtDirPath.Text;
                await LoadObjects(path);

                ClearDataGridSelection();
            });

            onGoingDownloadProgress = new Progress<dynamic>(obj => {
                DataGridViewRow dataGridViewRow = obj.dgvr;
                ulong fileSize = obj.sz;
                CGcsDownloadProgress progressOld = obj.pOld;
                CGcsDownloadProgress progressNew = obj.pNew;
                DateTime dateTime = obj.dt;

                DataGridViewCell dgOnProgress_Status = dataGridViewRow.Cells[dgOnProgress.Columns["dgOnProgress_Status"].Index];
                dgOnProgress_Status.Value = $"{progressNew.Status} ...";

                if (progressNew.Status == EGcsDownloadStatus.Downloading) {
                    string transferSpeed = $"0 KB/s";
                    TimeSpan diff = DateTime.Now - dateTime;
                    if (progressOld != null) {
                        transferSpeed = $"{((progressNew.BytesDownloaded - progressOld.BytesDownloaded) / diff.TotalMilliseconds):0.00} KB/s";
                    }
                    decimal transferPercentage = decimal.Parse($"{((decimal) 100 * progressNew.BytesDownloaded / fileSize):0.00}");
                    dataGridViewRow.Cells[dgOnProgress.Columns["dgOnProgress_Progress"].Index].Value = transferPercentage;
                    dataGridViewRow.Cells[dgOnProgress.Columns["dgOnProgress_Speed"].Index].Value = transferSpeed;
                    TimeSpan etaSeconds = TimeSpan.FromSeconds((int)(((decimal) fileSize / progressNew.BytesDownloaded) / (decimal)diff.TotalSeconds));
                    dgOnProgress_Status.Value += $" {etaSeconds.ToEtaString()}";
                }
                else if (progressNew.Status == EGcsDownloadStatus.Completed || progressNew.Status == EGcsDownloadStatus.Failed) {
                    onCompleteFailDownloadProgress.Report(new {
                        dgvr = dataGridViewRow,
                        sz = fileSize,
                        pOld = progressOld,
                        pNew = progressNew,
                        dt = dateTime
                    });
                }

                ClearDataGridSelection();
            });

            onCompleteFailDownloadProgress = new Progress<dynamic>(async obj => {
                DataGridViewRow dataGridViewRow = obj.dgvr;
                CGcsDownloadProgress progress = obj.pNew;

                DataGridView dgv = null;
                if (progress.Status == EGcsDownloadStatus.Completed) {
                    dgv = dgSuccess;
                }
                else {
                    dgv = dgErrorFail;
                }

                string errorMessage = string.Empty;
                if (progress.Exception != null) {
                    errorMessage = $" :: {progress.Exception.Message}";
                    _logger.WriteError(progress.Exception);
                }

                dgv.Rows.Add(
                    dataGridViewRow.Cells[dgOnProgress.Columns["dgOnProgress_FileLocal"].Index].Value,
                    dataGridViewRow.Cells[dgOnProgress.Columns["dgOnProgress_Direction"].Index].Value,
                    dataGridViewRow.Cells[dgOnProgress.Columns["dgOnProgress_FileRemote"].Index].Value,
                    $"{progress.Status}{errorMessage}"
                );
                dgOnProgress.Rows.RemoveAt(dgOnProgress.Rows.IndexOf(dataGridViewRow));

                string path = txtDirPath.Text;
                await LoadObjects(path);

                ClearDataGridSelection();
            });

            onWriteLogProgress = new Progress<string>(text => {
                txtLog.Text += Environment.NewLine + text + Environment.NewLine;
            });
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
                string fn1 = _config.Get<string>("File1Name", _app.GetConfig("file_1_name"));
                string fn2 = _config.Get<string>("File2Name", _app.GetConfig("file_2_name"));
                using (DbDataReader reader = await _db.Sqlite_ExecReaderAsync(@"
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
                            uploadInfo += $"[#] " + reader.GetString(2);
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
                                    uploadInfo += Environment.NewLine + $"[#] " + reader.GetString(3) + " :: " + new DateTime(long.Parse(reader.GetInt64(4).ToString())).ToString();
                                    uploadInfo += Environment.NewLine + $"[#] FILE {fn2} BELUM DI UNGGAH";
                                }
                                else if (!reader.IsDBNull(5) && !reader.IsDBNull(6)) {
                                    // -- SAMPE BISA MASUK KE SINI SIH DB DI OTAK ATIK PASTI !!
                                    uploadInfo += Environment.NewLine + $"[#] FILE {fn1} BELUM DI UNGGAH";
                                    uploadInfo += Environment.NewLine + $"[#] " + reader.GetString(5) + " :: " + new DateTime(long.Parse(reader.GetInt64(6).ToString())).ToString();
                                }
                                else {
                                    uploadInfo += Environment.NewLine + $"[#] FILE {fn1} BELUM DI UNGGAH";
                                    uploadInfo += Environment.NewLine + $"[#] FILE {fn2} BELUM DI UNGGAH";
                                }
                                uploadInfo += Environment.NewLine;
                                uploadPendingInfo += uploadInfo;
                            }
                        }
                    }
                    reader.Close();
                }
                _db.CloseAllConnection();
                if (rowCount == 0) {
                    string info = "Belum Ada File Yang Di Unggah Minggu Ini";
                    if (onWriteLogProgress != null) {
                        onWriteLogProgress.Report(info);
                    }
                    MessageBox.Show(info, "Weekly Checker", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else {
                    if (completed > 0) {
                        if (onWriteLogProgress != null) {
                            onWriteLogProgress.Report(uploadCompleteInfo);
                        }
                        MessageBox.Show(uploadCompleteInfo, "Weekly Checker", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    if (pending > 0) {
                        if (onWriteLogProgress != null) {
                            onWriteLogProgress.Report(uploadPendingInfo);
                        }
                        MessageBox.Show(uploadPendingInfo, "Weekly Checker", MessageBoxButtons.OK, MessageBoxIcon.Question);
                    }
                }
            }
            catch (Exception ex) {
                _logger.WriteError(ex);
                MessageBox.Show(ex.Message, "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearDataGridSelection() {
            dgQueue.ClearSelection();
            dgOnProgress.ClearSelection();
            dgSuccess.ClearSelection();
            dgErrorFail.ClearSelection();
        }

        private void UpdateLastActivity() {
            DateTime timeStamp = DateTime.Now;
            lblDate.Text = $"{timeStamp:dd-MM-yyyy}";
            lblTime.Text = $"{timeStamp:HH:mm:ss}";

            dtpExp.MaxDate = DateTime.Now.AddDays(5);
            dtpExp.MinDate = DateTime.Now.AddHours(1);
            dtpExp.Value = dtpExp.MinDate;
        }

        private async Task LoadBuckets() {
            try {
                btnHome.Enabled = false;
                btnRefresh.Enabled = false;
                txtFilter.ReadOnly = true;
                btnUpload.Enabled = false;
                btnExportLaporan.Enabled = false;
                btnDownload.Enabled = false;
                btnDdl.Enabled = false;
                await Task.Run(async () => {
                    allBuckets = await _gcs.ListAllBuckets();
                });
                imageList.Images.Clear();
                imageList.Images.Add(DEFAULT_ICON_BUCKET);
                lvRemote.SmallImageList = imageList;
                lvRemote.LargeImageList = imageList;
                lvRemote.Columns.Clear();
                ColumnHeader[] columnHeader = new ColumnHeader[] {
                    new ColumnHeader { Text = "Nama Bucket", Width = lvRemote.Size.Width - 167 - 10 },
                    new ColumnHeader { Text = "Diperbarui", Width = 167 }
                };
                lvRemote.Columns.AddRange(columnHeader);
                lvRemote.Items.Clear();
                ListViewItem[] lvis = allBuckets.Where(bckt => {
                    return bckt.Name.ToUpper().Contains(txtFilter.Text.ToUpper());
                }).Select(bckt => {
                    ListViewItem lvi = new ListViewItem { Tag = bckt, Text = bckt.Name, ImageIndex = 0 };
                    lvi.SubItems.Add(bckt.Updated.ToString());
                    return lvi;
                }).ToArray();
                lvRemote.Items.AddRange(lvis);
                UpdateLastActivity();
            }
            catch (Exception ex) {
                _logger.WriteError(ex);
                MessageBox.Show(ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally {
                txtDirPath.Text = string.Empty;
                btnRefresh.Enabled = true;
                txtFilter.ReadOnly = false;
            }
        }

        private async Task LoadObjects(string path) {
            try {
                btnHome.Enabled = false;
                btnRefresh.Enabled = false;
                btnUpload.Enabled = false;
                txtFilter.ReadOnly = true;
                btnExportLaporan.Enabled = false;
                btnDownload.Enabled = false;
                btnDdl.Enabled = false;
                await Task.Run(async () => {
                    allObjects = await _gcs.ListAllObjects(path);
                });
                imageList.Images.Clear();
                imageList.Images.Add(DEFAULT_ICON_OBJECT);
                lvRemote.SmallImageList = imageList;
                lvRemote.LargeImageList = imageList;
                lvRemote.Columns.Clear();
                ColumnHeader[] columnHeader = new ColumnHeader[] {
                    new ColumnHeader { Text = "Nama Berkas", Width = lvRemote.Size.Width - (96 + 192 + (2 * 10)) },
                    new ColumnHeader { Text = "Ukuran", Width = 96 },
                    new ColumnHeader { Text = "Tanggal Upload", Width = 192 }
                };
                lvRemote.Columns.AddRange(columnHeader);
                lvRemote.Items.Clear();
                ListViewItem[] lvis = allObjects.Where(obj => {
                    return obj.Name.ToUpper().Contains(txtFilter.Text.ToUpper());
                }).Select(obj => {
                    ListViewItem lvi = new ListViewItem { Tag = obj, Text = obj.Name, ImageIndex = 0 };
                    lvi.SubItems.Add(_converter.FormatByteSizeHumanReadable((long) obj.Size));
                    lvi.SubItems.Add(obj.Updated.ToString());
                    return lvi;
                }).ToArray();
                lvRemote.Items.AddRange(lvis);
                UpdateLastActivity();
            }
            catch (Exception ex) {
                _logger.WriteError(ex);
                MessageBox.Show(ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally {
                txtDirPath.Text = path;
                btnHome.Enabled = true;
                btnRefresh.Enabled = true;
                txtFilter.ReadOnly = false;
                btnUpload.Enabled = true;
                btnExportLaporan.Enabled = true;
            }
        }

        private async void BtnConnect_Click(object sender, EventArgs e) {
            SetIdleBusyStatus(false);
            try {
                string filePath = null;
                using (OpenFileDialog fd = new OpenFileDialog()) {
                    fd.InitialDirectory = _app.AppLocation;
                    fd.RestoreDirectory = true;
                    fd.CheckFileExists = true;
                    fd.Filter = "credentials (*.txt,*.json)|*.txt;*.json";
                    fd.Title = "Open credentials(.txt|.json)";
                    if (fd.ShowDialog() != DialogResult.OK) {
                        throw new Exception("Gagal memuat file credentials.json");
                    }
                    filePath = fd.FileName;
                }
                _gcs.LoadCredential(filePath, filePath.ToLower().EndsWith(".txt"));
                await LoadBuckets();
            }
            catch (Exception ex) {
                _logger.WriteError(ex);
                MessageBox.Show(ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SetIdleBusyStatus(true);
        }

        private void LvRemote_SelectedIndexChanged(object sender, EventArgs e) {
            bool enabled = false;
            if (lvRemote.SelectedItems.Count > 0) {
                dynamic item = lvRemote.SelectedItems[0].Tag;
                if (item is GcsObject || item.Kind == "storage#object") {
                    enabled = true;
                }
            }
            btnDownload.Enabled = enabled;
            btnDdl.Enabled = enabled;
        }

        private async void LvRemote_MouseDoubleClick(object sender, MouseEventArgs e) {
            SetIdleBusyStatus(false);
            dynamic item = lvRemote.SelectedItems[0].Tag;
            if (item is GcsBucket || item.Kind == "storage#bucket") {
                // Name = Id Bucket Sama Aja Kayaknya (?)
                await LoadObjects(item.Name);
            }
            SetIdleBusyStatus(true);
        }

        private async void BtnHome_Click(object sender, EventArgs e) {
            SetIdleBusyStatus(false);
            try {
                await LoadBuckets();
            }
            catch (Exception ex) {
                _logger.WriteError(ex);
                MessageBox.Show(ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SetIdleBusyStatus(true);
        }

        private async void BtnRefresh_Click(object sender, EventArgs e) {
            SetIdleBusyStatus(false);
            string path = txtDirPath.Text;
            if (string.IsNullOrEmpty(path)) {
                await LoadBuckets();
            }
            else {
                await LoadObjects(path);
            }
            SetIdleBusyStatus(true);
        }

        private void TxtFilter_KeyDown(object sender, KeyEventArgs e) {
            switch (e.KeyCode) {
                case Keys.Enter:
                    BtnRefresh_Click(sender, EventArgs.Empty);
                    break;
            }
        }

        private async void BtnExportLaporan_Click(object sender, EventArgs e) {
            SetIdleBusyStatus(false);
            bool btnDownloadEnabledBefore = btnDownload.Enabled;
            bool btnDdlEnabledBefore = btnDdl.Enabled;
            try {
                string path = txtDirPath.Text;
                if (!string.IsNullOrEmpty(path)) {
                    btnHome.Enabled = false;
                    btnRefresh.Enabled = false;
                    txtFilter.ReadOnly = true;
                    btnUpload.Enabled = false;
                    btnExportLaporan.Enabled = false;
                    btnDownload.Enabled = false;
                    btnDdl.Enabled = false;

                    List<GcsObject> objects = null;
                    await Task.Run(async () => {
                        objects = await _gcs.ListAllObjects(path);
                    });

                    string file1name_template = _config.Get<string>("File1Name", _app.GetConfig("file_1_name"));
                    string file2name_template = _config.Get<string>("File2Name", _app.GetConfig("file_2_name"));

                    List<GcsObject> file1 = new List<GcsObject>();
                    List<GcsObject> file2 = new List<GcsObject>();
                    foreach (GcsObject obj in objects) {
                        if (obj.Name.ToLower().StartsWith(file1name_template)) {
                            file1.Add(obj);
                        }
                        if (obj.Name.ToLower().StartsWith(file2name_template)) {
                            file2.Add(obj);
                        }
                    }

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("KODE_DC|TAHUN|BULAN|MINGGU|FILE_1_NAME|FILE_1_SIZE_BYTES|FILE_1_DATE_TIME|FILE_2_NAME|FILE_2_SIZE_BYTES|FILE_2_DATE_TIME");

                    foreach (GcsObject f1 in file1.OrderBy(f => f.Name)) {
                        string fileName = new List<string>(f1.Name.ToLower().Replace("\\", "/").Split('/')).Last();
                        DateTime fileDate = DateTime.ParseExact(fileName.Split('_').Last().Split('.').First().ToLower(), "yyMMdd", CultureInfo.InvariantCulture);
                        string fn1 = file1name_template ?? string.Empty;
                        string newLine = string.Empty;
                        if (fileName.StartsWith(fn1)) {
                            int index = fileName.IndexOf(fn1);
                            string xxx_xxxxxx = (index < 0) ? fileName : fileName.Remove(index, fn1.Length);
                            string dc_kode = $"G{xxx_xxxxxx.Substring(0, 3)}".ToUpper();
                            newLine += $"{dc_kode}|{fileDate.Year}|{fileDate.Month}|{fileDate.GetWeekOfMonth()}|{fileName}|{f1.Size}|{f1.Updated?.ToLocalTime()}|";
                            GcsObject f2 = file2.Find(f => f.Name.EndsWith(xxx_xxxxxx));
                            newLine += (f2 is null) ? "||" : $"{f2.Name}|{f2.Size}|{f2.Updated?.ToLocalTime()}";
                            sb.AppendLine(newLine);
                        }
                    }

                    string exportPath = Path.Combine(_berkas.TempFolderPath, $"{DateTime.Now:yyy-MM-dd_HH-mm-ss}.csv");
                    File.WriteAllText(exportPath, sb.ToString());
                    Process.Start(new ProcessStartInfo {
                        Arguments = _berkas.TempFolderPath,
                        FileName = "explorer.exe"
                    });
                }
            }
            catch (Exception ex) {
                _logger.WriteError(ex);
                MessageBox.Show(ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally {
                btnHome.Enabled = true;
                btnRefresh.Enabled = true;
                txtFilter.ReadOnly = false;
                btnUpload.Enabled = true;
                btnExportLaporan.Enabled = true;
                btnDownload.Enabled = btnDownloadEnabledBefore;
                btnDdl.Enabled = btnDdlEnabledBefore;
            }
            SetIdleBusyStatus(true);
        }

        private async void BtnDdl_Click(object sender, EventArgs e) {
            SetIdleBusyStatus(false);
            try {
                dynamic item = lvRemote.SelectedItems[0].Tag;
                if (item is GcsObject || item.Kind == "storage#object") {
                    string ddl = string.Empty;
                    await Task.Run(async () => {
                        ddl = await _gcs.CreateDownloadUrlSigned(item, dtpExp.Value);
                    });
                    if (string.IsNullOrEmpty(ddl)) {
                        MessageBox.Show("Gagal Membuat URL Unduhan", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else {
                        Clipboard.SetText(ddl);
                        MessageBox.Show(ddl, $"(CopyPaste) Expired :: {dtpExp.Value}", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Process.Start(ddl);
                    }
                }
            }
            catch (Exception ex) {
                _logger.WriteError(ex);
                MessageBox.Show(ex.Message, "Direct Download Link Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SetIdleBusyStatus(true);
        }

        private async void BtnUpload_Click(object sender, EventArgs e) {
            SetIdleBusyStatus(false);
            try {
                string selectedLocalFilePath = null;
                using (OpenFileDialog fd = new OpenFileDialog()) {
                    fd.InitialDirectory = _app.AppLocation;
                    fd.RestoreDirectory = true;
                    fd.CheckFileExists = true;
                    fd.Filter = "MemoryDump files (*.dmp)|*.dmp";
                    fd.Title = "Select idm_metadata_gxxx_xxxxxx.dmp | idm_43table_gxxx_xxxxxx.dmp";
                    if (fd.ShowDialog() != DialogResult.OK) {
                        throw new Exception("Gagal membuka file idm_***_gxxx_xxxxxx.dmp");
                    }
                    selectedLocalFilePath = fd.FileName;
                }

                string dc_kode = selectedLocalFilePath.Replace("\\", "/").Split('/').Last().ToLower();

                DateTime curr = DateTime.Now;
                int year = curr.Year;
                int week = curr.GetWeekOfYear();
                int month = curr.Month;

                string fn1 = _config.Get<string>("File1Name", _app.GetConfig("file_1_name"));
                string fn2 = _config.Get<string>("File2Name", _app.GetConfig("file_2_name"));
                if (dc_kode.StartsWith(fn1)) {
                    int index = dc_kode.IndexOf(fn1);
                    string xxx_xxxxxx = (index < 0) ? dc_kode : dc_kode.Remove(index, fn1.Length);
                    dc_kode = $"G{xxx_xxxxxx.Substring(0, 3)}".ToUpper();
                }
                else if (dc_kode.StartsWith(fn2)) {
                    int index = dc_kode.IndexOf(fn2);
                    string xxx_xxxxxx = (index < 0) ? dc_kode : dc_kode.Remove(index, fn2.Length);
                    dc_kode = $"G{xxx_xxxxxx.Substring(0, 3)}".ToUpper();
                }
                else {
                    throw new Exception("Format nama file salah, IDM_***.dmp");
                }

                string filedate = selectedLocalFilePath.Replace("\\", "/").Split('/').Last().Split('_').Last().Split('.').First().ToLower();
                DateTime fileDate = DateTime.ParseExact(filedate, "yyMMdd", CultureInfo.InvariantCulture);
                if (week != fileDate.GetWeekOfYear() || month != fileDate.Month) {
                    throw new Exception($"File harus di minggu & bulan yang sama dengan tanggal hari ini");
                }

                string file1name = string.Empty;
                DateTime file1date = DateTime.MinValue;
                string file2name = string.Empty;
                DateTime file2date = DateTime.MinValue;

                int rowCount = 0;
                using (DbDataReader reader = await _db.Sqlite_ExecReaderAsync(@"
                        SELECT file_1_name, file_1_date, file_2_name, file_2_date
                        FROM upload_log
                        WHERE year = :year AND week = :week AND month = :month AND dc_kode = :dc_kode
                    ", new List<CDbQueryParamBind> {
                        new CDbQueryParamBind { NAME = "year", VALUE = year },
                        new CDbQueryParamBind { NAME = "week", VALUE = week },
                        new CDbQueryParamBind { NAME = "month", VALUE = month },
                        new CDbQueryParamBind { NAME = "dc_kode", VALUE = dc_kode }
                    })) {
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
                _db.CloseAllConnection();
                if (rowCount == 0) {
                    await _db.SQLite_ExecQuery(@"
                            INSERT INTO upload_log(year, week, month, dc_kode)
                            VALUES(:year, :week, :month, :dc_kode)
                        ", new List<CDbQueryParamBind> {
                            new CDbQueryParamBind { NAME = "year", VALUE = year },
                            new CDbQueryParamBind { NAME = "week", VALUE = week },
                            new CDbQueryParamBind { NAME = "month", VALUE = month },
                            new CDbQueryParamBind { NAME = "dc_kode", VALUE = dc_kode }
                        });
                }

                if (!string.IsNullOrEmpty(file1name) && !string.IsNullOrEmpty(file2name)) {
                    string msg = $"Upload minggu ini sudah selesai{Environment.NewLine}{Environment.NewLine}{file1name}{Environment.NewLine}{file1date}{Environment.NewLine}{Environment.NewLine}{file2name}{Environment.NewLine}{file2date}";
                    MessageBox.Show(msg, $"Upload Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (string.IsNullOrEmpty(file1name) && string.IsNullOrEmpty(file2name)) {
                    await Upload12(selectedLocalFilePath);
                }
                else if (string.IsNullOrEmpty(file1name) && !string.IsNullOrEmpty(file2name)) {
                    await Upload1(selectedLocalFilePath, file2name.ToLower());
                }
                else if (!string.IsNullOrEmpty(file1name) && string.IsNullOrEmpty(file2name)) {
                    await Upload2(selectedLocalFilePath, file1name.ToLower());
                }
                else {
                    MessageBox.Show($"File bermasalah{Environment.NewLine}Silahkan coba lagi dengan file lain", $"File Check Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex) {
                _logger.WriteError(ex);
                MessageBox.Show(ex.Message, "Upload Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SetIdleBusyStatus(true);
        }

        private async Task Upload12(string selectedLocalFilePath) {

            // Check Nama File 1 :: idm_metadata_gxxx_xxxxxx.dmp :: Sesuai Tidak
            string file1name_template = _config.Get<string>("File1Name", _app.GetConfig("file_1_name"));
            string file1ext_template = _config.Get<string>("File1Ext", _app.GetConfig("file_1_ext"));
            string file1 = selectedLocalFilePath.Replace("\\", "/").Split('/').Last().ToLower();
            if (string.IsNullOrEmpty(file1name_template) || string.IsNullOrEmpty(file1ext_template) ||
                !file1.StartsWith(file1name_template) || !file1.EndsWith(file1ext_template)
            ) {
                throw new Exception($"Format nama file salah, {file1name_template}xxx_xxxxxx{file1ext_template}");
            }

            // Ambil Kode Gudang & Tanggal File
            int index = file1.IndexOf(file1name_template);
            string xxx_xxxxxx = (index < 0) ? file1 : file1.Remove(index, file1name_template.Length);

            // Check Nama File 1 :: idm_43table_gxxx_xxxxxx.dmp :: Ada Filenya Gak 1 Folder Pasangan
            string file2name_template = _config.Get<string>("File2Name", _app.GetConfig("file_2_name"));
            string file2ext_template = _config.Get<string>("File2Ext", _app.GetConfig("file_2_ext"));
            string file2 = $"{file2name_template}{xxx_xxxxxx}";
            string file2path = Path.Combine(Path.GetDirectoryName(selectedLocalFilePath), file2).Replace("\\", "/");
            if (!File.Exists(file2path)) {
                throw new Exception($"File tidak ditemukan, {file2name_template}xxx_xxxxxx{file2ext_template}");
            }

            await AddQueue(selectedLocalFilePath.Replace("\\", "/"), file2path.Replace("\\", "/"));
        }

        private async Task Upload1(string selectedLocalFilePath, string file2path) {

            // Ambil Kode Gudang & Tanggal File 2
            string file2name_template = _config.Get<string>("File2Name", _app.GetConfig("file_2_name"));
            int index = file2path.IndexOf(file2name_template);
            string xxx_xxxxxx = (index < 0) ? file2path : file2path.Remove(index, file2name_template.Length);

            // Check Nama File 1 :: idm_metadata_gxxx_xxxxxx.dmp :: Sesuai Tidak
            string file1name_template = _config.Get<string>("File1Name", _app.GetConfig("file_1_name"));
            string file1ext_template = _config.Get<string>("File1Ext", _app.GetConfig("file_1_ext"));
            string file1 = selectedLocalFilePath.Replace("\\", "/").Split('/').Last().ToLower();
            string target = $"{file1name_template}{xxx_xxxxxx}";
            if (file1 != target) {
                throw new Exception($"File pasangan tidak sesuai, {file1name_template}xxx_xxxxxx{file1ext_template}");
            }

            await AddQueue(selectedLocalFilePath.Replace("\\", "/"));
        }

        private async Task Upload2(string selectedLocalFilePath, string file1name) {

            // Ambil Kode Gudang & Tanggal File 1
            string file1name_template = _config.Get<string>("File1Name", _app.GetConfig("file_1_name"));
            int index = file1name.IndexOf(file1name_template);
            string xxx_xxxxxx = (index < 0) ? file1name : file1name.Remove(index, file1name_template.Length);

            // Check Nama File 2 :: idm_43table_gxxx_xxxxxx.dmp :: Sesuai Tidak
            string file2name_template = _config.Get<string>("File2Name", _app.GetConfig("file_2_name"));
            string file2ext_template = _config.Get<string>("File2Ext", _app.GetConfig("file_2_ext"));
            string file2 = selectedLocalFilePath.Replace("\\", "/").Split('/').Last().ToLower();
            string target = $"{file2name_template}{xxx_xxxxxx}";
            if (file2 != target) {
                throw new Exception($"File pasangan tidak sesuai, {file2name_template}xxx_xxxxxx{file2ext_template}");
            }

            await AddQueue(selectedLocalFilePath.Replace("\\", "/"));
        }

        private bool CheckProgressIsRunning(string localPath, string remotePath) {
            foreach (DataGridViewRow row in dgQueue.Rows) {
                if (
                    row.Cells[dgQueue.Columns["dgQueue_FileLocal"].Index].Value.ToString().Equals(localPath) &&
                    row.Cells[dgQueue.Columns["dgQueue_FileRemote"].Index].Value.ToString().Equals(remotePath)
                ) {
                    return true;
                }
            }
            foreach (DataGridViewRow row in dgOnProgress.Rows) {
                if (
                    row.Cells[dgOnProgress.Columns["dgOnProgress_FileLocal"].Index].Value.ToString().Equals(localPath) &&
                    row.Cells[dgOnProgress.Columns["dgOnProgress_FileRemote"].Index].Value.ToString().Equals(remotePath)
                ) {
                    return true;
                }
            }
            return false;
        }

        private async Task AddQueue(string filePath, string fileNextPath = null) {
            string[] arrRemoteDir = txtDirPath.Text.Split('/');
            string targetFolderId = arrRemoteDir[arrRemoteDir.Length - 1];

            FileInfo fileInfo = new FileInfo(filePath);

            if (CheckProgressIsRunning(filePath, $"Google://{targetFolderId}/{fileInfo.Name}")) {
                throw new Exception($"Proses {fileInfo.Name} sedang berjalan");
            }

            string allowedMime = _config.Get<string>("LocalAllowedFileMime", _app.GetConfig("local_allowed_file_mime"));
            string selectedMime = _chiper.GetMimeFromFile(filePath);
            if (string.IsNullOrEmpty(allowedMime) || selectedMime != allowedMime) {
                throw new Exception("Jenis MiMe file salah");
            }

            string signFull = _config.Get<string>("LocalAllowedFileSign", _app.GetConfig("local_allowed_file_sign"));
            if (string.IsNullOrEmpty(signFull)) {
                throw new Exception("Tidak ada tanda tangan file");
            }

            string[] signSplit = signFull.Split(' ');
            int minFileSize = signSplit.Length;
            if (fileInfo.Length < minFileSize) {
                throw new Exception("Isi konten file tidak sesuai");
            }

            int[] intList = new int[minFileSize];
            for (int i = 0; i < intList.Length; i++) {
                if (signSplit[i] == "??") {
                    intList[i] = -1;
                }
                else {
                    intList[i] = int.Parse(signSplit[i], NumberStyles.HexNumber);
                }
            }
            using (BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open))) {
                byte[] buff = new byte[minFileSize];
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                reader.Read(buff, 0, buff.Length);
                for (int i = 0; i < intList.Length; i++) {
                    if (intList[i] == -1 || buff[i] == intList[i]) {
                        continue;
                    }
                    throw new Exception("File rusak / corrupt / Tanda tangan tidak sesuai");
                }
            }

            DialogResult dialogResult = DialogResult.Yes;
            if (!cbReplaceIfExist.Checked) {
                foreach (GcsObject obj in allObjects) {
                    if (obj.Name.ToLower().Contains(fileInfo.Name.ToLower())) {
                        dialogResult = MessageBox.Show($"Tetap lanjut upload '{fileInfo.Name}' ?", "File Already Exist", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        break;
                    }
                }
            }

            if (dialogResult == DialogResult.Yes) {
                int idx = dgQueue.Rows.Add(filePath, "===>>>", $"Google://{targetFolderId}/{fileInfo.Name}");
                DataGridViewRow dgvrQueue = dgQueue.Rows[idx];

                _logger.WriteInfo("Antrian",
                    $"\"{dgvrQueue.Cells[dgQueue.Columns["dgQueue_FileLocal"].Index].Value}\" " +
                    $"{dgvrQueue.Cells[dgQueue.Columns["dgQueue_Direction"].Index].Value} " +
                    $"\"{dgvrQueue.Cells[dgQueue.Columns["dgQueue_FileRemote"].Index].Value}\" "
                );
            }

            if (!string.IsNullOrEmpty(fileNextPath)) {
                await AddQueue(fileNextPath.Replace("\\", "/"));
            }

        }

        private async void TimerQueue_Tick(object sender, EventArgs e) {
            if ((numMaxProcess.Value <= 1 && dgOnProgress.Rows.Count > 0) || dgQueue.Rows.Count <= 0) {
                return;
            }

            DataGridViewRow dgvrQueue = dgQueue.Rows[0];

            string fileLocal = dgvrQueue.Cells[dgQueue.Columns["dgQueue_FileLocal"].Index].Value.ToString();
            string fileUploadDownload = dgvrQueue.Cells[dgQueue.Columns["dgQueue_Direction"].Index].Value.ToString();
            string fileRemote = dgvrQueue.Cells[dgQueue.Columns["dgQueue_FileRemote"].Index].Value.ToString();

            dgQueue.Rows.Remove(dgvrQueue);

            int idx = dgOnProgress.Rows.Add(fileLocal, fileUploadDownload, fileRemote);
            DataGridViewRow dgvrOnProgress = dgOnProgress.Rows[idx];

            if (fileUploadDownload == "===>>>") {

                FileInfo fileInfo = new FileInfo(fileLocal);

                string googleUrl = string.Empty;
                if (fileRemote.StartsWith("Google://")) {
                    googleUrl = fileRemote.Replace("Google://", "");
                }
                string targetFolderId = googleUrl.Split('/').First();

                await Task.Run(async () => {
                    try {
                        string file_md5 = _chiper.CalculateMD5(fileInfo.FullName);
                        CGcsUploadProgress uploaded = null;

                        using (Stream stream = File.OpenRead(fileLocal)) {
                            GcsMediaUpload mediaUpload = _gcs.GenerateUploadMedia(fileInfo, targetFolderId, stream);
                            Uri uploadSession = null;

                            using (DbDataReader reader = await _db.Sqlite_ExecReaderAsync(@"
                                SELECT file_md5, file_session, file_date
                                FROM upload_chunk
                                WHERE file_md5 = :file_md5
                            ", new List<CDbQueryParamBind> {
                                new CDbQueryParamBind { NAME = "file_md5", VALUE = file_md5 }
                            })) {
                                while (reader.Read()) {
                                    if (!reader.IsDBNull(2)) {
                                        DateTime file_date = new DateTime(long.Parse(reader.GetInt64(2).ToString()));
                                        if (DateTime.Now.Ticks <= file_date.AddDays(5).Ticks) {
                                            if (!reader.IsDBNull(2)) {
                                                uploadSession = new Uri(reader.GetString(1));
                                            }
                                        }
                                    }
                                }
                                reader.Close();
                            }
                            _db.CloseAllConnection();

                            if (uploadSession == null) {
                                await _db.SQLite_ExecQuery(@"
                                    DELETE FROM upload_chunk
                                    WHERE file_md5 = :file_md5
                                ", new List<CDbQueryParamBind> {
                                    new CDbQueryParamBind { NAME = "file_md5", VALUE = file_md5 }
                                });
                                uploadSession = await _gcs.CreateUploadUri(mediaUpload);
                                await _db.SQLite_ExecQuery(@"
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
                            uploaded = await _gcs.UploadFile(mediaUpload, uploadSession, (progressNew) => {
                                onGoingUploadProgress.Report(new {
                                    dgvr = dgvrOnProgress,
                                    fi = fileInfo,
                                    pOld = progressOld,
                                    pNew = progressNew,
                                    dt = dateTime
                                });
                                progressOld = progressNew;
                                dateTime = DateTime.Now;
                            });
                        }

                        if (uploaded.Exception == null && uploaded.Status == EGcsUploadStatus.Completed) {
                            try {
                                string sql = @"UPDATE upload_log SET";
                                List<CDbQueryParamBind> param = new List<CDbQueryParamBind>();

                                string file1name_template = _config.Get<string>("File1Name", _app.GetConfig("file_1_name"));
                                if (fileInfo.Name.ToLower().Contains(file1name_template)) {
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
                                DateTime fileDate = DateTime.ParseExact(filedate, "yyMMdd", CultureInfo.InvariantCulture);
                                int year = fileDate.Year;
                                param.Add(new CDbQueryParamBind { NAME = "year", VALUE = year });
                                int week = fileDate.GetWeekOfYear();
                                param.Add(new CDbQueryParamBind { NAME = "week", VALUE = week });
                                int month = fileDate.Month;
                                param.Add(new CDbQueryParamBind { NAME = "month", VALUE = month });

                                string dc_kode = fileInfo.Name.ToLower();
                                string fn1 = _config.Get<string>("File1Name", _app.GetConfig("file_1_name"));
                                string fn2 = _config.Get<string>("File2Name", _app.GetConfig("file_2_name"));
                                if (dc_kode.StartsWith(fn1)) {
                                    int index = dc_kode.IndexOf(fn1);
                                    string xxx_xxxxxx = (index < 0) ? dc_kode : dc_kode.Remove(index, fn1.Length);
                                    dc_kode = $"G{xxx_xxxxxx.Substring(0, 3)}".ToUpper();
                                }
                                else if (dc_kode.StartsWith(fn2)) {
                                    int index = dc_kode.IndexOf(fn2);
                                    string xxx_xxxxxx = (index < 0) ? dc_kode : dc_kode.Remove(index, fn2.Length);
                                    dc_kode = $"G{xxx_xxxxxx.Substring(0, 3)}".ToUpper();
                                }
                                else {
                                    dc_kode = null;
                                }
                                param.Add(new CDbQueryParamBind { NAME = "dc_kode", VALUE = dc_kode });
                                await _db.SQLite_ExecQuery(sql, param);

                                await _db.SQLite_ExecQuery(@"
                                        DELETE FROM upload_chunk
                                        WHERE file_md5 = :file_md5
                                    ", new List<CDbQueryParamBind> {
                                        new CDbQueryParamBind { NAME = "file_md5", VALUE = file_md5 }
                                    });
                            }
                            catch (Exception ex) {
                                _logger.WriteError(ex);
                                MessageBox.Show(ex.Message, "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }

                            try {
                                DialogResult dialogResult =
                                    cbDeleteOnComplete.Checked ?
                                        DialogResult.Yes :
                                            MessageBox.Show(
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
                                _logger.WriteError(ex);
                                MessageBox.Show(ex.Message, "Delete Local Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (TaskCanceledException ex) {
                        _logger.WriteError(ex);
                        MessageBox.Show("Koneksi terputus", "Network Timeout", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex) {
                        _logger.WriteError(ex);
                        MessageBox.Show(ex.Message, "Upload Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                });

            }

            else if (fileUploadDownload == "<<<===") {

                // TODO ::

            }
        }

        private async void BtnDownload_Click(object sender, EventArgs e) {
            SetIdleBusyStatus(false);
            try {
                dynamic item = lvRemote.SelectedItems[0].Tag;
                if (item is GcsObject || item.Kind == "storage#object") {
                    string selectedLocalFilePath = null;
                    using (SaveFileDialog fd = new SaveFileDialog()) {
                        fd.InitialDirectory = _app.AppLocation;
                        fd.RestoreDirectory = true;
                        fd.CheckPathExists = true;
                        fd.FileName = item.Name;
                        fd.Filter = "MemoryDump files (*.dmp)|*.dmp";
                        fd.Title = $"Save {item.Name}";
                        fd.DefaultExt = _config.Get<string>("LocalAllowedFileExt", _app.GetConfig("local_allowed_file_ext"));
                        if (fd.ShowDialog() != DialogResult.OK) {
                            throw new Exception("Gagal menentukan lokasi penyimpanan file idm_***_gxxx_xxxxxx.dmp");
                        }
                        selectedLocalFilePath = fd.FileName;
                    }

                    string[] arrRemoteDir = txtDirPath.Text.Split('/');
                    string folderId = arrRemoteDir[arrRemoteDir.Length - 1];
                    string targetPathLocal = selectedLocalFilePath.Replace("\\", "/");

                    if (CheckProgressIsRunning(targetPathLocal, $"Google://{folderId}/{item.Name}")) {
                        throw new Exception($"Proses {item.Name} sedang berjalan");
                    }

                    int idx = dgOnProgress.Rows.Add(targetPathLocal, "<<<===", $"Google://{folderId}/{item.Name}");
                    DataGridViewRow dataGridViewRow = dgOnProgress.Rows[idx];

                    await Task.Run(async () => {
                        try {
                            CGcsDownloadProgress progressOld = null;
                            DateTime dateTime = DateTime.Now;
                            await _gcs.DownloadFile((GcsObject) item, selectedLocalFilePath, (progressNew) => {
                                onGoingDownloadProgress.Report(new {
                                    dgvr = dataGridViewRow,
                                    sz = item.Size,
                                    pOld = progressOld,
                                    pNew = progressNew,
                                    dt = dateTime
                                });
                                progressOld = progressNew;
                                dateTime = DateTime.Now;
                            });

                            Process.Start(new ProcessStartInfo {
                                Arguments = Path.GetDirectoryName(selectedLocalFilePath),
                                FileName = "explorer.exe"
                            });
                        }
                        catch (TaskCanceledException ex) {
                            _logger.WriteError(ex);
                            MessageBox.Show("Koneksi terputus", "Network Timeout", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        catch (Exception ex) {
                            _logger.WriteError(ex);
                            MessageBox.Show(ex.Message, "Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    });
                }
            }
            catch (Exception ex) {
                _logger.WriteError(ex);
                MessageBox.Show(ex.Message, "Save File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SetIdleBusyStatus(true);
        }
    }

}
