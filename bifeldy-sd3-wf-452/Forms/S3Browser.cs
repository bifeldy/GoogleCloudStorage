using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Libraries;
using bifeldy_sd3_lib_452.Models;
using bifeldy_sd3_lib_452.Utilities;

using GoogleCloudStorage.Utilities;

namespace GoogleCloudStorage.Forms {

    public sealed partial class CS3Browser : Form {

        private readonly IApp _app;
        private readonly ILogger _logger;
        private readonly IConfig _config;
        private readonly IAmazonS3 _s3;
        private readonly IConverter _converter;

        private readonly Icon DEFAULT_ICON_FOLDER = DefaultIcons.Extract("shell32.dll", 3, true);
        private readonly Icon DEFAULT_ICON_BUCKET = DefaultIcons.Extract("shell32.dll", 9, true);
        private readonly Icon DEFAULT_ICON_OBJECT = DefaultIcons.Extract("shell32.dll", 69, true);

        private List<AwsS3Bucket> allBuckets = null;
        private List<AwsS3Prefix> allPrefixes = null;
        private List<AwsS3Object> allObjects = null;

        private string currentBucket = string.Empty;

        public string SelectedBucket { get; private set; }
        public string SelectedObjectKey { get; private set; }
        public long SelectedObjectSize { get; private set; }
        public bool SelectedUploadByStreamPipe { get; private set; }

        private readonly List<string> ctlExclBusy = new List<string>() {
            "btnHome",
            "btnRefresh",
            "btnSelectFile",
            "txtDirPath",
            "txtFilter"
        };

        private CancellationTokenSource debounceToken = null;

        public CS3Browser(
            IApp app,
            ILogger logger,
            IConfig config,
            IAmazonS3 s3,
            IConverter converter
        ) {
            this._app = app;
            this._logger = logger;
            this._config = config;
            this._s3 = s3;
            this._converter = converter;

            this.InitializeComponent();
            this.imageList.ColorDepth = ColorDepth.Depth32Bit;
        }

        public void SetIdleBusyStatus(bool isIdle) {
            this._app.IsIdle = isIdle;
            this.prgrssBrStatus.Style = isIdle ? ProgressBarStyle.Continuous : ProgressBarStyle.Marquee;
            this.EnableDisableControl(this.Controls, isIdle);
        }

