/**
 * 
 * Author       :: Basilius Bias Astho Christyono
 * Phone        :: (+62) 889 236 6466
 * 
 * Department   :: IT SD 03
 * Mail         :: bias@indomaret.co.id
 * 
 * Catatan      :: Add-On Untuk Report Viewer
 *              :: Seharusnya Tidak Untuk Didaftarkan Ke DI Container
 * 
 */

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace GoogleCloudStorage.SqlServerTypes {

    /// <summary>
    /// Utility methods related to CLR Types for SQL Server 
    /// </summary>
    public static class CLoader {

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string libname);

        /// <summary>
        /// Loads the required native assemblies for the current architecture (x86 or x64)
        /// </summary>
        /// <param name="rootApplicationPath">
        /// Root path of the current application. Use Server.MapPath(".") for ASP.NET applications
        /// and AppDomain.CurrentDomain.BaseDirectory for desktop applications.
        /// </param>
        public static void LoadNativeAssemblies(string rootApplicationPath) {
            string arch = IntPtr.Size > 4 ? "x64" : "x86";
            string nativeBinaryPath = Path.Combine(rootApplicationPath, $@"SqlServerTypes\{arch}\");
            LoadNativeAssembly(nativeBinaryPath, "msvcr120.dll");
            LoadNativeAssembly(nativeBinaryPath, "SqlServerSpatial140.dll");
        }

        private static void LoadNativeAssembly(string nativeBinaryPath, string assemblyName) {
            string path = Path.Combine(nativeBinaryPath, assemblyName);
            IntPtr ptr = LoadLibrary(path);
            if (ptr == IntPtr.Zero) {
                int errCode = Marshal.GetLastWin32Error();
                throw new Exception($"Error loading {assemblyName} (ErrorCode: {errCode})");
            }
        }

    }

}