﻿using Aki.Common.Utils;
using Microsoft.Win32;
using System.IO;

namespace Aki.Core.Utils
{
    public static class ValidationUtil
    {
        public static bool Validate()
        {
            var c0 = @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\EscapeFromTarkov";
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

                ServerLog.Debug("Aki.Core", Gfs(v2, "UnityCrashHandler64.exe")?.Length.ToString() ?? "0");
                ServerLog.Debug("Aki.Core", Gfs(v2, "Uninstall.exe")?.Length.ToString() ?? "0");
                ServerLog.Debug("Aki.Core", Gfs(v2, "Register.bat")?.Length.ToString() ?? "0");

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