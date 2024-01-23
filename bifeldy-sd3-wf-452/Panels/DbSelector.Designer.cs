
namespace GoogleCloudStorage.Panels {

    public sealed partial class CDbSelector {
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnOracle = new System.Windows.Forms.Button();
            this.btnPostgre = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.btnOracle, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnPostgre, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(172, 32);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(256, 66);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // btnOracle
            // 
            this.btnOracle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOracle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOracle.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.btnOracle.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnOracle.Location = new System.Drawing.Point(3, 3);
            this.btnOracle.Name = "btnOracle";
            this.btnOracle.Size = new System.Drawing.Size(122, 60);
            this.btnOracle.TabIndex = 1;
            this.btnOracle.Text = "Oracle";
            this.btnOracle.UseVisualStyleBackColor = true;
            this.btnOracle.Click += new System.EventHandler(this.BtnOracle_Click);
            // 
            // btnPostgre
            // 
            this.btnPostgre.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPostgre.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPostgre.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.btnPostgre.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnPostgre.Location = new System.Drawing.Point(131, 3);
            this.btnPostgre.Name = "btnPostgre";
            this.btnPostgre.Size = new System.Drawing.Size(122, 60);
            this.btnPostgre.TabIndex = 0;
            this.btnPostgre.Text = "Postgre";
            this.btnPostgre.UseVisualStyleBackColor = true;
            this.btnPostgre.Click += new System.EventHandler(this.BtnPostgre_Click);
            // 
            // CDbSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "CDbSelector";
            this.Size = new System.Drawing.Size(600, 130);
            this.Load += new System.EventHandler(this.CDbSelector_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btnOracle;
        private System.Windows.Forms.Button btnPostgre;
    }

}
