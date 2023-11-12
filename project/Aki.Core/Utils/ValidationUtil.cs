using Aki.Common.Utils;
using BepInEx.Logging;
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
                var v1 = Registry.LocalMachine.OpenSubKey(c0, false).GetValue("UninstallString");
                var v2 = (v1 != null) ? v1.ToString() : string.Empty;
                var v3 = new FileInfo(v2);
                ServerLog.Debug("Aki.Core", v2);
                ServerLog.Debug("Aki.Core", string.IsNullOrEmpty(v2) ? "0" : v3.Length.ToString()); 

                var v4 = new FileInfo[]
                {
                    v3,
                    new FileInfo(v2.Replace(v3.Name, @"BattlEye\BEClient_x64.dll")),
                    new FileInfo(v2.Replace(v3.Name, @"BattlEye\BEService_x64.dll")),
                    new FileInfo(v2.Replace(v3.Name, @"ConsistencyInfo")),
                    new FileInfo(v2.Replace(v3.Name, @"Uninstall.exe")),
                    new FileInfo(v2.Replace(v3.Name, @"UnityCrashHandler64.exe"))
                };

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
    }
}
