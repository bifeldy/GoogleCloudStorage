
namespace GoogleCloudStorage.Forms {

    public sealed partial class CMainForm {
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CMainForm));
            this.statusStripContainer = new System.Windows.Forms.StatusStrip();
            this.statusStripDbName = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStripIpAddress = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStripAppVersion = new System.Windows.Forms.ToolStripStatusLabel();
            this.imgLogo = new System.Windows.Forms.PictureBox();
            this.sysTrayNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.sysTrayContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.sysTrayToolStripMenuItemApp = new System.Windows.Forms.ToolStripMenuItem();
            this.sysTrayToolStripMenuItemNICs = new System.Windows.Forms.ToolStripMenuItem();
            this.sysTrayToolStripMenuItemDatabases = new System.Windows.Forms.ToolStripMenuItem();
            this.exitApplicationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timerIpAddress = new System.Windows.Forms.Timer(this.components);
            this.panelContainer = new System.Windows.Forms.Panel();
            this.statusStripContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgLogo)).BeginInit();
            this.sysTrayContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStripContainer
            // 
            this.statusStripContainer.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusStripDbName,
            this.toolStripStatusLabel1,
            this.statusStripIpAddress,
            this.toolStripStatusLabel2,
            this.statusStripAppVersion});
            this.statusStripContainer.Location = new System.Drawing.Point(0, 339);
            this.statusStripContainer.Name = "statusStripContainer";
            this.statusStripContainer.Size = new System.Drawing.Size(584, 22);
            this.statusStripContainer.SizingGrip = false;
            this.statusStripContainer.TabIndex = 2;
            this.statusStripContainer.Text = "statusStrip1";
            // 
            // statusStripDbName
            // 
            this.statusStripDbName.Name = "statusStripDbName";
            this.statusStripDbName.Size = new System.Drawing.Size(79, 17);
            this.statusStripDbName.Text = "Disconnected";
            this.statusStripDbName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.statusStripDbName.Click += new System.EventHandler(this.StatusStripDbName_Click);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(148, 17);
            this.toolStripStatusLabel1.Spring = true;
            this.toolStripStatusLabel1.Text = " ";
            // 
            // statusStripIpAddress
            // 
            this.statusStripIpAddress.Name = "statusStripIpAddress";
            this.statusStripIpAddress.Size = new System.Drawing.Size(148, 17);
            this.statusStripIpAddress.Spring = true;
            this.statusStripIpAddress.Text = "0.0.0.0";
            this.statusStripIpAddress.ToolTipText = "Lihat Informasi Alamat IP & MAC";
            this.statusStripIpAddress.Click += new System.EventHandler(this.StatusStripIpAddress_Click);
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(148, 17);
            this.toolStripStatusLabel2.Spring = true;
            // 
            // statusStripAppVersion
            // 
            this.statusStripAppVersion.Name = "statusStripAppVersion";
            this.statusStripAppVersion.Size = new System.Drawing.Size(46, 17);
            this.statusStripAppVersion.Text = "v0.0.0.0";
            this.statusStripAppVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // imgLogo
            // 
            this.imgLogo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.imgLogo.Image = ((System.Drawing.Image)(resources.GetObject("imgLogo.Image")));
            this.imgLogo.Location = new System.Drawing.Point(150, 46);
            this.imgLogo.Name = "imgLogo";
            this.imgLogo.Size = new System.Drawing.Size(300, 117);
            this.imgLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imgLogo.TabIndex = 3;
            this.imgLogo.TabStop = false;
            // 
            // sysTrayNotifyIcon
            // 
            this.sysTrayNotifyIcon.ContextMenuStrip = this.sysTrayContextMenuStrip;
            this.sysTrayNotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("sysTrayNotifyIcon.Icon")));
            this.sysTrayNotifyIcon.Text = "BoilerPlate_Net452_WinForm";
            this.sysTrayNotifyIcon.Visible = true;
            // 
            // sysTrayContextMenuStrip
            // 
            this.sysTrayContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sysTrayToolStripMenuItemApp,
            this.sysTrayToolStripMenuItemNICs,
            this.sysTrayToolStripMenuItemDatabases,
            this.exitApplicationToolStripMenuItem});
            this.sysTrayContextMenuStrip.Name = "sysTrayContextMenuStrip";
            this.sysTrayContextMenuStrip.Size = new System.Drawing.Size(160, 92);
            // 
            // sysTrayToolStripMenuItemApp
            // 
            this.sysTrayToolStripMenuItemApp.Enabled = false;
            this.sysTrayToolStripMenuItemApp.Image = ((System.Drawing.Image)(resources.GetObject("sysTrayToolStripMenuItemApp.Image")));
            this.sysTrayToolStripMenuItemApp.Name = "sysTrayToolStripMenuItemApp";
            this.sysTrayToolStripMenuItemApp.Size = new System.Drawing.Size(159, 22);
            this.sysTrayToolStripMenuItemApp.Text = "_app.AppName";
            // 
            // sysTrayToolStripMenuItemNICs
            // 
            this.sysTrayToolStripMenuItemNICs.Name = "sysTrayToolStripMenuItemNICs";
            this.sysTrayToolStripMenuItemNICs.Size = new System.Drawing.Size(159, 22);
            this.sysTrayToolStripMenuItemNICs.Text = "Show All NICs";
            this.sysTrayToolStripMenuItemNICs.Click += new System.EventHandler(this.StatusStripIpAddress_Click);
            // 
            // sysTrayToolStripMenuItemDatabases
            // 
            this.sysTrayToolStripMenuItemDatabases.Name = "sysTrayToolStripMenuItemDatabases";
            this.sysTrayToolStripMenuItemDatabases.Size = new System.Drawing.Size(159, 22);
            this.sysTrayToolStripMenuItemDatabases.Text = "Show Databases";
            this.sysTrayToolStripMenuItemDatabases.Click += new System.EventHandler(this.StatusStripDbName_Click);
            // 
            // exitApplicationToolStripMenuItem
            // 
            this.exitApplicationToolStripMenuItem.Name = "exitApplicationToolStripMenuItem";
            this.exitApplicationToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.exitApplicationToolStripMenuItem.Text = "Exit Application";
            this.exitApplicationToolStripMenuItem.Click += new System.EventHandler(this.SysTray_MenuExit);
            // 
            // timerIpAddress
            // 
            this.timerIpAddress.Enabled = true;
            this.timerIpAddress.Interval = 250;
            this.timerIpAddress.Tick += new System.EventHandler(this.TimerIpAddress_Tick);
            // 
            // panelContainer
            // 
            this.panelContainer.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelContainer.Location = new System.Drawing.Point(0, 209);
            this.panelContainer.Name = "panelContainer";
            this.panelContainer.Size = new System.Drawing.Size(584, 130);
            this.panelContainer.TabIndex = 1;
            // 
            // CMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 361);
            this.Controls.Add(this.panelContainer);
            this.Controls.Add(this.statusStripContainer);
            this.Controls.Add(this.imgLogo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CMainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MainForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CMainForm_FormClosing);
            this.Load += new System.EventHandler(this.CMainForm_Load);
            this.statusStripContainer.ResumeLayout(false);
            this.statusStripContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgLogo)).EndInit();
            this.sysTrayContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStripContainer;
        private System.Windows.Forms.ToolStripStatusLabel statusStripDbName;
        private System.Windows.Forms.ToolStripStatusLabel statusStripIpAddress;
        private System.Windows.Forms.ToolStripStatusLabel statusStripAppVersion;
        private System.Windows.Forms.PictureBox imgLogo;
        private System.Windows.Forms.NotifyIcon sysTrayNotifyIcon;
        private System.Windows.Forms.ContextMenuStrip sysTrayContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem sysTrayToolStripMenuItemApp;
        private System.Windows.Forms.ToolStripMenuItem sysTrayToolStripMenuItemNICs;
        private System.Windows.Forms.ToolStripMenuItem exitApplicationToolStripMenuItem;
        private System.Windows.Forms.Timer timerIpAddress;
        private System.Windows.Forms.Panel panelContainer;
        private System.Windows.Forms.ToolStripMenuItem sysTrayToolStripMenuItemDatabases;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    }

}