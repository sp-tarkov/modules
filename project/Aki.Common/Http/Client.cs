using System;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Aki.Common.Utils;

namespace Aki.Common.Http
{
    public class Client : IDisposable
    {
        private readonly HttpClient _httpv;
        private readonly string _accountId;
        private readonly string _address;

        public Client(string address, string accountId)
        {
            _accountId = accountId;
            _address = address;

            // setup certificate handler
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback += IsCertificateValid;

            // setup http client
            _httpv = new HttpClient(handler);
        }

        private bool IsCertificateValid(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // todo: proper certificate validation
            return true;
        }

        private HttpRequestMessage GetNewRequest(HttpMethod method, string path)
        {
            return new HttpRequestMessage()
            {
                Method = method,
                RequestUri = new Uri(_address + path),
                Headers = {
                    // NOTE: might need option to set MIME type
                    { "Cookie", $"PHPSESSID={_accountId}" },
                    { "SessionId", _accountId }
                }
            };
        }

        private byte[] Send(HttpMethod method, string path, byte[] data, bool compress = true)
        {
            HttpResponseMessage response = null;

            using (var request = GetNewRequest(method, path))
            {
                if (data != null)
                {
                    // if there is data, convert to payload
                    byte[] payload = (compress)
                        ? Zlib.Compress(data, ZlibCompression.Maximum)
                        : data;

                    // add payload to request
                    request.Content = new ByteArrayContent(payload);
                }

                // send request
                response = _httpv.SendAsync(request).Result;
            }

            if (!response.IsSuccessStatusCode)
            {
                // response error
                throw new Exception($"Code {response.StatusCode}");
            }

            using (var ms = new MemoryStream())
            {
                using (var stream = response.Content.ReadAsStreamAsync().Result)
                {
                    // grap response payload
                    stream.CopyTo(ms);
                    var bytes = ms.ToArray();

                    if (bytes != null)
                    {
                        // payload contains data
                        return Zlib.IsCompressed(bytes)
                            ? Zlib.Decompress(bytes)
                            : bytes;
                    }
                }
            }

            // response returned no data
            return null;
        }

        public byte[] Get(string path)
        {
            return Send(HttpMethod.Get, path, null, false);
        }

        public byte[] Post(string path, byte[] data, bool compressed = true)
        {
            return Send(HttpMethod.Post, path, data, compressed);
        }

        public void Put(string path, byte[] data, bool compressed = true)
        {
            Send(HttpMethod.Put, path, data, compressed);
        }

        public void Dispose()
        {
            _httpv.Dispose();
        }
    }
}
