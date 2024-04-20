using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Aki.Common.Http;
using Aki.Common.Utils;

namespace Aki.Common.Http
{
    // NOTE: You do not want to dispose this, keep a reference for the lifetime
    //       of the application.
    // NOTE: There are many places within unity that do not support the Async
    //       methods, use with causion.
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

        protected HttpRequestMessage GetNewRequest(HttpMethod method, string path)
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

        protected async Task<byte[]> SendAsync(HttpMethod method, string path, byte[] data, bool compress = true)
        {
            HttpResponseMessage response = null;

            using (var request = GetNewRequest(method, path))
            {
                if (data != null)
                {
                    if (compress)
                    {
                        data = Zlib.Compress(data, ZlibCompression.Maximum);
                    }

                    // add payload to request
                    request.Content = new ByteArrayContent(data);
                }

                // send request
                response = await _httpv.SendAsync(request);
            }

            if (!response.IsSuccessStatusCode)
            {
                // response error
                throw new Exception($"Code {response.StatusCode}");
            }

            using (var ms = new MemoryStream())
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    // grap response payload
                    await stream.CopyToAsync(ms);
                    var body = ms.ToArray();

                    if (Zlib.IsCompressed(body))
                    {
                        body = Zlib.Decompress(body);
                    }

                    if (body == null)
                    {
                        // payload doesn't contains data
                        var code = response.StatusCode.ToString();
                        body = Encoding.UTF8.GetBytes(code);
                    }

                    return body;
                }
            }
        }

        protected async Task<byte[]> SendWithRetriesAsync(HttpMethod method, string path, byte[] data, bool compress = true)
        {
            var error = new Exception("Internal error");

            // NOTE: <= is intentional. 0 is send, 1/2/3 is retry
            for (var i = 0; i <= _retries; ++i)
            {
                try
                {
                    return await SendAsync(method, path, data, compress);
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            }

            throw error;
        }

        public async Task<byte[]> GetAsync(string path)
        {
            return await SendWithRetriesAsync(HttpMethod.Get, path, null);
        }

        public byte[] Get(string path)
        {
            return Task.Run(() => GetAsync(path)).Result;
        }

        public async Task<byte[]> PostAsync(string path, byte[] data, bool compress = true)
        {
            return await SendWithRetriesAsync(HttpMethod.Post, path, data, compress);
        }

        public byte[] Post(string path, byte[] data, bool compress = true)
        {
            return Task.Run(() => PostAsync(path, data, compress)).Result;
        }

        // NOTE: returns status code as bytes
        public async Task<byte[]> PutAsync(string path, byte[] data, bool compress = true)
        {
            return await SendWithRetriesAsync(HttpMethod.Post, path, data, compress);
        }

        // NOTE: returns status code as bytes
        public byte[] Put(string path, byte[] data, bool compress = true)
        {
            return Task.Run(() => PutAsync(path, data, compress)).Result;
        }

        public void Dispose()
        {
            _httpv.Dispose();
        }
    }
}
