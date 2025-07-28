using SPT.Common.Utils;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SPT.Common.Http
{
    // NOTE: Don't dispose this, keep a reference for the lifetime of the
    //       application.
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

            var handler = new HttpClientHandler
            {
                // set cookies in header instead
                UseCookies = false,

                // Bypass Cert validation in the httpServer - discard arguments as we dont use them
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
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

        protected async Task<byte[]> SendAsync(HttpMethod method, string path, byte[] data, bool zipped = true)
        {
            using var request = GetNewRequest(method, path);

            if (data != null)
            {
                // Add payload to request
                if (zipped)
                {
                    data = Zlib.Compress(data, ZlibCompression.Maximum);
                }

                request.Content = new ByteArrayContent(data);
            }

            // Send request
            using var response = await _httpv.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Http response status code: {response.StatusCode}");
            }

            var body = await response.Content.ReadAsByteArrayAsync();

            if (Zlib.IsCompressed(body))
            {
                body = Zlib.Decompress(body);
            }

            if (body == null)
            {
                // Payload doesn't contain data
                var code = response.StatusCode.ToString();
                body = Encoding.UTF8.GetBytes(code);
            }

            return body;
        }

        protected async Task<byte[]> SendWithRetriesAsync(HttpMethod method, string path, byte[] data, bool compress = true)
        {
            // NOTE: <= is intentional. 0 is send, 1/2/3 is retry
            for (var i = 0; i <= _retries; i++)
            {
                try
                {
                    return await SendAsync(method, path, data, compress);
                }
                catch (Exception ex)
                {
                    if (i > _retries)
                    {
                        throw ex;
                    }
                }
            }

            return null;
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns status code as bytes</returns>
        public async Task<byte[]> PutAsync(string path, byte[] data, bool compress = true)
        {
            return await SendWithRetriesAsync(HttpMethod.Post, path, data, compress);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns status code as bytes</returns>
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
