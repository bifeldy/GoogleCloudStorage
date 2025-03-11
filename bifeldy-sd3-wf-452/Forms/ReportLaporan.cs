/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Tidak Butuh Untuk Didaftarkan Ke DI Container
 * 
 */

using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

using Microsoft.Reporting.WinForms;

namespace GoogleCloudStorage.Forms {

    public sealed partial class CReportLaporan : Form {

        private bool isInitialized = false;

        public CReportLaporan() {
            this.InitializeComponent();
        }

        public bool SetLaporan(DataTable dtReport, List<ReportParameter> paramList, string rdlcPath, string dsName) {
            bool isReady = true;
            if (rdlcPath == null || dsName == null) {
                isReady = false;
            }

            if (dtReport == null || dtReport.Rows.Count <= 0) {
                isReady = false;
            }

            if (paramList == null || paramList.Count <= 0) {
                isReady = false;
            }

            if (isReady) {
                this.rptViewer.LocalReport.ReportPath = rdlcPath;
                this.rptViewer.LocalReport.DataSources.Add(new ReportDataSource(dsName, dtReport));
                this.rptViewer.LocalReport.SetParameters(paramList);
            }
            else {
                _ = MessageBox.Show("Tidak Ada Data", $"Report Viewer :: {dsName}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return isReady;
        }

        private void CReportLaporan_FormClosing(object sender, FormClosingEventArgs e) {
            this.rptViewer.LocalReport.ReportPath = null;
            this.rptViewer.LocalReport.DataSources.Clear();
            this.rptViewer.RefreshReport();
            this.Dispose();
        }

        private void CReportLaporan_Load(object sender, System.EventArgs e) {
            if (!this.isInitialized) {

                this.rptViewer.RefreshReport();

                this.isInitialized = true;
            }
        }

    }

}
