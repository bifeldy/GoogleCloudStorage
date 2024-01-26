
namespace GoogleCloudStorage.Panels {

    public sealed partial class CMainPanel {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CMainPanel));
            this.chkWindowsStartup = new System.Windows.Forms.CheckBox();
            this.userInfo = new System.Windows.Forms.Label();
            this.imgDomar = new System.Windows.Forms.PictureBox();
            this.prgrssBrStatus = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.appInfo = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.txtDirPath = new System.Windows.Forms.TextBox();
            this.btnHome = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.txtFilter = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblRefresh = new System.Windows.Forms.Label();
            this.lblTime = new System.Windows.Forms.Label();
            this.lblDate = new System.Windows.Forms.Label();
            this.lvRemote = new System.Windows.Forms.ListView();
            this.btnUpload = new System.Windows.Forms.Button();
            this.btnDownload = new System.Windows.Forms.Button();
            this.btnDdl = new System.Windows.Forms.Button();
            this.tabUpDownProgress = new System.Windows.Forms.TabControl();
            this.tabQueue = new System.Windows.Forms.TabPage();
            this.dgQueue = new System.Windows.Forms.DataGridView();
            this.tabOnProgress = new System.Windows.Forms.TabPage();
            this.dgOnProgress = new System.Windows.Forms.DataGridView();
            this.tabErrorFail = new System.Windows.Forms.TabPage();
            this.dgErrorFail = new System.Windows.Forms.DataGridView();
            this.tabSuccess = new System.Windows.Forms.TabPage();
            this.dgSuccess = new System.Windows.Forms.DataGridView();
            this.btnExportLaporan = new System.Windows.Forms.Button();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.cbDeleteOnComplete = new System.Windows.Forms.CheckBox();
            this.cbReplaceIfExist = new System.Windows.Forms.CheckBox();
            this.Expired = new System.Windows.Forms.Label();
            this.dtpExp = new System.Windows.Forms.DateTimePicker();
            this.timerQueue = new System.Windows.Forms.Timer(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.numMaxProcess = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.imgDomar)).BeginInit();
            this.tabUpDownProgress.SuspendLayout();
            this.tabQueue.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgQueue)).BeginInit();
            this.tabOnProgress.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgOnProgress)).BeginInit();
            this.tabErrorFail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgErrorFail)).BeginInit();
            this.tabSuccess.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgSuccess)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxProcess)).BeginInit();
            this.SuspendLayout();
            // 
            // chkWindowsStartup
            // 
            this.chkWindowsStartup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkWindowsStartup.AutoSize = true;
            this.chkWindowsStartup.Location = new System.Drawing.Point(576, 67);
            this.chkWindowsStartup.Name = "chkWindowsStartup";
            this.chkWindowsStartup.Size = new System.Drawing.Size(143, 17);
            this.chkWindowsStartup.TabIndex = 0;
            this.chkWindowsStartup.Text = "Run After Windows Start";
            this.chkWindowsStartup.UseVisualStyleBackColor = true;
            this.chkWindowsStartup.CheckedChanged += new System.EventHandler(this.ChkWindowsStartup_CheckedChanged);
            // 
            // userInfo
            // 
            this.userInfo.AutoSize = true;
            this.userInfo.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.userInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(157)))), ((int)(((byte)(88)))));
            this.userInfo.Location = new System.Drawing.Point(51, 62);
            this.userInfo.Name = "userInfo";
            this.userInfo.Size = new System.Drawing.Size(343, 21);
            this.userInfo.TabIndex = 1;
            this.userInfo.Text = ".: {{ KodeDc }} - {{ NamaDc }} :: {{ UserName }} :.";
            this.userInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // imgDomar
            // 
            this.imgDomar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.imgDomar.Image = ((System.Drawing.Image)(resources.GetObject("imgDomar.Image")));
            this.imgDomar.Location = new System.Drawing.Point(724, 27);
            this.imgDomar.Name = "imgDomar";
            this.imgDomar.Size = new System.Drawing.Size(51, 56);
            this.imgDomar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imgDomar.TabIndex = 2;
            this.imgDomar.TabStop = false;
            this.imgDomar.Click += new System.EventHandler(this.ImgDomar_Click);
            // 
            // prgrssBrStatus
            // 
            this.prgrssBrStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.prgrssBrStatus.ForeColor = System.Drawing.Color.GreenYellow;
            this.prgrssBrStatus.Location = new System.Drawing.Point(575, 52);
            this.prgrssBrStatus.MarqueeAnimationSpeed = 25;
            this.prgrssBrStatus.Name = "prgrssBrStatus";
            this.prgrssBrStatus.Size = new System.Drawing.Size(139, 10);
            this.prgrssBrStatus.Step = 1;
            this.prgrssBrStatus.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.prgrssBrStatus.TabIndex = 3;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(64)))), ((int)(((byte)(129)))));
            this.lblStatus.Location = new System.Drawing.Point(572, 30);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(142, 23);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "Program {{ Idle }} ...";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // appInfo
            // 
            this.appInfo.AutoSize = true;
            this.appInfo.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.appInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(133)))), ((int)(((byte)(244)))));
            this.appInfo.Location = new System.Drawing.Point(26, 27);
            this.appInfo.Name = "appInfo";
            this.appInfo.Size = new System.Drawing.Size(342, 30);
            this.appInfo.TabIndex = 5;
            this.appInfo.Text = "{{ BoilerPlate-Net452-WinForm }}";
            this.appInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.Location = new System.Drawing.Point(229, 112);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(551, 100);
            this.txtLog.TabIndex = 15;
            // 
            // btnConnect
            // 
            this.btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConnect.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnConnect.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnConnect.Location = new System.Drawing.Point(20, 170);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(190, 30);
            this.btnConnect.TabIndex = 18;
            this.btnConnect.Text = "Re/Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.BtnConnect_Click);
            // 
            // txtDirPath
            // 
            this.txtDirPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDirPath.Location = new System.Drawing.Point(311, 220);
            this.txtDirPath.Name = "txtDirPath";
            this.txtDirPath.ReadOnly = true;
            this.txtDirPath.Size = new System.Drawing.Size(388, 20);
            this.txtDirPath.TabIndex = 19;
            // 
            // btnHome
            // 
            this.btnHome.Enabled = false;
            this.btnHome.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHome.Location = new System.Drawing.Point(229, 218);
            this.btnHome.Name = "btnHome";
            this.btnHome.Size = new System.Drawing.Size(76, 24);
            this.btnHome.TabIndex = 20;
            this.btnHome.Text = "Home ~/";
            this.btnHome.UseVisualStyleBackColor = true;
            this.btnHome.Click += new System.EventHandler(this.BtnHome_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.Enabled = false;
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Location = new System.Drawing.Point(705, 218);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 24);
            this.btnRefresh.TabIndex = 21;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            // 
            // txtFilter
            // 
            this.txtFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFilter.Location = new System.Drawing.Point(321, 248);
            this.txtFilter.Name = "txtFilter";
            this.txtFilter.Size = new System.Drawing.Size(459, 20);
            this.txtFilter.TabIndex = 22;
            this.txtFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtFilter_KeyDown);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(226, 251);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 23;
            this.label1.Text = "Filter Pencarian ::";
            // 
            // lblRefresh
            // 
            this.lblRefresh.AutoSize = true;
            this.lblRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRefresh.Location = new System.Drawing.Point(17, 116);
            this.lblRefresh.Name = "lblRefresh";
            this.lblRefresh.Size = new System.Drawing.Size(73, 13);
            this.lblRefresh.TabIndex = 26;
            this.lblRefresh.Text = "Terakhir Sync";
            this.lblRefresh.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTime.Location = new System.Drawing.Point(131, 136);
            this.lblTime.Name = "lblTime";
            this.lblTime.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblTime.Size = new System.Drawing.Size(79, 20);
            this.lblTime.TabIndex = 25;
            this.lblTime.Text = "88:88:88";
            this.lblTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblDate
            // 
            this.lblDate.AutoSize = true;
            this.lblDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.lblDate.Location = new System.Drawing.Point(15, 133);
            this.lblDate.Name = "lblDate";
            this.lblDate.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblDate.Size = new System.Drawing.Size(112, 24);
            this.lblDate.TabIndex = 24;
            this.lblDate.Text = "88-88-8888";
            this.lblDate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lvRemote
            // 
            this.lvRemote.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvRemote.BackColor = System.Drawing.SystemColors.ControlLight;
            this.lvRemote.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lvRemote.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.lvRemote.FullRowSelect = true;
            this.lvRemote.HideSelection = false;
            this.lvRemote.Location = new System.Drawing.Point(229, 274);
            this.lvRemote.MultiSelect = false;
            this.lvRemote.Name = "lvRemote";
            this.lvRemote.Size = new System.Drawing.Size(551, 194);
            this.lvRemote.TabIndex = 27;
            this.lvRemote.TileSize = new System.Drawing.Size(200, 36);
            this.lvRemote.UseCompatibleStateImageBehavior = false;
            this.lvRemote.View = System.Windows.Forms.View.Details;
            this.lvRemote.SelectedIndexChanged += new System.EventHandler(this.LvRemote_SelectedIndexChanged);
            this.lvRemote.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LvRemote_MouseDoubleClick);
            // 
            // btnUpload
            // 
            this.btnUpload.Enabled = false;
            this.btnUpload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUpload.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnUpload.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnUpload.Location = new System.Drawing.Point(20, 206);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(190, 30);
            this.btnUpload.TabIndex = 28;
            this.btnUpload.Text = "Upload Ke Cloud ...";
            this.btnUpload.UseVisualStyleBackColor = true;
            this.btnUpload.Click += new System.EventHandler(this.BtnUpload_Click);
            // 
            // btnDownload
            // 
            this.btnDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDownload.Enabled = false;
            this.btnDownload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDownload.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnDownload.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnDownload.Location = new System.Drawing.Point(20, 369);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(190, 30);
            this.btnDownload.TabIndex = 29;
            this.btnDownload.Text = "Simpan Ke Lokal ...";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.BtnDownload_Click);
            // 
            // btnDdl
            // 
            this.btnDdl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDdl.Enabled = false;
            this.btnDdl.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDdl.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnDdl.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnDdl.Location = new System.Drawing.Point(20, 405);
            this.btnDdl.Name = "btnDdl";
            this.btnDdl.Size = new System.Drawing.Size(190, 30);
            this.btnDdl.TabIndex = 30;
            this.btnDdl.Text = "Buat Link Download";
            this.btnDdl.UseVisualStyleBackColor = true;
            this.btnDdl.Click += new System.EventHandler(this.BtnDdl_Click);
            // 
            // tabUpDownProgress
            // 
            this.tabUpDownProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabUpDownProgress.Controls.Add(this.tabQueue);
            this.tabUpDownProgress.Controls.Add(this.tabOnProgress);
            this.tabUpDownProgress.Controls.Add(this.tabErrorFail);
            this.tabUpDownProgress.Controls.Add(this.tabSuccess);
            this.tabUpDownProgress.Location = new System.Drawing.Point(20, 483);
            this.tabUpDownProgress.Margin = new System.Windows.Forms.Padding(0);
            this.tabUpDownProgress.Name = "tabUpDownProgress";
            this.tabUpDownProgress.Padding = new System.Drawing.Point(0, 0);
            this.tabUpDownProgress.SelectedIndex = 0;
            this.tabUpDownProgress.Size = new System.Drawing.Size(765, 102);
            this.tabUpDownProgress.TabIndex = 31;
            // 
            // tabQueue
            // 
            this.tabQueue.Controls.Add(this.dgQueue);
            this.tabQueue.Location = new System.Drawing.Point(4, 22);
            this.tabQueue.Name = "tabQueue";
            this.tabQueue.Padding = new System.Windows.Forms.Padding(0, 2, 2, 1);
            this.tabQueue.Size = new System.Drawing.Size(757, 76);
            this.tabQueue.TabIndex = 3;
            this.tabQueue.Text = "Daftar Antrian";
            this.tabQueue.UseVisualStyleBackColor = true;
            // 
            // dgQueue
            // 
            this.dgQueue.AllowUserToAddRows = false;
            this.dgQueue.AllowUserToDeleteRows = false;
            this.dgQueue.AllowUserToOrderColumns = true;
            this.dgQueue.AllowUserToResizeRows = false;
            this.dgQueue.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgQueue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgQueue.Location = new System.Drawing.Point(0, 2);
            this.dgQueue.Name = "dgQueue";
            this.dgQueue.ReadOnly = true;
            this.dgQueue.RowHeadersVisible = false;
            this.dgQueue.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dgQueue.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgQueue.Size = new System.Drawing.Size(755, 73);
            this.dgQueue.TabIndex = 3;
            // 
            // tabOnProgress
            // 
            this.tabOnProgress.Controls.Add(this.dgOnProgress);
            this.tabOnProgress.Location = new System.Drawing.Point(4, 22);
            this.tabOnProgress.Name = "tabOnProgress";
            this.tabOnProgress.Padding = new System.Windows.Forms.Padding(0, 2, 2, 1);
            this.tabOnProgress.Size = new System.Drawing.Size(757, 76);
            this.tabOnProgress.TabIndex = 0;
            this.tabOnProgress.Text = "Sedang Berjalan";
            this.tabOnProgress.UseVisualStyleBackColor = true;
            // 
            // dgOnProgress
            // 
            this.dgOnProgress.AllowUserToAddRows = false;
            this.dgOnProgress.AllowUserToDeleteRows = false;
            this.dgOnProgress.AllowUserToOrderColumns = true;
            this.dgOnProgress.AllowUserToResizeRows = false;
            this.dgOnProgress.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgOnProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgOnProgress.Location = new System.Drawing.Point(0, 2);
            this.dgOnProgress.Name = "dgOnProgress";
            this.dgOnProgress.ReadOnly = true;
            this.dgOnProgress.RowHeadersVisible = false;
            this.dgOnProgress.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dgOnProgress.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgOnProgress.Size = new System.Drawing.Size(755, 73);
            this.dgOnProgress.TabIndex = 2;
            // 
            // tabErrorFail
            // 
            this.tabErrorFail.Controls.Add(this.dgErrorFail);
            this.tabErrorFail.Location = new System.Drawing.Point(4, 22);
            this.tabErrorFail.Name = "tabErrorFail";
            this.tabErrorFail.Padding = new System.Windows.Forms.Padding(0, 2, 2, 1);
            this.tabErrorFail.Size = new System.Drawing.Size(757, 76);
            this.tabErrorFail.TabIndex = 1;
            this.tabErrorFail.Text = "Error / Gagal";
            this.tabErrorFail.UseVisualStyleBackColor = true;
            // 
            // dgErrorFail
            // 
            this.dgErrorFail.AllowUserToAddRows = false;
            this.dgErrorFail.AllowUserToDeleteRows = false;
            this.dgErrorFail.AllowUserToOrderColumns = true;
            this.dgErrorFail.AllowUserToResizeRows = false;
            this.dgErrorFail.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgErrorFail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgErrorFail.Location = new System.Drawing.Point(0, 2);
            this.dgErrorFail.Name = "dgErrorFail";
            this.dgErrorFail.ReadOnly = true;
            this.dgErrorFail.RowHeadersVisible = false;
            this.dgErrorFail.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dgErrorFail.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgErrorFail.Size = new System.Drawing.Size(755, 73);
            this.dgErrorFail.TabIndex = 3;
            // 
            // tabSuccess
            // 
            this.tabSuccess.Controls.Add(this.dgSuccess);
            this.tabSuccess.Location = new System.Drawing.Point(4, 22);
            this.tabSuccess.Name = "tabSuccess";
            this.tabSuccess.Padding = new System.Windows.Forms.Padding(0, 2, 2, 1);
            this.tabSuccess.Size = new System.Drawing.Size(757, 76);
            this.tabSuccess.TabIndex = 2;
            this.tabSuccess.Text = "Berhasil";
            this.tabSuccess.UseVisualStyleBackColor = true;
            // 
            // dgSuccess
            // 
            this.dgSuccess.AllowUserToAddRows = false;
            this.dgSuccess.AllowUserToDeleteRows = false;
            this.dgSuccess.AllowUserToOrderColumns = true;
            this.dgSuccess.AllowUserToResizeRows = false;
            this.dgSuccess.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgSuccess.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgSuccess.Location = new System.Drawing.Point(0, 2);
            this.dgSuccess.Name = "dgSuccess";
            this.dgSuccess.ReadOnly = true;
            this.dgSuccess.RowHeadersVisible = false;
            this.dgSuccess.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dgSuccess.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgSuccess.Size = new System.Drawing.Size(755, 73);
            this.dgSuccess.TabIndex = 4;
            // 
            // btnExportLaporan
            // 
            this.btnExportLaporan.Enabled = false;
            this.btnExportLaporan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportLaporan.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnExportLaporan.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnExportLaporan.Location = new System.Drawing.Point(20, 319);
            this.btnExportLaporan.Name = "btnExportLaporan";
            this.btnExportLaporan.Size = new System.Drawing.Size(190, 30);
            this.btnExportLaporan.TabIndex = 32;
            this.btnExportLaporan.Text = "Export Laporan";
            this.btnExportLaporan.UseVisualStyleBackColor = true;
            this.btnExportLaporan.Click += new System.EventHandler(this.BtnExportLaporan_Click);
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // cbDeleteOnComplete
            // 
            this.cbDeleteOnComplete.AutoSize = true;
            this.cbDeleteOnComplete.Checked = true;
            this.cbDeleteOnComplete.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDeleteOnComplete.Location = new System.Drawing.Point(31, 247);
            this.cbDeleteOnComplete.Name = "cbDeleteOnComplete";
            this.cbDeleteOnComplete.Size = new System.Drawing.Size(170, 17);
            this.cbDeleteOnComplete.TabIndex = 33;
            this.cbDeleteOnComplete.Text = "Hapus Setelah Selesai Upload";
            this.cbDeleteOnComplete.UseVisualStyleBackColor = true;
            // 
            // cbReplaceIfExist
            // 
            this.cbReplaceIfExist.AutoSize = true;
            this.cbReplaceIfExist.Checked = true;
            this.cbReplaceIfExist.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbReplaceIfExist.Location = new System.Drawing.Point(31, 270);
            this.cbReplaceIfExist.Name = "cbReplaceIfExist";
            this.cbReplaceIfExist.Size = new System.Drawing.Size(158, 17);
            this.cbReplaceIfExist.TabIndex = 34;
            this.cbReplaceIfExist.Text = "Timpa File Yang Sudah Ada";
            this.cbReplaceIfExist.UseVisualStyleBackColor = true;
            // 
            // Expired
            // 
            this.Expired.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Expired.AutoSize = true;
            this.Expired.Location = new System.Drawing.Point(25, 448);
            this.Expired.Name = "Expired";
            this.Expired.Size = new System.Drawing.Size(28, 13);
            this.Expired.TabIndex = 36;
            this.Expired.Text = "Exp.";
            // 
            // dtpExp
            // 
            this.dtpExp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.dtpExp.CustomFormat = " dd MMM yyyy - HH:mm:ss";
            this.dtpExp.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpExp.Location = new System.Drawing.Point(55, 445);
            this.dtpExp.Name = "dtpExp";
            this.dtpExp.Size = new System.Drawing.Size(155, 20);
            this.dtpExp.TabIndex = 37;
            // 
            // timerQueue
            // 
            this.timerQueue.Interval = 3000;
            this.timerQueue.Tick += new System.EventHandler(this.TimerQueue_Tick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(25, 296);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(108, 13);
            this.label2.TabIndex = 38;
            this.label2.Text = "Max. Upload Berjalan";
            // 
            // numMaxProcess
            // 
            this.numMaxProcess.Location = new System.Drawing.Point(139, 293);
            this.numMaxProcess.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numMaxProcess.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numMaxProcess.Name = "numMaxProcess";
            this.numMaxProcess.Size = new System.Drawing.Size(71, 20);
            this.numMaxProcess.TabIndex = 39;
            this.numMaxProcess.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numMaxProcess.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // CMainPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.numMaxProcess);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dtpExp);
            this.Controls.Add(this.Expired);
            this.Controls.Add(this.cbReplaceIfExist);
            this.Controls.Add(this.cbDeleteOnComplete);
            this.Controls.Add(this.btnExportLaporan);
            this.Controls.Add(this.tabUpDownProgress);
            this.Controls.Add(this.btnDdl);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.btnUpload);
            this.Controls.Add(this.lvRemote);
            this.Controls.Add(this.lblRefresh);
            this.Controls.Add(this.lblTime);
            this.Controls.Add(this.lblDate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtFilter);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnHome);
            this.Controls.Add(this.txtDirPath);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.chkWindowsStartup);
            this.Controls.Add(this.userInfo);
            this.Controls.Add(this.imgDomar);
            this.Controls.Add(this.prgrssBrStatus);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.appInfo);
            this.Name = "CMainPanel";
            this.Size = new System.Drawing.Size(800, 600);
            this.Load += new System.EventHandler(this.CMainPanel_Load);
            ((System.ComponentModel.ISupportInitialize)(this.imgDomar)).EndInit();
            this.tabUpDownProgress.ResumeLayout(false);
            this.tabQueue.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgQueue)).EndInit();
            this.tabOnProgress.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgOnProgress)).EndInit();
            this.tabErrorFail.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgErrorFail)).EndInit();
            this.tabSuccess.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgSuccess)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxProcess)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkWindowsStartup;
        private System.Windows.Forms.Label userInfo;
        private System.Windows.Forms.PictureBox imgDomar;
        private System.Windows.Forms.ProgressBar prgrssBrStatus;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label appInfo;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox txtDirPath;
        private System.Windows.Forms.Button btnHome;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.TextBox txtFilter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblRefresh;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Label lblDate;
        private System.Windows.Forms.ListView lvRemote;
        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Button btnDdl;
        private System.Windows.Forms.TabControl tabUpDownProgress;
        private System.Windows.Forms.TabPage tabQueue;
        private System.Windows.Forms.DataGridView dgQueue;
        private System.Windows.Forms.TabPage tabOnProgress;
        private System.Windows.Forms.DataGridView dgOnProgress;
        private System.Windows.Forms.TabPage tabErrorFail;
        private System.Windows.Forms.DataGridView dgErrorFail;
        private System.Windows.Forms.TabPage tabSuccess;
        private System.Windows.Forms.DataGridView dgSuccess;
        private System.Windows.Forms.Button btnExportLaporan;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.CheckBox cbDeleteOnComplete;
        private System.Windows.Forms.CheckBox cbReplaceIfExist;
        private System.Windows.Forms.Label Expired;
        private System.Windows.Forms.DateTimePicker dtpExp;
        private System.Windows.Forms.Timer timerQueue;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numMaxProcess;
    }

}
