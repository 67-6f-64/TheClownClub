using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common.Types;

namespace Common.Services {
    public class HttpHelper {
        private const string UserAgent = "User-Agent: Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36";

        public static async Task<string> GetString(Uri url, HttpMethod method, CancellationToken cancelToken, int retries = 3) {
            string str;

            HttpMessageHandler handler;
            try {
                handler = new Http2WinHttpHandler {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
            }
            catch (PlatformNotSupportedException) {
                handler = new HttpClientHandler {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
            }

            using (var client = new HttpClient(handler)) {
                str = await GetString(url, method, client, cancelToken, retries);
            }

            return str;
        }

        public static async Task<string> GetString(Uri url, HttpMethod method, HttpClient client, CancellationToken cancelToken, int retries = 3, bool pushAllExceptions = false) {
            string str = null;

            using (var response = await GetResponse(url, method, client, cancelToken, retries, pushAllExceptions)) {
                if (response != null) {
                    str = await response.Content.ReadAsStringAsync();
                }
            }

            return str;
        }

        public static async Task<string> GetString(HttpRequestMessage request, HttpClient client, CancellationToken cancelToken) {
            string str = null;
            
            using (var response = await GetResponse(request, client, cancelToken)) {
                if (response != null) {
                    str = await response.Content.ReadAsStringAsync();
                }
            }

            return str;
        }

        public static async Task<string> GetString(HttpRequestMessage request, CancellationToken cancelToken) {
            string str = null;

            using (var response = await GetResponse(request, cancelToken)) {
                if (response != null) {
                    str = await response.Content.ReadAsStringAsync();
                }
            }

            return str;
        }

        public static async Task<string> GetString(Uri url, HttpMethod method, HttpContent content, CancellationToken cancelToken, int retries = 3) {
            string str;

            using (var request = new HttpRequestMessage() {
                RequestUri = url,
                Method = method,
                Content = content,
                Version = new Version(2, 0)
            }) {
                str = await GetString(request, cancelToken);
            }

            return str;
        }

        public static string GetStringSync(HttpRequestMessage request, HttpClient client, out HttpStatusCode? statusCode, CancellationToken cancelToken) {
            string res = null;

            try {
                var httpTask = GetResponseSync(request, client, cancelToken);
                httpTask.Wait(cancelToken);
                var response = httpTask.Result;

                var htmlTask = response.Content.ReadAsStringAsync();
                htmlTask.Wait(cancelToken);

                res = htmlTask.Result;
                statusCode = response.StatusCode;

                response.Dispose();
            }
            catch (Exception) {
                statusCode = null;
            }

            return res;
        }

        public static async Task<Stream> GetStream(Uri url, HttpMethod method, CancellationToken cancelToken, int retries = 3) {
            Stream stream = null;

            using (var client = new HttpClient()) {
                stream = await GetStream(url, method, client, cancelToken, retries);
            }

            return stream;
        }

        public static async Task<Stream> GetStream(Uri url, HttpMethod method, HttpClient client, CancellationToken cancelToken, int retries = 3) {
            Stream ret = null;

            using (var response = await GetResponse(url, method, client, cancelToken, retries)) {
                if (response == null) return ret;
                using (var stream = await response.Content.ReadAsStreamAsync()) {
                    if (stream is null) return null;

                    ret = new MemoryStream();
                    await stream.CopyToAsync(ret);
                    ret.Position = 0;
                }
            }

            return ret;
        }

        public static async Task<HttpResponseMessage> GetResponse(Uri url, HttpMethod method, CancellationToken cancelToken, int retries = 3) {
            HttpResponseMessage response = null;

            HttpMessageHandler httpClientHandler;
            try {
                httpClientHandler = new Http2WinHttpHandler {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
            }
            catch (PlatformNotSupportedException) {
                httpClientHandler = new HttpClientHandler {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
            }

            using (var client = new HttpClient(httpClientHandler)) {
                response = await GetResponse(url, method, client, cancelToken, retries);
            }

            httpClientHandler.Dispose();

            return response;
        }

        public static async Task<HttpResponseMessage> GetResponse(Uri url, HttpMethod method, HttpClient client, CancellationToken cancelToken, int retries = 3, bool pushAllExceptions = false) {
            HttpResponseMessage response = null;
            uint retriesCount = 0;
            Exception exception = null;

            while (retriesCount <= retries) {
                try {
                    exception = null;

                    var request = new HttpRequestMessage() {
                        RequestUri = url,
                        Method = method
                    };
                    request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);
                    request.Headers.TryAddWithoutValidation("Accept", "*/*");
                    request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");

                    response = await client.SendAsync(request, cancelToken);
                }
                catch (Exception ex) {
                    exception = ex;

                    if (ex.GetType() == typeof(TaskCanceledException)) {
                        throw;
                    }

                    retriesCount++;
                    continue;
                }

                break;
            }

            if (exception != null && pushAllExceptions) {
                throw exception;
            }

            return response;
        }

        public static async Task<HttpResponseMessage> GetResponse(HttpRequestMessage request, HttpClient client, CancellationToken cancelToken) {
            HttpResponseMessage response = null;

            try {
                response = await client.SendAsync(request, cancelToken);
            }
            catch (Exception ex) {
                if (ex.GetType() == typeof(TaskCanceledException)) {
                    throw;
                }
            }

            return response;
        }

        public static async Task<HttpResponseMessage> GetResponseSync(HttpRequestMessage request, HttpClient client, CancellationToken cancelToken) {
            HttpResponseMessage response = null;

            try {
                response = await client.SendAsync(request, cancelToken).ConfigureAwait(false);
            }
            catch (Exception ex) {
                if (ex.GetType() == typeof(TaskCanceledException)) {
                    throw;
                }
            }

            return response;
        }

        public static async Task<HttpResponseMessage> GetResponse(HttpRequestMessage request, CancellationToken cancelToken) {
            HttpResponseMessage response = null;

            using (var client = new HttpClient()) {
                response = await GetResponse(request, client, cancelToken);
            }

            return response;
        }
    }
}