#region DEPRECATED, REMOVE IN 3.8.1

using System;

namespace Aki.Custom.Models
{
    [Obsolete("BundleInfo is deprecated, please use BundleItem instead.")]
    public class BundleInfo
    {
        public string Key { get; }
        public string Path { get; set; }
        public string[] DependencyKeys { get; }

        public BundleInfo(string key, string path, string[] dependencyKeys)
        {
            Key = key;
            Path = path;
            DependencyKeys = dependencyKeys;
        }
    }
}

#endregion