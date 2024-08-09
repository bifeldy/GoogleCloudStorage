/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Entry Point
 * 
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

using bifeldy_sd3_lib_452;

using GoogleCloudStorage.Forms;
using GoogleCloudStorage.SqlServerTypes;
using GoogleCloudStorage.Utilities;

namespace GoogleCloudStorage {

    public static class CProgram {

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public static Bifeldy Bifeldyz { get; set; }

        [STAThread] // bifeldy-sd3-wf-452.exe -arg0 arg1 --arg2 "a r g 3"
        public static void Main(string[] args) {
            Process currentProcess = Process.GetCurrentProcess();
            Process[] allProcess = Process.GetProcessesByName(currentProcess.ProcessName);
            using (Mutex mutex = new Mutex(true, currentProcess.MainModule.ModuleName, out bool createdNew)) {
                if (createdNew) {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    // Report Viewer
                    CLoader.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

                    // Dependency Injection
                    Bifeldyz = new Bifeldy(args);

                    // Classes As Interfaces
                    // Bifeldyz.RegisterDiClassAsInterface<CClass, IInterface>();
                    Bifeldyz.RegisterDiClassAsInterfaceByNamespace(Assembly.GetExecutingAssembly(), new string[] {
                        "GoogleCloudStorage.Handlers",
                        "GoogleCloudStorage.Utilities"
                    });

                    // Classes Only
                    // Bifeldyz.RegisterDiClass<CClass>();
                    Bifeldyz.RegisterDiClassByNamespace(Assembly.GetExecutingAssembly(), new string[] {
                        /* "GoogleCloudStorage.Forms", */
                        "GoogleCloudStorage.Panels"
                    });

                    // Khusus Form Bisa Di Bikin Independen, Jadinya Gak Wajib Masuk Ke Dependency Injection (DI), Form Utama Yang Wajib DI
                    // Kalau Singleton Form Di Panggil Via Resolve DI, Saat Di Close(); Kena Dispose GC, Tidak Bisa Resolve Lagi
                    // Isi Parameter Dengan `false` Supaya Selalu Dapat Object Baru Setiap Instance (Bukan Singleton)
                    //
                    // Misal :: Di Buat Dan Di Panggil Dari From Lain
                    //
                    // Tanpa DI, Tidak Bisa Lanjut Pakai DI, Constructor Bersih
                    //
                    //     CReportLaporan reportLaporan = new CReportLaporan();
                    //
                    // Kalau Via Resolve DI, Enaknya Bisa Lanjut Pakai DI Juga Di Constructor
                    //
                    //     CReportLaporan reportLaporan = CProgram.Bifeldyz.Resolve<CReportLaporan>();
                    //
                    Bifeldyz.RegisterDiClass<CMainForm>(false);
                    Bifeldyz.RegisterDiClass<CReportLaporan>(false);

                    using (dynamic lifetimeScope = Bifeldyz.BeginLifetimeScope()) {
                        IApp _app = Bifeldyz.Resolve<IApp>();
                        string changeLog = Path.Combine(_app.AppLocation, "ChangeLog.txt");
                        if (File.Exists(changeLog)) {
                            string cl = File.ReadAllText(changeLog);
                            MessageBox.Show(cl, $"[ChangeLog] {_app.AppName}", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        Application.Run(Bifeldyz.Resolve<CMainForm>());
                    }
                }
                else {
                    foreach (Process process in allProcess) {
                        if (process.Id != currentProcess.Id) {
                            SetForegroundWindow(process.MainWindowHandle);
                            MessageBox.Show(
                                "Program Saat Ini Sudah Berjalan, Cek System Tray Icon Kanan Bawah Windows",
                                currentProcess.MainModule.ModuleName,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            break;
                        }
                    }
                }
            }
        }

    }

}
