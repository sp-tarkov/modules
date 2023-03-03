namespace Aki.Common.Http
{
    public class ServerConfig
    {
        public string BackendUrl { get; }
        public string Version { get; }

        public ServerConfig(string backendUrl, string version)
        {
            BackendUrl = backendUrl;
            Version = version;
        }
    }
}
