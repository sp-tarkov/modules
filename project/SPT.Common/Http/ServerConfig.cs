namespace SPT.Common.Http
{
    public class ServerConfig
    {
        public string BackendUrl { get; }
        public string MatchingVersion { get; }

        public ServerConfig(string backendUrl, string matchingVersion)
        {
            BackendUrl = backendUrl;
            MatchingVersion = matchingVersion;
        }
    }
}
