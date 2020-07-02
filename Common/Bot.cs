using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Common.Supreme;
using Common.Types;
using Newtonsoft.Json;

namespace Common {
    public enum BotType {
        Undefined,
        Supreme,
        Shopify
    }

    public abstract class Bot : INotifyPropertyChanged {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private string _status = "Idle";

        [Browsable(false), JsonIgnore] protected bool SerializeDebug { get; set; }

        [DisplayName("Type"), Category("Information")]
        public BotType BotType { get; }
        [DisplayName("Billing Profile"), Category("Configuration"), TypeConverter(typeof(ExpandableObjectConverter))]
        public BillingProfile BillingProfile { get; }
        private Task Task { get; set; }
        private CancellationTokenSource CancellationTokenSource { get; set; }
        private HttpMessageHandler HttpMessageHandler { get; set; }
        private SearchProduct _searchProduct;

        private long _completedInMs = -1;

        [ReadOnly(true), JsonIgnore]
        [Category("Information")]
        [DisplayName("Completed In")]
        public long CompletedInMs {
            get => _completedInMs;
            set {
                _completedInMs = value;
                OnPropertyChanged();
            }
        }

        [Category("Configuration")]
        [DisplayName("Product Info")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public SearchProduct SearchProduct {
            get => _searchProduct;
            set { _searchProduct = value; OnPropertyChanged(); }
        }

        [Browsable(false)]
        public List<Tuple<HttpRequestMessage, string>> RequestsList { get; } =
            new List<Tuple<HttpRequestMessage, string>>();

        [Category("Configuration")]
        public string Proxy {
            get {
                if (HttpMessageHandler.GetType() == typeof(Http2WinHttpHandler)) {
                    if (((Http2WinHttpHandler)HttpMessageHandler).Proxy is null) {
                        return "n/a";
                    }

                    var proxyAddress = ((WebProxy)((Http2WinHttpHandler)HttpMessageHandler).Proxy).Address.ToString();
                    var from = proxyAddress.IndexOf("://", StringComparison.Ordinal) + "://".Length;
                    var to = proxyAddress.LastIndexOf("/", StringComparison.Ordinal);
                    return proxyAddress.Substring(from, to - from);
                }

                if (((HttpClientHandler)HttpMessageHandler).Proxy is null) {
                    return "n/a";
                }

                var proxyAddress2 = ((WebProxy)((HttpClientHandler)HttpMessageHandler).Proxy).Address.ToString();
                var from2 = proxyAddress2.IndexOf("://", StringComparison.Ordinal) + "://".Length;
                var to2 = proxyAddress2.LastIndexOf("/", StringComparison.Ordinal);
                return proxyAddress2.Substring(from2, to2 - from2);
            }

            set {
                try {
                    const string proxyPattern = @"\d{1,3}(\.\d{1,3}){3}:\d{1,5}";
                    if (Regex.Match(value, proxyPattern).Success) {
                        WebProxy newProxy;
                        try {
                            newProxy = new WebProxy("http://" + value);
                        }
                        catch (Exception) {
                            return;
                        }

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
                    else {
                        if (HttpMessageHandler.GetType() == typeof(Http2WinHttpHandler)) {
                            HttpMessageHandler = new Http2WinHttpHandler {
                                AutomaticRedirection = true,
                                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.GZip,
                                WindowsProxyUsePolicy = WindowsProxyUsePolicy.DoNotUseProxy
                            };
                        }
                        else {
                            HttpMessageHandler = new HttpClientHandler {
                                AllowAutoRedirect = true,
                                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.GZip,
                                UseProxy = false
                            };
                        }
                    }
                }
                catch (Exception) { }

                OnPropertyChanged();
            }
        }

        [ReadOnly(true), JsonIgnore]
        [Category("Information")]
        public string Status {
            get => _status;
            set {
                _status = value;
                OnPropertyChanged();
            }
        }

        private readonly SortedSet<BotTask> _tasks =
            new SortedSet<BotTask>(Comparer<BotTask>.Create((a, b) => a.Priority() - b.Priority()));

        protected Bot() {
            BotType = BotType.Undefined;
            CancellationTokenSource = new CancellationTokenSource();
        }

        protected Bot(BotType botType, BillingProfile profile, SearchProduct searchProduct) {
            BotType = botType;
            BillingProfile = profile;
            SearchProduct = searchProduct;
            CancellationTokenSource = new CancellationTokenSource();

            try {
                HttpMessageHandler = new Http2WinHttpHandler {
                    AutomaticRedirection = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    WindowsProxyUsePolicy = WindowsProxyUsePolicy.DoNotUseProxy
                };
            }
            catch (PlatformNotSupportedException) {
                HttpMessageHandler = new HttpClientHandler {
                    AllowAutoRedirect = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    UseProxy = false
                };
            }
        }

        public void Execute() {
            if (Task != null && Task.Status.Equals(TaskStatus.Running)) return;
            Task = Task.Run(() => {
                var stopwatch = Stopwatch.StartNew();
                foreach (var task in _tasks.Where(task => task.Validate())) {
                    Status = task.Description();
                    task.Execute();
                }
                stopwatch.Stop();
                CompletedInMs = stopwatch.ElapsedMilliseconds;
                SerializeDebug = true;
                Debug.WriteLine(JsonConvert.SerializeObject(this, Formatting.Indented));
                SerializeDebug = false;
            });
        }

        public void Abort() {
            try {
                CancellationTokenSource.Cancel();
                Task = null;
                CancellationTokenSource = new CancellationTokenSource();
                Status = "Aborted";
                CompletedInMs = -1;
            }
            catch (Exception) { }
        }

        /**
        * Add tasks to our current BotTask.
        */
        public void Append(params BotTask[] tasks) {
            Array.ForEach(tasks, task => _tasks.Add(task));
        }

        public HttpClient GetNewHttpClient() {
            return new HttpClient(HttpMessageHandler, false) {Timeout = TimeSpan.FromSeconds(2)};
        }

        public HttpMessageHandler GetClientHandler() {
            return HttpMessageHandler;
        }

        public CancellationToken GetCancellationToken() {
            return CancellationTokenSource.Token;
        }

        public bool ShouldSerializeRequestsList() {
            return SerializeDebug;
        }
    }
}