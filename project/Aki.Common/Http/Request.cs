#region DEPRECATED, REMOVE IN 3.8.1

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Aki.Common.Utils;

namespace Aki.Common.Http
{
    public class Request
    {
        [Obsolete("Request.Send() is deprecated, please use Aki.Common.Http.Client instead.")]
        public byte[] Send(string url, string method, byte[] data = null, bool compress = true, string mime = null, Dictionary<string, string> headers = null)
        {
            if (!WebConstants.IsValidMethod(method))
            {
                throw new ArgumentException("request method is invalid");
            }

            Uri uri = new Uri(url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

            if (uri.Scheme == "https")
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                request.ServerCertificateValidationCallback = delegate { return true; };
            }

            request.Timeout = 15000;
            request.Method = method;
            request.Headers.Add("Accept-Encoding", "deflate");

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> item in headers)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            }

            if (method != WebConstants.Get && method != WebConstants.Head && data != null)
            {
                byte[] body = (compress) ? Zlib.Compress(data, ZlibCompression.Maximum) : data;

                request.ContentType = WebConstants.IsValidMime(mime) ? mime : "application/octet-stream";
                request.ContentLength = body.Length;

                if (compress)
                {
                    request.Headers.Add("Content-Encoding", "deflate");
                }

                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(body, 0, body.Length);
                }
            }

            using (WebResponse response = request.GetResponse())
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    response.GetResponseStream().CopyTo(ms);
                    byte[] body = ms.ToArray();

                    if (body.Length == 0)
                    {
                        return null;
                    }

                    if (Zlib.IsCompressed(body))
                    {
                        return Zlib.Decompress(body);
                    }

                    return body;
                }
            }
        }
    }
}

#endregion