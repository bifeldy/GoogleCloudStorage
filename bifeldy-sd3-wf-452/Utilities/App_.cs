/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Pengaturan Aplikasi
 *              :: Harap Didaftarkan Ke DI Container
 * 
 */

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using bifeldy_sd3_lib_452.Utilities;

namespace GoogleCloudStorage.Utilities {

    public interface IApp : IApplication {
        int ScreenWidth { get; }
        int ScreenHeight { get; }
        void Exit();
        string Author { get; }
        List<string> ListDcCanUse { get; }
    }

    public sealed class CApp : CApplication, IApp {

        public int ScreenWidth { get; }
        public int ScreenHeight { get; }

        public string Author { get; }

        public List<string> ListDcCanUse { get; }

        public CApp(IConfig config) : base(config) {
            this.ScreenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            this.ScreenHeight = Screen.PrimaryScreen.WorkingArea.Height;
            this.Author = "sd3@indomaret.co.id" + Environment.NewLine + "ssd3@indomaret.co.id";
            this.ListDcCanUse = new List<string> { /* "HO", "INDUK", "DEPO", "SEWA", "FROZEN", "PERISHABLE", "LPG" */ };
        }

        public void Exit() => Application.Exit();

    }

}
