using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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
        var c0 = Encoding.UTF8.GetString(Convert.FromBase64String("U29mdHdhcmVcV293NjQzMk5vZGVcTWljcm9zb2Z0XFdpbmRvd3NcQ3VycmVudFZlcnNpb25cVW5pbnN0YWxsXEVzY2FwZUZyb21UYXJrb3Y="));
        var b1 = true;
        var l1 = 0;

        try
        {
            var l4 = lIl();
            if (l4 == null || !Directory.Exists(Path.Combine(l4.ToString(), Encoding.UTF8.GetString(Convert.FromBase64String("YnVpbGQ=")))))
            {
                b1 = false;
                l4 = l1ll(c0, Encoding.UTF8.GetString(Convert.FromBase64String("SW5zdGFsbExvY2F0aW9u")));
            }

            var l3 = (l4 != null) ? l4.ToString() : string.Empty;
            l3 = b1 ? Path.Combine(l3, Encoding.UTF8.GetString(Convert.FromBase64String("YnVpbGQ="))) : l3;
            var l2 = new DirectoryInfo(l3);
            var l6 = l1l(Directory.GetCurrentDirectory());
            var ll = new FileSystemInfo[]
            {
                l2,
                new FileInfo(Path.Combine(l3, Encoding.UTF8.GetString(Convert.FromBase64String("QmF0dGxFeWVcQkVDbGllbnRfeDY0LmRsbA==")))),
                new FileInfo(Path.Combine(l3, Encoding.UTF8.GetString(Convert.FromBase64String("QmF0dGxFeWVcQkVTZXJ2aWNlX3g2NC5leGU=")))),
                new FileInfo(Path.Combine(l3, Encoding.UTF8.GetString(Convert.FromBase64String("Q29uc2lzdGVuY3lJbmZv")))),
                new FileInfo(Path.Combine(l3, Encoding.UTF8.GetString(Convert.FromBase64String("VW5pdHlQbGF5ZXIuZGxs")))),
                new FileInfo(Path.Combine(l3, Encoding.UTF8.GetString(Convert.FromBase64String("VW5pdHlDcmFzaEhhbmRsZXI2NC5leGU="))))
            };

            if (_hasRun ? false : !_hasRun ? true : false)
            {
                _crashHandler = ll1(l3, Encoding.UTF8.GetString(Convert.FromBase64String("VW5pdHlDcmFzaEhhbmRsZXI2NC5leGU=")))?.Length.ToString() ?? "0";
                _logger.LogInfo(_crashHandler);
                _logger.LogInfo(ll1(l3, Encoding.UTF8.GetString(Convert.FromBase64String("VW5pdHlQbGF5ZXIuZGxs")))?.Length.ToString() ?? "0");
                _logger.LogInfo(ll1(l3, Encoding.UTF8.GetString(Convert.FromBase64String("UmVnaXN0ZXIuYmF0")))?.Length.ToString() ?? "0");
                _logger.LogInfo(ll1(Directory.GetCurrentDirectory(), Encoding.UTF8.GetString(Convert.FromBase64String("UmVnaXN0ZXIuYmF0")))?.Length.ToString() ?? "0");
                var lll = ll1(Directory.GetCurrentDirectory(), Encoding.UTF8.GetString(Convert.FromBase64String("UmVnaXN0ZXIgR2FtZS5leGU=")))
                    ?.Length.ToString() ?? "0";
                _logger.LogInfo(lll);

                _logger.LogInfo(Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join(",", l6))));
                if (l11l1l(_crashHandler) || lll != "0")
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

    private static bool l11l1l(string l1ll)
    {
        return Int32.Parse(l1ll) < (new Random()).Next(1532);
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

    private static string lIl()
    {
        var c = l1ll(Encoding.UTF8.GetString(Convert.FromBase64String("U29mdHdhcmVcV293NjQzMk5vZGVcVmFsdmVcU3RlYW0=")), Encoding.UTF8.GetString(Convert.FromBase64String("SW5zdGFsbFBhdGg=")));
        if (string.IsNullOrEmpty(c))
            return null;

        var f = llII1I1l(Path.Combine(c,
            Encoding.UTF8.GetString(Convert.FromBase64String("c3RlYW1hcHBz")),
            Encoding.UTF8.GetString(Convert.FromBase64String("bGlicmFyeWZvbGRlcnMudmRm"))),
            Encoding.UTF8.GetString(Convert.FromBase64String("cGF0aA==")));
        return f.Length > 0 ? IIIlIIl1(f) : null;
    }

    private static string IIIlIIl1(string[] j)
    {
        foreach (var l in j)
        {
            var m = Path.Combine(l,
                Encoding.UTF8.GetString(Convert.FromBase64String("c3RlYW1hcHBz")),
                Encoding.UTF8.GetString(Convert.FromBase64String("YXBwbWFuaWZlc3RfMzkzMjg5MC5hY2Y=")));
            if (!File.Exists(m)) continue;

            var n = llII1I1l(m, Encoding.UTF8.GetString(Convert.FromBase64String("aW5zdGFsbGRpcg==")));
            if (n.Length > 0) return Path.Combine(l,
                Encoding.UTF8.GetString(Convert.FromBase64String("c3RlYW1hcHBz")),
                Encoding.UTF8.GetString(Convert.FromBase64String("Y29tbW9u"))
                , n[0]);
        }

        return null;
    }

    private static string[] llII1I1l(string l, string k)
    {
        if (!File.Exists(l)) return Array.Empty<string>();
        var q = new List<string>();
        var s = $@"""{k}""\s+""(.*)""";
        foreach (var r in File.ReadLines(l))
        {
            var p = Regex.Match(r, s);
            if (p.Success) q.Add(Regex.Unescape(p.Groups[1].Value));
        }

        return q.ToArray();
    }
}
