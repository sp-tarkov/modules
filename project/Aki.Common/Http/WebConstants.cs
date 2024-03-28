#region DEPRECATED, REMOVE IN 3.8.1

using System;
using System.Collections.Generic;
using System.Linq;

namespace Aki.Common.Http
{
    public static class WebConstants
    {
        [Obsolete("Get is deprecated, please use HttpMethod.Get instead.")]
        public const string Get = "GET";

        [Obsolete("Head is deprecated, please use HttpMethod.Head instead.")]
        public const string Head = "HEAD";

        [Obsolete("Post is deprecated, please use HttpMethod.Post instead.")]
        public const string Post = "POST";

        [Obsolete("Put is deprecated, please use HttpMethod.Put instead.")]
        public const string Put = "PUT";

        [Obsolete("Delete is deprecated, please use HttpMethod.Delete instead.")]
        public const string Delete = "DELETE";

        [Obsolete("Connect is deprecated, please use HttpMethod.Connect instead.")]
        public const string Connect = "CONNECT";

        [Obsolete("Options is deprecated, please use HttpMethod.Options instead.")]
        public const string Options = "OPTIONS";

        [Obsolete("Trace is deprecated, please use HttpMethod.Trace instead.")]
        public const string Trace = "TRACE";

        [Obsolete("Mime is deprecated, there is sadly no replacement.")]
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

        [Obsolete("IsValidMethod is deprecated, please check against HttpMethod entries instead.")]
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

        [Obsolete("isValidMime is deprecated, there is sadly no replacement.")]
		public static bool IsValidMime(string mime)
        {
            return Mime.Any(x => x.Value == mime);
        }
    }
}

#endregion