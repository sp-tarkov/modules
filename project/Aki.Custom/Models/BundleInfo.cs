namespace Aki.Custom.Models
{
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
