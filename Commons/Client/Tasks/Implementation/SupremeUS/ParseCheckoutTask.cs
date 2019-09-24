using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common.Services;
using HtmlAgilityPack;

namespace Common {
    class ParseCheckoutTask : BotTask {
        public ParseCheckoutTask(Bot bot) : base(bot) { }

        public override bool Validate() {
            var bot = (SupremeUsBot)GetBot();

            string checkoutHtml = null;
            HttpStatusCode? checkoutStatus = null;

            using (var request = new HttpRequestMessage() {
                RequestUri = new Uri($"https://www.supremenewyork.com/checkout"),
                Method = HttpMethod.Get,
            }) {
                using (var client = new HttpClient(bot.GetClientHandler(), false)) {
                    request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                    request.Headers.TryAddWithoutValidation("Accept", "*/*");
                    request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                    request.Headers.TryAddWithoutValidation("Connection", "keep-alive");

                    checkoutHtml = HttpHelper.GetStringSync(request, client, out checkoutStatus, bot.GetCancellationToken());
                }
            }

            bot.CheckoutHtml = checkoutHtml;
            return checkoutHtml != null;
        }

        public override void Execute() { // TODO: improve parser and verify we have all required fields
            var bot = (SupremeUsBot) GetBot();

            var doc = new HtmlDocument();
            doc.LoadHtml(bot.CheckoutHtml);

            bot.CsrfToken = doc.DocumentNode.SelectSingleNode("//meta[@name=\"csrf-token\"]").GetAttributeValue("content", "");

            var form = doc.DocumentNode.Descendants("form").FirstOrDefault();
            if (form is null) return;

            var formContentList = new List<KeyValuePair<string, string>>();
            var inputLoop = Task.Factory.StartNew(() => {
                Parallel.ForEach(form.Descendants("input"), node => {
                    var nodeName = node.GetAttributeValue("name", "");
                    if (node.GetAttributeValue("type", "").Equals("hidden")) {
                        formContentList.Add(new KeyValuePair<string, string>(nodeName, node.GetAttributeValue("value", "")));
                    }
                    else {
                        if (nodeName.Contains("name")) {
                            formContentList.Add(new KeyValuePair<string, string>(nodeName, "Alberto Edison"));
                        }
                        else if (nodeName.Contains("email")) {
                            formContentList.Add(new KeyValuePair<string, string>(nodeName, "johnny.appleseed@gmail.com"));
                        }
                        else if (nodeName.Contains("tel")) {
                            formContentList.Add(new KeyValuePair<string, string>(nodeName, "801-675-9290"));
                        }
                        else if (nodeName.Contains("address_2")) {
                            formContentList.Add(new KeyValuePair<string, string>(nodeName, ""));
                        }
                        else if (nodeName.Contains("billing_address")) {
                            formContentList.Add(new KeyValuePair<string, string>(nodeName, "1676 Tori Lane"));
                        }
                        else if (nodeName.Contains("zip")) {
                            formContentList.Add(new KeyValuePair<string, string>(nodeName, "84116"));
                        }
                        else if (nodeName.Contains("city")) {
                            formContentList.Add(new KeyValuePair<string, string>(nodeName, "Salt Lake City"));
                        }
                        else if (nodeName.Contains("vvv")) {
                            formContentList.Add(new KeyValuePair<string, string>(nodeName, "123"));
                        }
                        else if (nodeName.Contains("terms")) {
                            formContentList.Add(new KeyValuePair<string, string>(nodeName, "1"));
                        }
                        else if (node.GetAttributeValue("placeholder", "").Contains("number")) {
                            formContentList.Add(new KeyValuePair<string, string>(nodeName, "4242424242424242"));
                        }
                    }
                });
            });

            var selectLoop = Task.Factory.StartNew(() => {
                Parallel.ForEach(form.Descendants("select"), node => {
                    var nodeName = node.GetAttributeValue("name", "");
                    if (node.GetAttributeValue("type", "").Equals("hidden")) {
                        formContentList.Add(new KeyValuePair<string, string>(nodeName, node.GetAttributeValue("value", "")));
                    }
                    else {
                        if (nodeName.Contains("state")) {
                            formContentList.Add(new KeyValuePair<string, string>(nodeName, "UT"));
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

            bot.CheckoutFormUrlEncodedContent = new FormUrlEncodedContent(formContentList);
        }

        public override int Priority() {
            return 40;
        }

        public override string Description() {
            return "Parse Checkout";
        }
    }
}
