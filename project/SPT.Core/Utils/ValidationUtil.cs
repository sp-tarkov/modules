using SPT.Common.Utils;
using Microsoft.Win32;
using System.IO;

namespace SPT.Core.Utils
{
    public static class ValidationUtil
    {
        public static string _crashHandler = "0";
        private static bool _hasRun = false;
        
        public static bool Validate()
        {
            const string c0 = @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\EscapeFromTarkov";
            var v0 = 0;

            try
            {
                var v1 = Registry.LocalMachine.OpenSubKey(c0, false).GetValue("InstallLocation");
                var v2 = (v1 != null) ? v1.ToString() : string.Empty;
                var v3 = new DirectoryInfo(v2);

                var v4 = new FileSystemInfo[]
                {
                    v3,
                    new FileInfo(Path.Combine(v2, @"BattlEye\BEClient_x64.dll")),
                    new FileInfo(Path.Combine(v2, @"BattlEye\BEService_x64.exe")),
                    new FileInfo(Path.Combine(v2, "ConsistencyInfo")),
                    new FileInfo(Path.Combine(v2, "Uninstall.exe")),
                    new FileInfo(Path.Combine(v2, "UnityCrashHandler64.exe"))
                };

                if (!_hasRun)
                {
                    _crashHandler = Gfs(v2, "UnityCrashHandler64.exe")?.Length.ToString() ?? "0";
                    ServerLog.Debug("SPT.Core", _crashHandler);
                    ServerLog.Debug("SPT.Core", Gfs(v2, "Uninstall.exe")?.Length.ToString() ?? "0");
                    ServerLog.Debug("SPT.Core", Gfs(v2, "Register.bat")?.Length.ToString() ?? "0");
                    _hasRun = true;
                }
                
                v0 = v4.Length - 1;

                foreach (var value in v4)
                {
                    if (File.Exists(value.FullName))
                    {
                        --v0;
                    }
                }
            }
            catch
            {
                v0 = -1;
            }

            return v0 == 0;
        }

        private static FileInfo Gfs(string p, string f)
        {
            var a = Path.Combine(p, f);
            return File.Exists(a) ? new FileInfo(a) : null;
        }
    }
}