namespace SPT.Common.Http;

public class ServerConfig(string backendUrl, string matchingVersion, string version)
{
    public string BackendUrl { get; } = backendUrl;
    public string MatchingVersion { get; } = matchingVersion;
    public string Version { get; } = version;
}
