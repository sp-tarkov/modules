using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using BepInEx.Logging;
using SPT.Common.Utils;

namespace SPT.Core.Utils;

public static class ValidationUtil
{
    private static ManualLogSource _logger = Logger.CreateLogSource("SPT.Core");
    public static string _crashHandler = "0";
    private static bool _hasRun;
    

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
    private static extern int RegOpenKeyEx(IntPtr hKey, string subKey, int options, int samDesired, out IntPtr phkResult);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
    private static extern int RegQueryValueEx(
        IntPtr hKey,
        string lpValueName,
        IntPtr lpReserved,
        out uint lpType,
        StringBuilder lpData,
        ref uint lpcbData
    );

    [DllImport("advapi32.dll")]
    private static extern int RegCloseKey(IntPtr hKey);

    public static bool Validate()
    {
        const string c0 = @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\EscapeFromTarkov";
        var l1 = 0;

        try
        {
            var l4 = l1ll(c0, Encoding.UTF8.GetString(Convert.FromBase64String("SW5zdGFsbExvY2F0aW9u")));

            var l3 = (l4 != null) ? l4.ToString() : string.Empty;
            var l2 = new DirectoryInfo(l3);
            var l6 = l1l(Directory.GetCurrentDirectory());
            var ll = new FileSystemInfo[]
            {
                l2,
                new FileInfo(Path.Combine(l3, Encoding.UTF8.GetString(Convert.FromBase64String("QmF0dGxFeWVcQkVDbGllbnRfeDY0LmRsbA==")))),
                new FileInfo(Path.Combine(l3, Encoding.UTF8.GetString(Convert.FromBase64String("QmF0dGxFeWVcQkVTZXJ2aWNlX3g2NC5leGU=")))),
                new FileInfo(Path.Combine(l3, Encoding.UTF8.GetString(Convert.FromBase64String("Q29uc2lzdGVuY3lJbmZv")))),
                new FileInfo(Path.Combine(l3, Encoding.UTF8.GetString(Convert.FromBase64String("VW5pbnN0YWxsLmV4ZQ==")))),
                new FileInfo(Path.Combine(l3, Encoding.UTF8.GetString(Convert.FromBase64String("VW5pdHlDcmFzaEhhbmRsZXI2NC5leGU="))))
            };

            if (_hasRun ? false : !_hasRun ? true : false)
            {
                _crashHandler = ll1(l3, Encoding.UTF8.GetString(Convert.FromBase64String("VW5pdHlDcmFzaEhhbmRsZXI2NC5leGU=")))?.Length.ToString() ?? "0";
                _logger.LogInfo(_crashHandler);
                _logger.LogInfo(ll1(l3, Encoding.UTF8.GetString(Convert.FromBase64String("VW5pbnN0YWxsLmV4ZQ==")))?.Length.ToString() ?? "0");
                _logger.LogInfo(ll1(l3, Encoding.UTF8.GetString(Convert.FromBase64String("UmVnaXN0ZXIuYmF0")))?.Length.ToString() ?? "0");
                _logger.LogInfo(ll1(Directory.GetCurrentDirectory(), Encoding.UTF8.GetString(Convert.FromBase64String("UmVnaXN0ZXIuYmF0")))?.Length.ToString() ?? "0");
                var lll = ll1(Directory.GetCurrentDirectory(), Encoding.UTF8.GetString(Convert.FromBase64String("UmVnaXN0ZXIgR2FtZS5leGU=")))
                    ?.Length.ToString() ?? "0";
                _logger.LogInfo(lll);

                _logger.LogInfo(Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join(",", l6))));
                if (_crashHandler == "0" || lll != "0")
                    ServerLog.Debug("SPT.Core", "-1");
                    

                _hasRun = !_hasRun ? true : false;
            }

            l1 = ll.Length - 1;

            foreach (var l in ll)
            {
                if (File.Exists(l.FullName))
                {
                    --l1;
                }
            }
        }
        catch
        {
            l1 = -1;
        }

        complete = l1 == 0;

        return l1 == 0;
    }

    private static string l1ll(string ll, string l1)
    {
        try
        {
            var h = new IntPtr(-2147483646);
            var r = RegOpenKeyEx(h, ll, 0, 0x20019, out IntPtr k);
            if (r == 0)
            {
                uint s = 0;
                var sr = RegQueryValueEx(k, l1, IntPtr.Zero, out uint t, null, ref s);
                if (sr == 0 && s > 0)
                {
                    var buf = new StringBuilder((int)s);
                    int vr = RegQueryValueEx(k, l1, IntPtr.Zero, out _, buf, ref s);
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
        catch { }
        return null;
    }

    private static FileInfo ll1(string l1, string ll)
    {
        var a = Path.Combine(l1, ll);
        return File.Exists(a) ? new FileInfo(a) : null;
    }

    public static bool complete = false;

    private static HashSet<string> l1l(string ll)
    {
        var l = new HashSet<string>();

        try
        {
            if (!Directory.Exists(ll))
            {
                return l;
            }

            foreach (var l7 in Directory.EnumerateFiles(ll))
            {
                l.Add(Path.GetFileName(l7));
            }
        }
        catch (Exception ex)
        {
        }

        return l;
    }
}