        private void EnableDisableControl(Control.ControlCollection controls, bool isIdle) {
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

        private async void S3Browser_Load(object sender, EventArgs e) {
            await this.LoadBuckets();
        }

        private async void BtnHome_Click(object sender, EventArgs e) {
            this.currentBucket = string.Empty;
            this.txtDirPath.Text = string.Empty;
            await this.LoadBuckets();
        }

        private async void BtnRefresh_Click(object sender, EventArgs e) {
            if (string.IsNullOrEmpty(this.currentBucket)) {
                await this.LoadBuckets();
            }
            else {
                string prefix = this.txtDirPath.Text.Length > this.currentBucket.Length
                    ? this.txtDirPath.Text.Substring(this.currentBucket.Length + 1)
                    : string.Empty;

                _ = await this.LoadObjects(this.currentBucket, prefix);
            }
        }

        private void TxtFilter_KeyDown(object sender, KeyEventArgs e) {
            switch (e.KeyCode) {
                case Keys.Enter:
                    this.BtnRefresh_Click(sender, EventArgs.Empty);
                    break;
            }
        }

        private async void TxtFilter_TextChanged(object sender, EventArgs e) {
            if (this.debounceToken != null) {
                this.debounceToken.Cancel();
                this.debounceToken.Dispose();
            }

            this.debounceToken = new CancellationTokenSource();
            CancellationToken token = this.debounceToken.Token;

            try {
                await Task.Delay(400, token);
                if (token.IsCancellationRequested) {
                    return;
                }

                string keyword = this.txtFilter.Text.ToUpper();

                this.lvRemote.BeginUpdate();

                if (string.IsNullOrEmpty(this.currentBucket)) {
                    if (this.allBuckets == null) {
                        return;
                    }

                    AwsS3Bucket[] filtered = this.allBuckets.Where(b => b.BucketName.ToUpper().Contains(keyword)).ToArray();
                    this.lvRemote.Items.Clear();

                    foreach (AwsS3Bucket bckt in filtered) {
                        var lvi = new ListViewItem() {
                            Tag = bckt,
                            Text = bckt.BucketName,
                            ImageIndex = 0
                        };

                        _ = lvi.SubItems.Add(bckt.CreationDate.ToString());
                        _ = this.lvRemote.Items.Add(lvi);
                    }
                }
                else {
                    if (this.allObjects == null || this.allPrefixes == null) {
                        return;
                    }

                    this.lvRemote.Items.Clear();
                    var itemsToAdd = new List<ListViewItem>();

                    string currentPrefix = this.txtDirPath.Text.Length > this.currentBucket.Length
                        ? this.txtDirPath.Text.Substring(this.currentBucket.Length + 1)
                        : string.Empty;

                    if (!string.IsNullOrEmpty(currentPrefix)) {
                        string parentPrefix = string.Empty;
                        string tempPrefix = currentPrefix.TrimEnd('/');
                        int lastSlashIdx = tempPrefix.LastIndexOf('/');

                        if (lastSlashIdx >= 0) {
                            parentPrefix = tempPrefix.Substring(0, lastSlashIdx + 1);
                        }

                        var upPrefix = new AwsS3Prefix() {
                            BucketName = this.currentBucket,
                            Prefix = parentPrefix
                        };

                        var lviUp = new ListViewItem() {
                            Tag = upPrefix,
                            Text = "../ (Naik 1 Folder)",
                            ImageIndex = 0
                        };

                        _ = lviUp.SubItems.Add(string.Empty);
                        _ = lviUp.SubItems.Add(string.Empty);

                        itemsToAdd.Add(lviUp);
                    }

                    AwsS3Prefix[] filteredPrefixes = this.allPrefixes.Where(pre => {
                        string folderName = currentPrefix != string.Empty ? pre.Prefix.Substring(currentPrefix.Length) : pre.Prefix;
                        return folderName.ToUpper().Contains(keyword);
                    }).ToArray();

                    foreach (AwsS3Prefix pre in filteredPrefixes) {
                        string folderName = currentPrefix != string.Empty ? pre.Prefix.Substring(currentPrefix.Length) : pre.Prefix;
                        var lvi = new ListViewItem() {
                            Tag = pre,
                            Text = folderName,
                            ImageIndex = 0
                        };

                        _ = lvi.SubItems.Add(string.Empty);
                        _ = lvi.SubItems.Add(string.Empty);

                        itemsToAdd.Add(lvi);
                    }

                    string fp = this._config.Get<string>("SigNamePattern", this._app.GetConfig("sig_name_pattern"));
                    string _fp = fp.Replace(".sig", string.Empty);

                    AwsS3Object[] filteredObjects = this.allObjects.Where(obj => {
                        string fileName = currentPrefix != string.Empty ? obj.Key.Substring(currentPrefix.Length) : obj.Key;

                        return fileName.ToUpper().Contains(keyword) &&
                            Regex.IsMatch(obj.Key, _fp, RegexOptions.IgnoreCase);
                    }).OrderByDescending(obj => obj.LastModified).ToArray();

                    foreach (AwsS3Object obj in filteredObjects) {
                        string fileName = currentPrefix != string.Empty ? obj.Key.Substring(currentPrefix.Length) : obj.Key;

                        var lvi = new ListViewItem() {
                            Tag = obj,
                            Text = fileName,
                            ImageIndex = 1
                        };

                        _ = lvi.SubItems.Add(this._converter.FormatByteSizeHumanReadable(obj.Size));
                        _ = lvi.SubItems.Add(obj.LastModified.ToString());

                        itemsToAdd.Add(lvi);
                    }

                    this.lvRemote.Items.AddRange(itemsToAdd.ToArray());
                }
            }
            catch (TaskCanceledException) {
                //
            }
            finally {
                this.lvRemote.EndUpdate();
            }
        }

        private async Task LoadBuckets() {
            this.SetIdleBusyStatus(false);

            try {
                this.btnHome.Enabled = false;
                this.btnRefresh.Enabled = false;
                this.txtFilter.ReadOnly = true;

                await Task.Run(async () => {
                    this.allBuckets = await this._s3.ListBucketsAsync();
                });

                this.imageList.Images.Clear();
                this.imageList.Images.Add(this.DEFAULT_ICON_BUCKET);

                this.lvRemote.SmallImageList = this.imageList;
                this.lvRemote.LargeImageList = this.imageList;

                var columnHeader = new ColumnHeader[] {
                    new ColumnHeader() {
                        Text = "Nama Bucket",
                        Width = this.lvRemote.Size.Width - 167 - 10
                    },
                    new ColumnHeader() {
                        Text = "Dibuat",
                        Width = 167
                    }
                };

                this.lvRemote.Columns.Clear();
                this.lvRemote.Columns.AddRange(columnHeader);

                ListViewItem[] lvis = this.allBuckets.Where(bckt => {
                    return bckt.BucketName.ToUpper().Contains(this.txtFilter.Text.ToUpper());
                }).OrderBy(bckt => bckt.BucketName).Select(bckt => {
                    var lvi = new ListViewItem() {
                        Tag = bckt,
                        Text = bckt.BucketName,
                        ImageIndex = 0
                    };

                    _ = lvi.SubItems.Add(bckt.CreationDate.ToString());

                    return lvi;
                }).ToArray();

                this.lvRemote.Items.Clear();
                this.lvRemote.Items.AddRange(lvis);
            }
            catch (TaskCanceledException ex) {
                this._logger.WriteError(ex);
                _ = MessageBox.Show("Koneksi terputus", "Network Timeout", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private async Task<bool> LoadObjects(string bucketName, string prefix = "") {
            bool result = false;

            this.SetIdleBusyStatus(false);

            try {
                this.btnHome.Enabled = false;
                this.btnRefresh.Enabled = false;
                this.txtFilter.ReadOnly = true;

                this.currentBucket = bucketName;

                await Task.Run(async () => {
                    (List<AwsS3Prefix>, List<AwsS3Object>) data = await this._s3.ListObjectsAsync(bucketName, prefix);
                    this.allPrefixes = data.Item1;
                    this.allObjects = data.Item2;
                });

                this.imageList.Images.Clear();
                this.imageList.Images.Add(this.DEFAULT_ICON_FOLDER);
                this.imageList.Images.Add(this.DEFAULT_ICON_OBJECT);

                this.lvRemote.SmallImageList = this.imageList;
                this.lvRemote.LargeImageList = this.imageList;

                var columnHeader = new ColumnHeader[] {
                    new ColumnHeader() {
                        Text = "Nama Berkas",
                        Width = this.lvRemote.Size.Width - (96 + 192 + (2 * 10))
                    },
                    new ColumnHeader() {
                        Text = "Ukuran",
                        Width = 96
                    },
                    new ColumnHeader() {
                        Text = "Tanggal Selesai Upload",
                        Width = 192
                    }
                };

                this.lvRemote.Columns.Clear();
                this.lvRemote.Columns.AddRange(columnHeader);

                this.lvRemote.BeginUpdate();
                this.lvRemote.Items.Clear();

                var itemsToAdd = new List<ListViewItem>();

                if (!string.IsNullOrEmpty(prefix)) {
                    string parentPrefix = string.Empty;
                    string tempPrefix = prefix.TrimEnd('/');
                    int lastSlashIdx = tempPrefix.LastIndexOf('/');

                    if (lastSlashIdx >= 0) {
                        parentPrefix = tempPrefix.Substring(0, lastSlashIdx + 1);
                    }

                    var upPrefix = new AwsS3Prefix() {
                        BucketName = bucketName,
                        Prefix = parentPrefix
                    };

                    var lviUp = new ListViewItem() {
                        Tag = upPrefix,
                        Text = "../ (Naik 1 Folder)",
                        ImageIndex = 0
                    };

                    _ = lviUp.SubItems.Add(string.Empty);
                    _ = lviUp.SubItems.Add(string.Empty);

                    itemsToAdd.Add(lviUp);
                }

                foreach (AwsS3Prefix pre in this.allPrefixes) {
                    string folderName = pre.Prefix;
                    if (prefix != string.Empty) {
                        folderName = pre.Prefix.Substring(prefix.Length);
                    }

                    var lvi = new ListViewItem() {
                        Tag = pre,
                        Text = folderName,
                        ImageIndex = 0
                    };

                    _ = lvi.SubItems.Add(string.Empty);
                    _ = lvi.SubItems.Add(string.Empty);

                    itemsToAdd.Add(lvi);
                }

                string fp = this._config.Get<string>("SigNamePattern", this._app.GetConfig("sig_name_pattern"));
                string _fp = fp.Replace(".sig", string.Empty);

                IOrderedEnumerable<AwsS3Object> filteredObjects = this.allObjects.Where(obj => {
                    return obj.Key.ToUpper().Contains(this.txtFilter.Text.ToUpper()) &&
                           Regex.IsMatch(obj.Key, _fp, RegexOptions.IgnoreCase);
                }).OrderByDescending(obj => obj.LastModified);

                foreach (AwsS3Object obj in filteredObjects) {
                    string fileName = obj.Key;
                    if (prefix != string.Empty) {
                        fileName = obj.Key.Substring(prefix.Length);
                    }

                    var lvi = new ListViewItem() {
                        Tag = obj,
                        Text = fileName,
                        ImageIndex = 1
                    };

                    _ = lvi.SubItems.Add(this._converter.FormatByteSizeHumanReadable(obj.Size));
                    _ = lvi.SubItems.Add(obj.LastModified.ToString());

                    itemsToAdd.Add(lvi);
                }

                this.lvRemote.Items.AddRange(itemsToAdd.ToArray());
                this.lvRemote.EndUpdate();

                result = true;
            }
            catch (TaskCanceledException ex) {
                this._logger.WriteError(ex);
                _ = MessageBox.Show("Koneksi terputus", "Network Timeout", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex) {
                this._logger.WriteError(ex);
                _ = MessageBox.Show(ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally {
                this.txtDirPath.Text = string.IsNullOrEmpty(prefix) ? bucketName : $"{bucketName}/{prefix}";
                this.btnHome.Enabled = true;
                this.btnRefresh.Enabled = true;
                this.txtFilter.ReadOnly = false;
            }

            this.SetIdleBusyStatus(true);

            return result;
        }

        private async void LvRemote_DoubleClick(object sender, EventArgs e) {
            if (this.lvRemote.SelectedItems.Count > 0) {
                ListViewItem selectedItem = this.lvRemote.SelectedItems[0];

                if (this._app.IsIdle) {
                    if (selectedItem.Tag is AwsS3Bucket bucket) {
                        this.txtFilter.Text = string.Empty;
                        _ = await this.LoadObjects(bucket.BucketName, string.Empty);
                    }
                    else if (selectedItem.Tag is AwsS3Prefix folderPrefix) {
                        this.txtFilter.Text = string.Empty;
                        _ = await this.LoadObjects(folderPrefix.BucketName, folderPrefix.Prefix);
                    }
                }
            }
        }

        private void LvRemote_SelectedIndexChanged(object sender, EventArgs e) {
            if (this.lvRemote.SelectedItems.Count > 0) {
                ListViewItem selectedItem = this.lvRemote.SelectedItems[0];
                this.btnSelectFile.Enabled = selectedItem.Tag is AwsS3Object;
            }
            else {
                this.btnSelectFile.Enabled = false;
            }
        }

        private void BtnSelectFile_Click(object sender, EventArgs e) {
            if (this.lvRemote.SelectedItems.Count > 0) {
                ListViewItem selectedItem = this.lvRemote.SelectedItems[0];

                if (selectedItem.Tag is AwsS3Object s3Obj) {
                    this.SelectedBucket = this.currentBucket;

                    this.SelectedObjectKey = s3Obj.Key;
                    this.SelectedObjectSize = s3Obj.Size;
                    this.SelectedUploadByStreamPipe = this.cbKoneksiLokal.Checked;

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }

    }

}
