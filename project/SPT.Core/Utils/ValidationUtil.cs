﻿using SPT.Common.Utils;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SPT.Core.Utils
{
    public static class ValidationUtil
    {
        public static string _crashHandler = "0";
        private static bool _hasRun;

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int RegOpenKeyEx(IntPtr hKey, string subKey, int options, int samDesired, out IntPtr phkResult);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int RegQueryValueEx(IntPtr hKey, string lpValueName, IntPtr lpReserved, out uint lpType, StringBuilder lpData, ref uint lpcbData);

        [DllImport("advapi32.dll")]
        private static extern int RegCloseKey(IntPtr hKey);

        public static bool Validate()
        {
            const string c0 = @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\EscapeFromTarkov";
            var v0 = 0;

            try
            {
                var v1 = Rfs(c0, "InstallLocation");

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
                    if (_crashHandler == "0") ServerLog.Debug("SPT.Core", "-1");

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

        private static string Rfs(string a, string b)
        {
            try
            {
                var h = new IntPtr(-2147483646);
                int r = RegOpenKeyEx(h, a, 0, 0x20019, out IntPtr k);
                if (r == 0)
                {
                    uint s = 0;
                    int sr = RegQueryValueEx(k, b, IntPtr.Zero, out uint t, null, ref s);
                    if (sr == 0 && s > 0)
                    {
                        var buf = new StringBuilder((int)s);
                        int vr = RegQueryValueEx(k, b, IntPtr.Zero, out _, buf, ref s);
                        if (vr == 0)
                        {
                            var res = buf.ToString();
                            RegCloseKey(k);
                            return res;
                        }
                    }
                    RegCloseKey(k);
                }
            }
            catch
            {
            }
            return null;
        }

        private static FileInfo Gfs(string p, string f)
        {
            var a = Path.Combine(p, f);
            return File.Exists(a) ? new FileInfo(a) : null;
        }
    }
}