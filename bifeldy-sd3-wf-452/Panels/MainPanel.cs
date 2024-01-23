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

        public CMainPanel(IApp app, ILogger logger, IDb db, IConfig config, IWinReg winreg, IConverter converter, IGoogleCloudStorage gcs, IBerkas berkas) {
            _app = app;
            _logger = logger;
            _db = db;
            _config = config;
            _winreg = winreg;
            _converter = converter;
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
                List<GcsBucket> buckets = null;
                await Task.Run(async () => {
                    buckets = await _gcs.ListAllBuckets();
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
                ListViewItem[] lvis = buckets.Where(b => {
                    dynamic bckt = b;
                    return bckt.Name.ToUpper().Contains(txtFilter.Text.ToUpper());
                }).Select(b => {
                    dynamic bckt = b;
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
                List<GcsObject> objects = null;
                await Task.Run(async () => {
                    objects = await _gcs.ListAllObjects(path);
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
                ListViewItem[] lvis = objects.Where(o => {
                    dynamic obj = o;
                    return obj.Name.ToUpper().Contains(txtFilter.Text.ToUpper());
                }).Select(o => {
                    dynamic obj = o;
                    ListViewItem lvi = new ListViewItem { Tag = obj, Text = obj.Name, ImageIndex = 0 };
                    lvi.SubItems.Add(_converter.FormatByteSizeHumanReadable((long)obj.Size));
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
                btnConnect.Enabled = false;
                string filePath = null;
                using (OpenFileDialog upload = new OpenFileDialog()) {
                    upload.InitialDirectory = _app.AppLocation;
                    upload.Filter = "Json files (*.json)|*.json";
                    upload.Title = "Open credentials.json";
                    if (upload.ShowDialog() != DialogResult.OK) {
                        throw new Exception("Gagal memuat file credentials.json");
                    }
                    filePath = upload.FileName;
                }
                _gcs.LoadCredential(filePath);
                await LoadBuckets();
            }
            catch (Exception ex) {
                _logger.WriteError(ex);
                MessageBox.Show(ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnConnect.Enabled = true;
            }
            SetIdleBusyStatus(true);
        }

        private void lvRemote_SelectedIndexChanged(object sender, EventArgs e) {
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

                    List<dynamic> file1 = new List<dynamic>();
                    List<dynamic> file2 = new List<dynamic>();
                    foreach (GcsObject o in objects) {
                        dynamic obj = o;
                        if (obj.Name.ToLower().StartsWith(file1name_template)) {
                            file1.Add(obj);
                        }
                        if (obj.Name.ToLower().StartsWith(file2name_template)) {
                            file2.Add(obj);
                        }
                    }

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("KODE_DC|TAHUN|BULAN|MINGGU|FILE_1_NAME|FILE_1_SIZE_BYTES|FILE_1_DATE_TIME|FILE_2_NAME|FILE_2_SIZE_BYTES|FILE_2_DATE_TIME");

                    foreach (dynamic f1 in file1.OrderBy(f => f.Name)) {
                        string fileName = new List<string>(f1.Name.ToLower().Replace("\\", "/").Split('/')).Last();
                        DateTime fileDate = DateTime.ParseExact(fileName.Split('_').Last().Split('.').First().ToLower(), "yyMMdd", CultureInfo.InvariantCulture);
                        string fn1 = file1name_template ?? string.Empty;
                        string newLine = string.Empty;
                        if (fileName.StartsWith(fn1)) {
                            int index = fileName.IndexOf(fn1);
                            string xxx_xxxxxx = (index < 0) ? fileName : fileName.Remove(index, fn1.Length);
                            string dc_kode = $"G{xxx_xxxxxx.Substring(0, 3)}".ToUpper();
                            newLine += $"{dc_kode}|{fileDate.Year}|{fileDate.Month}|{fileDate.GetWeekOfMonth()}|{fileName}|{f1.Size}|{f1.Updated.ToLocalTime()}|";
                            dynamic f2 = file2.Find(f => f.Name.EndsWith(xxx_xxxxxx));
                            newLine += (f2 == null) ? "||" : $"{f2.Name}|{f2.Size}|{f2.Updated.ToLocalTime()}";
                            sb.AppendLine(newLine);
                        }
                    }

                    string exportPath = Path.Combine(_berkas.TempFolderPath, $"{DateTime.Now:yyy-MM-dd_HH-mm-ss}.csv");
                    File.WriteAllText(exportPath, sb.ToString());
                    Process.Start(new ProcessStartInfo { Arguments = _berkas.TempFolderPath, FileName = "explorer.exe" });
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
                        MessageBox.Show(ddl, $"Expired :: {dtpExp.Value}", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Process.Start(ddl);
                    }
                }
            }
            catch (Exception ex) {
                _logger.WriteError(ex);
                MessageBox.Show(ex.Message, "Number Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SetIdleBusyStatus(true);
        }

    }

}
