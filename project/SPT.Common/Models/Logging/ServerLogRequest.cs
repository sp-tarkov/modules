using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;

namespace SPT.Common.Models.Logging
{
    public class ServerLogRequest
    {
        public string Source { get; set; }
        public EServerLogLevel Level { get; set; }
        public string Message { get; set; }
        public EServerLogTextColor Color { get; set; }
        public EServerLogBackgroundColor BackgroundColor { get; set; }
    }
}
