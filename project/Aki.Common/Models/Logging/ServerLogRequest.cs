using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;

namespace Aki.Common.Models.Logging
{
    public class ServerLogRequest
    {
        public string Source { get; set; }
        public EServerLogLevel Level { get; set; }
        public string Message { get; set; }
        public ServerLogTextColor Color { get; set; }
        public ServerLogBackgroundColor BackgroundColor { get; set; }
    }
}
