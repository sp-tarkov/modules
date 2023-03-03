using System.Collections.Generic;
using System.Linq;

namespace Aki.Common.Http
{
    public static class WebConstants
    {
        /// <summary>
        /// HTML GET method.
        /// </summary>
        public const string Get = "GET";

        /// <summary>
        /// HTML HEAD method.
        /// </summary>
        public const string Head = "HEAD";

        /// <summary>
        /// HTML POST method.
        /// </summary>
        public const string Post = "POST";

        /// <summary>
        /// HTML PUT method.
        /// </summary>
        public const string Put = "PUT";

        /// <summary>
        /// HTML DELETE method.
        /// </summary>
        public const string Delete = "DELETE";

        /// <summary>
        /// HTML CONNECT method.
        /// </summary>
        public const string Connect = "CONNECT";

        /// <summary>
        /// HTML OPTIONS method.
        /// </summary>
        public const string Options = "OPTIONS";

        /// <summary>
        /// HTML TRACE method.
        /// </summary>
        public const string Trace = "TRACE";

        /// <summary>
        /// HTML MIME types.
        /// </summary>
        public static Dictionary<string, string> Mime { get; private set; }

        static WebConstants()
        {
            Mime = new Dictionary<string, string>()
            {
                { ".bin", "application/octet-stream" },
                { ".txt", "text/plain" },
                { ".htm", "text/html" },
                { ".html", "text/html" },
                { ".css", "text/css" },
                { ".js", "text/javascript" },
                { ".jpeg", "image/jpeg" },
                { ".jpg", "image/jpeg" },
                { ".png", "image/png" },
                { ".ico", "image/vnd.microsoft.icon" },
                { ".json", "application/json" }
            };
        }

        /// <summary>
        /// Is HTML method valid?
        /// </summary>
        public static bool IsValidMethod(string method)
        {
            return method == Get
                || method == Head
                || method == Post
                || method == Put
                || method == Delete
                || method == Connect
                || method == Options
                || method == Trace;
        }

        /// <summary>
        /// Is MIME type valid?
        /// </summary>
		public static bool IsValidMime(string mime)
        {
            return Mime.Any(x => x.Value == mime);
        }
    }
}
