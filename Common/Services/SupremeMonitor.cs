using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common.Supreme;
using Common.Types;

namespace Common.Services {
    public static class SupremeMonitor {
        private static readonly Dictionary<string, string> cyrillicDictionary = new Dictionary<string, string> {
            { "а", "a" },
            { "б", "b" },
            { "в", "v" },
            { "г", "g" },
            { "д", "d" },
            { "е", "e" },
            { "ё", "yo" },
            { "ж", "zh" },
            { "з", "z" },
            { "и", "i" },
            { "й", "j" },
            { "к", "k" },
            { "л", "l" },
            { "м", "m" },
            { "н", "n" },
            { "о", "o" },
            { "п", "p" },
            { "р", "r" },
            { "с", "s" },
            { "т", "t" },
            { "у", "u" },
            { "ф", "f" },
            { "х", "h" },
            { "ц", "c" },
            { "ч", "ch" },
            { "ш", "sh" },
            { "щ", "sch" },
            { "ъ", "j" },
            { "ы", "i" },
            { "ь", "j" },
            { "э", "e" },
            { "ю", "yu" },
            { "я", "ya" },
            { "А", "A" },
            { "Б", "B" },
            { "В", "V" },
            { "Г", "G" },
            { "Д", "D" },
            { "Е", "E" },
            { "Ё", "Yo" },
            { "Ж", "Zh" },
            { "З", "Z" },
            { "И", "I" },
            { "Й", "J" },
            { "К", "K" },
            { "Л", "L" },
            { "М", "M" },
            { "Н", "N" },
            { "О", "O" },
            { "П", "P" },
            { "Р", "R" },
            { "С", "S" },
            { "Т", "T" },
            { "У", "U" },
            { "Ф", "F" },
            { "Х", "H" },
            { "Ц", "C" },
            { "Ч", "Ch" },
            { "Ш", "Sh" },
            { "Щ", "Sch" },
            { "Ъ", "J" },
            { "Ы", "I" },
            { "Ь", "J" },
            { "Э", "E" },
            { "Ю", "Yu" },
            { "Я", "Ya" }
        };

        public static TimeSpan RefreshInterval = TimeSpan.FromSeconds(2);
        public static MobileStock MobileStock;

        private static HttpMessageHandler HttpMessageHandler;

        static SupremeMonitor() {
            try {
                HttpMessageHandler = new Http2WinHttpHandler {
                    AutomaticRedirection = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.GZip,
                    WindowsProxyUsePolicy = WindowsProxyUsePolicy.DoNotUseProxy
                };
            }
            catch (PlatformNotSupportedException) {
                HttpMessageHandler = new HttpClientHandler {
                    AllowAutoRedirect = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.GZip,
                    UseProxy = false
                };
            }
        }

        private static Task _task;
        private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public static void Start() {
            if (_task != null) Stop();

            _task = Task.Run(async () => {
                do {
                    string mobileStockJson;
                    do {
                        await Task.Delay(RefreshInterval, _cancellationTokenSource.Token);
                        
                        using (var request = new HttpRequestMessage {
                            RequestUri = new Uri("https://www.supremenewyork.com/shop.json"),
                            Method = HttpMethod.Get,
                        }) {
                            using (var client = new HttpClient(HttpMessageHandler, false)
                                {Timeout = TimeSpan.FromSeconds(2)}) {
                                request.Headers.TryAddWithoutValidation("User-Agent",
                                    "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                                request.Headers.TryAddWithoutValidation("Accept", "*/*");
                                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                                request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                                
                                mobileStockJson = HttpHelper.GetStringSync(request, client, out _, _cancellationTokenSource.Token);
                            }
                        }
                    } while (string.IsNullOrEmpty(mobileStockJson) &&
                             !_cancellationTokenSource.IsCancellationRequested);
                    
                    if (_cancellationTokenSource.IsCancellationRequested) return;

                    mobileStockJson = cyrillicDictionary.Aggregate(mobileStockJson,
                        (current, pair) => current.Replace(pair.Key, pair.Value));
                    
                    try {
                        MobileStock = MobileStock.FromJson(mobileStockJson);
                    }
                    catch (Exception ex) {
                        Debug.WriteLine(ex.Message);
                    }
                } while (!_cancellationTokenSource.IsCancellationRequested);
            });
        }

        public static void Stop() {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public static void SetProxy(string proxy = null) {
            WebProxy newProxy;
            try {
                newProxy = new WebProxy("http://" + proxy);
            }
            catch (Exception) { return; }

            if (HttpMessageHandler.GetType() == typeof(Http2WinHttpHandler)) {
                HttpMessageHandler = new Http2WinHttpHandler {
                    AutomaticRedirection = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.GZip,
                    WindowsProxyUsePolicy = WindowsProxyUsePolicy.UseCustomProxy,
                    Proxy = newProxy
                };
            }
            else {
                HttpMessageHandler = new HttpClientHandler {
                    AllowAutoRedirect = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.GZip,
                    Proxy = newProxy,
                    UseProxy = true
                };
            }
        } 
    }
}
