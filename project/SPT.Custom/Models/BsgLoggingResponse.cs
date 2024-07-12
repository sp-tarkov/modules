namespace SPT.Custom.Models
{
    public struct LoggingLevelResponse
    {
        public int verbosity { get; set; }
        public bool sendToServer { get; set; }
    }
}