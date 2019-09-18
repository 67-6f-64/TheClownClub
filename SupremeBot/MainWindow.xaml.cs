using HtmlAgilityPack;
using SupremeBot.Services;
using SupremeBot.Templates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Size = SupremeBot.Templates.Size;
using Style = SupremeBot.Templates.Style;

namespace SupremeBot {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        CancellationTokenSource tokenSource = new CancellationTokenSource();

        public MainWindow() {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            List<long> theTimes = new List<long>();
            var watch = Stopwatch.StartNew();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancelToken = cancellationTokenSource.Token;

            var clientHandler = new HttpClientHandler() {
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseProxy = false
            };

            string mobileStockJson = null;
            HttpStatusCode? mobileStockStatus = null;

            using (HttpRequestMessage request = new HttpRequestMessage() {
                RequestUri = new Uri($"https://www.supremenewyork.com/mobile_stock.json"),
                Method = HttpMethod.Get,
            }) {
                using (HttpClient client = new HttpClient(clientHandler, false)) {
                    request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                    request.Headers.TryAddWithoutValidation("Accept", "*/*");
                    request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                    request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                    mobileStockJson = HttpHelper.GetStringSync(request, client, out mobileStockStatus, cancelToken);
                }
            }

            if (mobileStockJson != null) {
                MobileStock mobileStock = MobileStock.FromJson(mobileStockJson);
                MobileStockProduct foundProduct = null;
                Parallel.ForEach(mobileStock.ProductsAndCategories["new"], (product, state) => {
                    if (product.Name.IndexOf("mug shot crew", StringComparison.OrdinalIgnoreCase) >= 0) {
                        foundProduct = product;
                        state.Break();
                    }
                });
                theTimes.Add(watch.ElapsedMilliseconds);
                if (foundProduct != null) {
                    string productJson = null;
                    HttpStatusCode? productStatus = null;
                    using (HttpRequestMessage request = new HttpRequestMessage() {
                        RequestUri = new Uri($"https://www.supremenewyork.com/shop/{foundProduct.Id}.json"),
                        Method = HttpMethod.Get,
                    }) {
                        using (HttpClient client = new HttpClient(clientHandler, false)) {
                            request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                            request.Headers.TryAddWithoutValidation("Accept", "*/*");
                            request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                            request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                            productJson = HttpHelper.GetStringSync(request, client, out productStatus, cancelToken);
                        }
                    }

                    if (productJson != null) {
                        Product product = Product.FromJson(productJson);
                        Style foundStyle = null;
                        Parallel.ForEach(product.Styles, (style, state) => {
                            if (style.Name.IndexOf("burnt", StringComparison.OrdinalIgnoreCase) >= 0) {
                                foundStyle = style;
                                state.Break();
                            }
                        });
                        theTimes.Add(watch.ElapsedMilliseconds);
                        if (foundStyle != null) {
                            Size foundSize = null;
                            Parallel.ForEach(foundStyle.Sizes, (size, state) => {
                                if (size.StockLevel.Equals(1)) {
                                    foundSize = size;
                                    state.Break();
                                }
                            });
                            theTimes.Add(watch.ElapsedMilliseconds);
                            if (foundSize != null) {
                                string atcResponse = null;
                                HttpStatusCode? atcStatusCode = null;
                                using (HttpRequestMessage addToCartRequest = new HttpRequestMessage() {
                                    RequestUri = new Uri($"https://www.supremenewyork.com/shop/{foundProduct.Id}/add.json"),
                                    Method = HttpMethod.Post
                                }) {
                                    using (HttpClient client = new HttpClient(clientHandler, false)) {
                                        addToCartRequest.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                                        addToCartRequest.Headers.TryAddWithoutValidation("Accept", "*/*");
                                        addToCartRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                                        addToCartRequest.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
                                        addToCartRequest.Headers.TryAddWithoutValidation("Refer", "http://www.supremenewyork.com/mobile");
                                        addToCartRequest.Headers.TryAddWithoutValidation("Connection", "keep-alive");
                                        addToCartRequest.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                                        {
                                            new KeyValuePair<string, string>("st", foundStyle.Id.ToString()),
                                            new KeyValuePair<string, string>("s", foundSize.Id.ToString()),
                                            new KeyValuePair<string, string>("qty", "1")
                                        });

                                        atcResponse = HttpHelper.GetStringSync(addToCartRequest, client, out atcStatusCode, cancelToken);
                                    }
                                }

                                if (atcResponse != null) {
                                    theTimes.Add(watch.ElapsedMilliseconds);
                                    string checkoutHtml = null;
                                    HttpStatusCode? checkoutStatus = null;

                                    using (HttpRequestMessage request = new HttpRequestMessage() {
                                        RequestUri = new Uri($"https://www.supremenewyork.com/checkout"),
                                        Method = HttpMethod.Get,
                                    }) {
                                        using (HttpClient client = new HttpClient(clientHandler, false)) {
                                            request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                                            request.Headers.TryAddWithoutValidation("Accept", "*/*");
                                            request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                                            request.Headers.TryAddWithoutValidation("Connection", "keep-alive");

                                            checkoutHtml = HttpHelper.GetStringSync(request, client, out checkoutStatus, cancelToken);
                                        }
                                    }

                                    if (checkoutHtml != null) {
                                        var doc = new HtmlDocument();
                                        doc.LoadHtml(checkoutHtml);
                                        var csrfToken = doc.DocumentNode.SelectSingleNode("//meta[@name=\"csrf-token\"]").GetAttributeValue("content", "");
                                        var form = doc.DocumentNode.Descendants("form").FirstOrDefault();
                                        List<KeyValuePair<string, string>> formContentList = new List<KeyValuePair<string, string>>();
                                        var inputLoop = Task.Factory.StartNew(() => {
                                            Parallel.ForEach(form.Descendants("input"), node => {
                                                string nodeName = node.GetAttributeValue("name", "");
                                                if (node.GetAttributeValue("type", "").Equals("hidden")) {
                                                    formContentList.Add(new KeyValuePair<string, string>(nodeName, node.GetAttributeValue("value", "")));
                                                }
                                                else {
                                                    if (nodeName.Contains("name")) {
                                                        formContentList.Add(new KeyValuePair<string, string>(nodeName, "Trey Phillips"));
                                                    }
                                                    else if (nodeName.Contains("email")) {
                                                        formContentList.Add(new KeyValuePair<string, string>(nodeName, "trey.phillips2014.1@gmail.com"));
                                                    }
                                                    else if (nodeName.Contains("tel")) {
                                                        formContentList.Add(new KeyValuePair<string, string>(nodeName, "336-999-4585"));
                                                    }
                                                    else if (nodeName.Contains("address_2")) {
                                                        formContentList.Add(new KeyValuePair<string, string>(nodeName, ""));
                                                    }
                                                    else if (nodeName.Contains("billing_address")) {
                                                        formContentList.Add(new KeyValuePair<string, string>(nodeName, "3610 Swicegood Rd"));
                                                    }
                                                    else if (nodeName.Contains("zip")) {
                                                        formContentList.Add(new KeyValuePair<string, string>(nodeName, "27299"));
                                                    }
                                                    else if (nodeName.Contains("city")) {
                                                        formContentList.Add(new KeyValuePair<string, string>(nodeName, "Linwood"));
                                                    }
                                                    else if (nodeName.Contains("state")) {
                                                        formContentList.Add(new KeyValuePair<string, string>(nodeName, "NC"));
                                                    }
                                                    else if (nodeName.Contains("country")) {
                                                        formContentList.Add(new KeyValuePair<string, string>(nodeName, "USA"));
                                                    }
                                                    else if (nodeName.Contains("month")) {
                                                        formContentList.Add(new KeyValuePair<string, string>(nodeName, "09"));
                                                    }
                                                    else if (nodeName.Contains("year")) {
                                                        formContentList.Add(new KeyValuePair<string, string>(nodeName, "2020"));
                                                    }
                                                    else if (nodeName.Contains("vvv")) {
                                                        formContentList.Add(new KeyValuePair<string, string>(nodeName, "322"));
                                                    }
                                                    else if (nodeName.Contains("terms")) {
                                                        formContentList.Add(new KeyValuePair<string, string>(nodeName, "1"));
                                                    }
                                                    else if (node.GetAttributeValue("placeholder", "").Contains("number")) {
                                                        formContentList.Add(new KeyValuePair<string, string>(nodeName, "9281767209128752"));
                                                    }
                                                }
                                            });
                                        });

                                        var selectLoop = Task.Factory.StartNew(() => {
                                            Parallel.ForEach(form.Descendants("select"), node => {
                                                string nodeName = node.GetAttributeValue("name", "");
                                                if (node.GetAttributeValue("type", "").Equals("hidden")) {
                                                    formContentList.Add(new KeyValuePair<string, string>(nodeName, node.GetAttributeValue("value", "")));
                                                }
                                                else {
                                                    if (nodeName.Contains("state")) {
                                                        formContentList.Add(new KeyValuePair<string, string>(nodeName, "NC"));
                                                    }
                                                    else if (nodeName.Contains("country")) {
                                                        formContentList.Add(new KeyValuePair<string, string>(nodeName, "USA"));
                                                    }
                                                    else if (nodeName.Contains("month")) {
                                                        formContentList.Add(new KeyValuePair<string, string>(nodeName, "09"));
                                                    }
                                                    else if (nodeName.Contains("year")) {
                                                        formContentList.Add(new KeyValuePair<string, string>(nodeName, "2020"));
                                                    }
                                                }
                                            });
                                        });

                                        Task.WaitAll(inputLoop, selectLoop);

                                        FormUrlEncodedContent formUrlEncodedContent = new FormUrlEncodedContent(formContentList);
                                        theTimes.Add(watch.ElapsedMilliseconds);
                                        string checkoutResponse = null;
                                        HttpStatusCode? checkoutRequestStatus = null;

                                        using (HttpRequestMessage request = new HttpRequestMessage() {
                                            RequestUri = new Uri($"https://www.supremenewyork.com/checkout.json"),
                                            Method = HttpMethod.Post,
                                        }) {
                                            using (HttpClient client = new HttpClient(clientHandler, false)) {
                                                request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                                                request.Headers.TryAddWithoutValidation("Accept", "*/*");
                                                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                                                request.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
                                                request.Headers.TryAddWithoutValidation("Refer", "https://www.supremenewyork.com/checkout");
                                                request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                                                request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.8,en-GB;q=0.6");
                                                request.Headers.Add("X-CSRF-Token", csrfToken);
                                                request.Content = formUrlEncodedContent;

                                                checkoutResponse = HttpHelper.GetStringSync(request, client, out checkoutRequestStatus, cancelToken);
                                            }
                                        }

                                        if (checkoutResponse != null) {
                                            theTimes.Add(watch.ElapsedMilliseconds);
                                            watch.Stop();
                                            string thingy = "";
                                            foreach (var time in theTimes) {
                                                thingy += "\n" + time.ToString();
                                            }
                                            MessageBox.Show(thingy);
                                            //MessageBox.Show(watch.ElapsedMilliseconds.ToString());
                                            MessageBox.Show(checkoutResponse);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            
        }
    }
}
