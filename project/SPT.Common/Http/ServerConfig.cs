namespace SPT.Common.Http
{
    public class ServerConfig
    {
        public string BackendUrl { get; }
        public string MatchingVersion { get; }
        public string Version { get; }

        public ServerConfig(string backendUrl, string matchingVersion, string version)
        {
            BackendUrl = backendUrl;
            MatchingVersion = matchingVersion;
            Version = version;
        }
    }
}
