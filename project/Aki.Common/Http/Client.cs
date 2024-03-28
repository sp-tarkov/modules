using System;
using System.IO;
using System.Net.Http;
using Aki.Common.Http;
using Aki.Common.Utils;

namespace Aki.Common.Http
{
    // NOTE: you do not want to dispose this, keep a reference for the lifetime
    //       of the application.
    // NOTE: cannot be made async due to Unity's limitations.
    public class Client : IDisposable
    {
        protected readonly HttpClient _httpv;
        protected readonly string _address;
        protected readonly string _accountId;
        protected readonly int _retries;

        public Client(string address, string accountId, int retries = 3)
        {
            _address = address;
            _accountId = accountId;
            _retries = retries;

            var handler = new HttpClientHandler()
            {
                // force setting cookies in header instead of CookieContainer
                UseCookies = false
            };

            _httpv = new HttpClient(handler);
        }

        private HttpRequestMessage GetNewRequest(HttpMethod method, string path)
        {
            return new HttpRequestMessage()
            {
                Method = method,
                RequestUri = new Uri(_address + path),
                Headers = {
                    { "Cookie", $"PHPSESSID={_accountId}" }
                }
            };
        }

        protected byte[] Send(HttpMethod method, string path, byte[] data, bool compress = true)
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
            var error = new Exception("Internal error");

            // NOTE: <= is intentional, 0 is send, 1,2,3 is retry
            for (var i = 0; i <= _retries; ++i)
            {
                try
                {
                    return Send(HttpMethod.Get, path, null, false);
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            }

            throw error;
        }

        public byte[] Post(string path, byte[] data, bool compressed = true)
        {
            var error = new Exception("Internal error");

            // NOTE: <= is intentional, 0 is send, 1,2,3 is retry
            for (var i = 0; i <= _retries; ++i)
            {
                try
                {
                    return Send(HttpMethod.Post, path, data, compressed);
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            }

            throw error;
        }

        public void Put(string path, byte[] data, bool compressed = true)
        {
            var error = new Exception("Internal error");

            // NOTE: <= is intentional, 0 is send, 1,2,3 is retry
            for (var i = 0; i <= _retries; ++i)
            {
                try
                {
                    Send(HttpMethod.Put, path, data, compressed);
                    return;
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            }

            throw error;
        }

        public void Dispose()
        {
            _httpv.Dispose();
        }
    }
}
