using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SPT.Common.Utils;
using SPT.Custom.Models;

namespace SPT.Custom.Utils;

public static class BundleCrcCache
{
    private static readonly string CachePath =
        Path.Combine(AppContext.BaseDirectory, "SPT/user/cache/bundle_crc_cache.json");

    private static Dictionary<string, CrcCacheEntry> _cache = new();

    public static void Load()
    {
        if (!File.Exists(CachePath))
        {
            return;
        }

        var json = VFS.ReadTextFile(CachePath);
        _cache = JsonConvert.DeserializeObject<
                     Dictionary<string, CrcCacheEntry>
                 >(json)
                 ?? new Dictionary<string, CrcCacheEntry>();
    }

    public static void Save()
    {
        var json = JsonConvert.SerializeObject(
            _cache,
            Formatting.Indented
        );

        File.WriteAllText(CachePath, json);
    }

    public static bool TryGet(string key, out CrcCacheEntry entry)
    {
        return _cache.TryGetValue(key, out entry);
    }

    public static void Update(string key, long size, uint crc)
    {
        _cache[key] = new CrcCacheEntry
        {
            Size = size,
            Crc = crc
        };
    }
}
