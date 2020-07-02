using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Common.Services;
using Common.Shopify;
using Common.Shopify.Shopify;
using Discord;
using Discord.WebSocket;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TestApp {
    static class Program {
        private static DiscordSocketClient _client;
        private static int RandomNumber(int min, int max) {
            var random = new Random();
            return random.Next(min, max);
        }

        private static readonly HttpMessageHandler HttpMessageHandler = new HttpClientHandler {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.GZip,
            UseProxy = false
        };

        private static HttpClient GetNewHttpClient() {
            return new HttpClient(HttpMessageHandler, false) { Timeout = TimeSpan.FromSeconds(2) };
        }

        private static bool _isReady = false;
        private const string InviteRegex = @"discord\.gift\/(.{16})";
        private const string AuthToken = "mfa.dG-to0xXJUAqo004F76JnPEe5Aq17bh5CP0pGe6AaqK_l0meDDshlgrz-3VC8e6FeSvwkMfvuXWh8Mz0OcD8";

        private static void RedeemGift(ulong channelId, string inviteCode) {
            using var request = new HttpRequestMessage {
                RequestUri = new Uri($"https://discordapp.com/api/v6/entitlements/gift-codes/{inviteCode}/redeem"),
                Method = HttpMethod.Post,
            };
            using var client = GetNewHttpClient();

            request.Headers.TryAddWithoutValidation("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) discord/0.0.305 Chrome/69.0.3497.128 Electron/4.0.8 Safari/537.36");
            request.Headers.TryAddWithoutValidation("Accept", "*/*");
            request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
            request.Headers.TryAddWithoutValidation("Authorization", $"{AuthToken}");

            request.Content = new StringContent($"{{\"channel_id\":\"{channelId}\"}}", Encoding.UTF8, "application/json");

            var inviteRedeemResponse = HttpHelper.GetStringSync(request, client, out _, CancellationToken.None);
            Console.WriteLine(inviteRedeemResponse);
        }

        private static void Main(string[] args) {
            _client = new DiscordSocketClient(new DiscordSocketConfig {
                MessageCacheSize = 100,
                LogLevel = LogSeverity.Info
            });

            _client.Ready += () => {
                if (_isReady) return Task.FromResult(0);
                Console.WriteLine($"Logged into {_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator}");
                _isReady = true;
                return Task.FromResult(0);
            };

            _client.MessageReceived += msg => {
                if (msg.Author.Id.Equals(_client.CurrentUser.Id)) return Task.FromResult(0);
                try {
                    var match = Regex.Match(msg.Content, InviteRegex);
                    if (!match.Success) return Task.FromResult(0);
                    Console.WriteLine(
                        $"Found Invite // ID: {match.Groups[1].Value} // Channel ID: {msg.Channel.Id} // From: {msg.Author.Username}#{msg.Author.Discriminator}");
                    RedeemGift(msg.Channel.Id, match.Groups[1].Value);
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
                return Task.FromResult(0);
            };

            Task.Run(async () => {
                await _client.LoginAsync(0, AuthToken);
                await _client.StartAsync();
            });
            Console.ReadLine();
        }

        private static void ShopifyMain(string[] args) {
            ShopifyProducts products;
            using (var request = new HttpRequestMessage {
                RequestUri = new Uri($"https://bdgastore.com/products.json?limit={RandomNumber(1000000, int.MaxValue)}"),
                Method = HttpMethod.Get,
            }) {
                using var client = GetNewHttpClient();

                request.Headers.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                request.Headers.TryAddWithoutValidation("Accept", "*/*");
                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                var productsJson = HttpHelper.GetStringSync(request, client, out _, CancellationToken.None);
                products = ShopifyProducts.FromJson(productsJson);
            }

            var shopifyProduct = products.ProductsList.First();
            using (var request = new HttpRequestMessage {
                RequestUri = new Uri($"https://bdgastore.com/cart/add.js?quantity=1&id={shopifyProduct.Variants.First().Id}"),
                Method = HttpMethod.Get,
            }) {
                using var client = GetNewHttpClient();

                request.Headers.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                request.Headers.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");

                var atcResponse = HttpHelper.GetStringSync(request, client, out _, CancellationToken.None);
            }

            string checkoutUrl, checkoutAction, checkoutGateway;

            using (var request = new HttpRequestMessage {
                RequestUri = new Uri($"https://bdgastore.com/checkout.json"),
                Method = HttpMethod.Get,
            }) {
                using var client = GetNewHttpClient();

                request.Headers.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                request.Headers.TryAddWithoutValidation("Accept", "*/*");
                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");

                var beginCheckoutResponse = HttpHelper.GetResponse(request, client, CancellationToken.None).Result;
                var checkoutDoc = new HtmlDocument();
                checkoutDoc.LoadHtml(beginCheckoutResponse.Content.ReadAsStringAsync().Result);
                checkoutUrl = beginCheckoutResponse.RequestMessage.RequestUri.ToString();
                checkoutAction = checkoutDoc.DocumentNode.SelectSingleNode("//form[@class=\"edit_checkout\"]").GetAttributeValue("action", "");
            }

            var keywordsList = new List<string> {
                "air",
                "force",
                "mtaa"
            };

            var colorKeywords = new List<string> {
                "blue"
            };

            var sizeKeyword = "10.5";

            ShopifyProduct foundProduct = null;
            Parallel.ForEach(products.ProductsList,
                (product, state) => {
                    var isValid = !keywordsList.Any(keyword =>
                        keyword.StartsWith("-")
                            ? !product.Title.IndexOf(keyword.Substring(1, keyword.Length - 1),
                                StringComparison.CurrentCultureIgnoreCase).Equals(-1)
                            : product.Title.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase).Equals(-1));
                    if (!isValid) return;
                    foundProduct = product;
                    state.Stop();
                });

            Console.WriteLine(foundProduct != null ? foundProduct.Title : "not found");

            var foundVariant = foundProduct.Variants.FirstOrDefault(potentialVariant => {
                return colorKeywords.Any(keyword =>
                           keyword.StartsWith("-")
                               ? !string.IsNullOrEmpty(potentialVariant.Option1) &&
                                 !potentialVariant.Option1.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase)
                                     .Equals(-1)
                                 || !string.IsNullOrEmpty(potentialVariant.Option2) &&
                                 !potentialVariant.Option2.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase)
                                     .Equals(-1)
                                 || !string.IsNullOrEmpty(potentialVariant.Option3) &&
                                 !potentialVariant.Option3.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase)
                                     .Equals(-1)
                               : !string.IsNullOrEmpty(potentialVariant.Option1) &&
                                 potentialVariant.Option1.IndexOf(keyword.Substring(1, keyword.Length - 1),
                                         StringComparison.CurrentCultureIgnoreCase)
                                     .Equals(-1)
                                 || !string.IsNullOrEmpty(potentialVariant.Option2) &&
                                 potentialVariant.Option2.IndexOf(keyword.Substring(1, keyword.Length - 1),
                                         StringComparison.CurrentCultureIgnoreCase)
                                     .Equals(-1)
                                 || !string.IsNullOrEmpty(potentialVariant.Option3) &&
                                 potentialVariant.Option3.IndexOf(keyword.Substring(1, keyword.Length - 1),
                                         StringComparison.CurrentCultureIgnoreCase)
                                     .Equals(-1)
                       )
                       && !string.IsNullOrEmpty(potentialVariant.Option1) &&
                       potentialVariant.Option1.Equals(sizeKeyword)
                       || !string.IsNullOrEmpty(potentialVariant.Option2) &&
                       potentialVariant.Option2.Equals(sizeKeyword)
                       || !string.IsNullOrEmpty(potentialVariant.Option3) &&
                       potentialVariant.Option3.Equals(sizeKeyword);
            });

            Console.WriteLine($"{foundVariant.Title}");

            using (var request = new HttpRequestMessage {
                RequestUri = new Uri($"https://bdgastore.com/cart/update.json"),
                Method = HttpMethod.Post,
            }) {
                using var client = GetNewHttpClient();

                request.Headers.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                request.Headers.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");

                var values = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>($"updates[{foundVariant.Id}]", "1"),
                    new KeyValuePair<string, string>($"updates[{shopifyProduct.Variants.First().Id}]", "0")
                };

                var formContent = new FormUrlEncodedContent(values);
                request.Content = formContent;

                var atcResponse = HttpHelper.GetStringSync(request, client, out _, CancellationToken.None);
            }

            //using (var request = new HttpRequestMessage {
            //    RequestUri = new Uri($"https://bdgastore.com/cart/add.js?quantity=1&id={foundVariant.Id}"),
            //    Method = HttpMethod.Get,
            //}) {
            //    using var client = GetNewHttpClient();
            //
            //    request.Headers.TryAddWithoutValidation("User-Agent",
            //        "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
            //    request.Headers.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
            //    request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
            //
            //    var atcResponse = HttpHelper.GetStringSync(request, client, out _, CancellationToken.None);
            //    Console.WriteLine(atcResponse);
            //}

            var checkoutFormContentList = new Queue<KeyValuePair<string, string>>();
            FormUrlEncodedContent checkoutFormContent;

            var doc = new HtmlDocument();
            using (var request = new HttpRequestMessage {
                RequestUri = new Uri($"https://bdgastore.com/checkout.json"),
                Method = HttpMethod.Get,
            }) {
                using var client = GetNewHttpClient();

                request.Headers.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                request.Headers.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");

                var beginCheckoutResponse = HttpHelper.GetResponse(request, client, CancellationToken.None).Result;
                checkoutUrl = beginCheckoutResponse.RequestMessage.RequestUri.ToString(); // TODO: check for queue or oos (stock_problems)
                doc.LoadHtml(beginCheckoutResponse.Content.ReadAsStringAsync().Result);
            }

            var form = doc.DocumentNode.SelectSingleNode("//form[@class=\"edit_checkout\"]");
            foreach (var node in form.Descendants("input")) {
                if (node.GetAttributeValue("data-honeypot", "").Equals("true")) continue;
                var nodeName = node.GetAttributeValue("name", "");
                if (node.GetAttributeValue("type", "").Equals("hidden")) {
                    checkoutFormContentList.Enqueue(
                        new KeyValuePair<string, string>(nodeName, node.GetAttributeValue("value", "")));
                }
                else {
                    if (nodeName.Contains("email")) {
                        checkoutFormContentList.Enqueue(
                            new KeyValuePair<string, string>(nodeName, "realyrealyrealy0@gmail.com"));
                    }
                    else if (nodeName.Contains("first_name")) {
                        checkoutFormContentList.Enqueue(
                            new KeyValuePair<string, string>(nodeName, "Trey"));
                    }
                    else if (nodeName.Contains("last_name")) {
                        checkoutFormContentList.Enqueue(
                            new KeyValuePair<string, string>(nodeName, "Phillips"));
                    }
                    else if (nodeName.Contains("company")) {
                        checkoutFormContentList.Enqueue(
                            new KeyValuePair<string, string>(nodeName, ""));
                    }
                    else if (nodeName.Contains("address1")) {
                        checkoutFormContentList.Enqueue(
                            new KeyValuePair<string, string>(nodeName, "3610 Swicegood Rd"));
                    }
                    else if (nodeName.Contains("address2")) {
                        checkoutFormContentList.Enqueue(
                            new KeyValuePair<string, string>(nodeName, ""));
                    }
                    else if (nodeName.Contains("city")) {
                        checkoutFormContentList.Enqueue(
                            new KeyValuePair<string, string>(nodeName, "Linwood"));
                    }
                    else if (nodeName.Contains("province")) {
                        checkoutFormContentList.Enqueue(
                            new KeyValuePair<string, string>(nodeName, "North Carolina"));
                    }
                    else if (nodeName.Contains("zip")) {
                        checkoutFormContentList.Enqueue(
                            new KeyValuePair<string, string>(nodeName, "27299"));
                    }
                    else if (nodeName.Contains("phone")) {
                        checkoutFormContentList.Enqueue(
                            new KeyValuePair<string, string>(nodeName, "(336) 999-4585"));
                    }
                    else {
                        checkoutFormContentList.Enqueue(
                            new KeyValuePair<string, string>(nodeName, node.GetAttributeValue("value", "")));
                    }
                }
            }

            foreach (var node in form.Descendants("select")) {
                var nodeName = node.GetAttributeValue("name", "");
                if (node.GetAttributeValue("type", "").Equals("hidden")) {
                    checkoutFormContentList.Enqueue(
                        new KeyValuePair<string, string>(nodeName, node.GetAttributeValue("value", "")));
                }
                else {
                    if (nodeName.Contains("country")) {
                        checkoutFormContentList.Enqueue(
                            new KeyValuePair<string, string>(nodeName, "United States"));
                    }
                }
            }

            checkoutFormContentList.Enqueue(
                new KeyValuePair<string, string>("g-recaptcha-response", "03AOLTBLQ42nCkesd4NiAiGMdH_SE9j80d4vQk1xIBYhIPq2vCV0kigwoHHDEpLnvzJpk9R9rVURp1pPJNIsfEkZD9vS4Sx_18qmmqnATPiQlBqMNXjX752Jvx6x6prruvU3XkNnA-B1P4oqKFOFtiuRus7Hyi0sQztiPcF_56hGxS1I0gDON7ZdH2x90tLM1QXGW0hm5HDD7JYGQC1Jo3NN2awNoLtERuxNa2AnAXgmIpP5XJxs7LONLNiunbUEe6CetUOZl4RZ6mBWaH7S94POs-slaLX8uoFaZBl8IZQIT7iQrfG4bYlODMcfeTViMt2HVlEpDzEQ6mXTS4xerd8RQTTBz-vNrx1bqABETvt_uoi0iVDjj6-3kwvuG_6DFJdtyVDP5ODlx9pRNIhv1kYad6JhKnnRzTKJSfujZoaWsS4Qt0cLKKK8n0h0i_9OIKnF_QnB5Qnuj4faswHJBFNr8kgZm0YWXVpCjsFz-0ww5bjiaoY1zhd-I"));

            checkoutFormContent = new FormUrlEncodedContent(checkoutFormContentList);

            var shippingDoc = new HtmlDocument();

            using (var request = new HttpRequestMessage {
                RequestUri = new Uri(checkoutUrl),
                Method = HttpMethod.Post,
            }) {
                using var client = GetNewHttpClient();

                request.Headers.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                request.Headers.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                request.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");

                request.Content = checkoutFormContent;

                var beginCheckoutResponse = HttpHelper.GetResponse(request, client, CancellationToken.None).Result;
                shippingDoc.LoadHtml(beginCheckoutResponse.Content.ReadAsStringAsync().Result);
            }

            string shippingCode = "", shippingControlName = "";
            int minShippingCents = int.MaxValue;

            var shippingContentList = new Queue<KeyValuePair<string, string>>();
            var shippingForm = shippingDoc.DocumentNode.SelectSingleNode("//form[@class=\"edit_checkout\"]");
            foreach (var node in shippingForm.Descendants("input")) {
                var nodeName = node.GetAttributeValue("name", "");
                if (node.GetAttributeValue("type", "").Equals("hidden")) {
                    shippingContentList.Enqueue(
                        new KeyValuePair<string, string>(nodeName, node.GetAttributeValue("value", "")));
                }
                else {
                    if (nodeName.Contains("shipping_rate")) {
                        int shippingCents = int.Parse(node.GetAttributeValue("data-checkout-total-shipping-cents", "2147483647"));
                        if (shippingCents >= minShippingCents) continue;
                        minShippingCents = shippingCents;
                        shippingCode = node.GetAttributeValue("value", "shopify-UPS%20Ground%20(oz-2lbs)-10.00");
                        shippingControlName = nodeName;
                    }
                }
            }

            shippingContentList.Enqueue(new KeyValuePair<string, string>(shippingControlName, shippingCode));

            var shippingContent = new FormUrlEncodedContent(shippingContentList);
            Console.WriteLine(checkoutUrl);

            using (var request = new HttpRequestMessage {
                RequestUri = new Uri(checkoutUrl),
                Method = HttpMethod.Post,
            }) {
                using var client = GetNewHttpClient();

                request.Headers.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                request.Headers.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                request.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");

                request.Content = shippingContent;

                var beginCheckoutResponse = HttpHelper.GetResponse(request, client, CancellationToken.None).Result;
            }

            string continueUrl;

            using (var request = new HttpRequestMessage {
                RequestUri = new Uri(checkoutUrl),
                Method = HttpMethod.Post,
            }) {
                using var client = GetNewHttpClient();

                request.Headers.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                request.Headers.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                request.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");

                request.Content = shippingContent;

                var beginCheckoutResponse = HttpHelper.GetResponse(request, client, CancellationToken.None).Result;
                continueUrl = beginCheckoutResponse.RequestMessage.RequestUri.ToString();
            }

            var storeId = checkoutAction.Split("/")[1];
            var checkoutId = checkoutAction.Split("/")[3];

            Console.WriteLine($"{storeId} {checkoutId}");

            string ccVerifyCode;

            using (var request = new HttpRequestMessage {
                RequestUri = new Uri($"https://elb.deposit.shopifycs.com/sessions"),
                Method = HttpMethod.Post,
            }) {
                using var client = GetNewHttpClient();

                request.Headers.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                request.Headers.TryAddWithoutValidation("Accept", "application/json");
                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                request.Headers.TryAddWithoutValidation("Origin", "https://checkout.us.shopifycs.com");
                request.Headers.TryAddWithoutValidation("Referer", $"https://checkout.us.shopifycs.com/number?identifier={checkoutId}&location=https%3A%2F%2Fbdgastore.com%2F{storeId}%2Fcheckouts%2F0c232d8876fdfb16ef5b451f3a8db21f%3Fprevious_step%3Dshipping_method%26step%3Dpayment_method?previous_step=shipping_method&step=payment_method&dir=ltr");

                var payload = "{\"credit_card\":{\"number\":\"" + "4242-4242-4242-4242".Replace("-", " ") + "\",\"name\":\"" + "Trey" + " " + "Phillips" + "\",\"month\":" + "9" + ",\"year\":" + "2020" + "," + "\"verification_value\":\"" + "123" + "\"}}";
                request.Content = new StringContent(payload);

                var beginCheckoutResponse = HttpHelper.GetResponse(request, client, CancellationToken.None).Result;
                Console.WriteLine(beginCheckoutResponse.Content.ReadAsStringAsync().Result);
                ccVerifyCode = JObject.Parse(beginCheckoutResponse.Content.ReadAsStringAsync().Result).Value<string>("id");
            }

            var finalDoc = new HtmlDocument();

            using (var request = new HttpRequestMessage {
                RequestUri = new Uri(checkoutUrl + "?step=payment_method"),
                Method = HttpMethod.Get,
            }) {
                using var client = GetNewHttpClient();

                request.Headers.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                request.Headers.TryAddWithoutValidation("Accept", "*/*");
                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");

                var beginCheckoutResponse = HttpHelper.GetString(request, client, CancellationToken.None).Result;
                finalDoc.LoadHtml(beginCheckoutResponse);
            }

            var paymentGatewayList = new Queue<KeyValuePair<string, string>>();
            bool firstPaymentGateway = true, firstBillingAddressDifferent = true;
            var finalForm = finalDoc.DocumentNode.SelectSingleNode("//form[@class=\"edit_checkout\" and @data-payment-form]");
            foreach (var node in finalForm.Descendants("input")) {
                if (node.GetAttributeValue("data-honeypot", "").Equals("true")) continue;
                var nodeName = node.GetAttributeValue("name", "");
                if (node.GetAttributeValue("type", "").Equals("hidden")) {
                    if (nodeName.Contains("payment_gateway")) continue;
                    else if (nodeName.Equals("s")) {
                        paymentGatewayList.Enqueue(
                            new KeyValuePair<string, string>(nodeName, ccVerifyCode));
                    }
                    else if (nodeName.Contains("redirect")) { }
                    else {
                        paymentGatewayList.Enqueue(
                            new KeyValuePair<string, string>(nodeName, node.GetAttributeValue("value", "")));
                    }
                    Console.WriteLine($"Hidden Input: {nodeName}");
                }
                else {
                    if (nodeName.Contains("different_billing") && firstBillingAddressDifferent) {
                        paymentGatewayList.Enqueue(
                            new KeyValuePair<string, string>(nodeName, node.GetAttributeValue("value", "")));
                        firstBillingAddressDifferent = false;
                    }
                    else if (nodeName.Contains("payment_gateway") && firstPaymentGateway) {
                        paymentGatewayList.Enqueue(
                            new KeyValuePair<string, string>(nodeName, node.GetAttributeValue("value", "")));
                        firstPaymentGateway = false;
                    }
                    Console.WriteLine($"Visible Input: {nodeName}");
                }
            }

            using (var request = new HttpRequestMessage {
                RequestUri = new Uri(checkoutUrl),
                Method = HttpMethod.Post,
            }) {
                using var client = GetNewHttpClient();

                request.Headers.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                request.Headers.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                request.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");

                request.Content = new FormUrlEncodedContent(paymentGatewayList);

                var beginCheckoutResponse = HttpHelper.GetResponse(request, client, CancellationToken.None).Result;
                continueUrl = beginCheckoutResponse.RequestMessage.RequestUri.ToString();
                Console.WriteLine(continueUrl);
                Console.WriteLine(beginCheckoutResponse.Content.ReadAsStringAsync().Result);
            }
        }
    }
}
