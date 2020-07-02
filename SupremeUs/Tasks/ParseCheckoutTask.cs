using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.XPath;
using Common;
using Common.Services;
using HtmlAgilityPack;

namespace Supreme.Tasks {
    class ParseCheckoutTask : BotTask {
        public ParseCheckoutTask(Bot bot) : base(bot) { }

        public override bool Validate() {
            var bot = (SupremeBot)GetBot();

            string checkoutHtml;

            using (var request = new HttpRequestMessage() {
                RequestUri = new Uri($"https://www.supremenewyork.com/mobile#checkout"),
                Method = HttpMethod.Get,
            }) {
                using var client = new HttpClient(bot.GetClientHandler(), false);
                request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                request.Headers.TryAddWithoutValidation("Accept", "*/*");
                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");

                checkoutHtml = HttpHelper.GetStringSync(request, client, out _, bot.GetCancellationToken());
                bot.RequestsList.Add(new Tuple<HttpRequestMessage, string>(request, checkoutHtml));
            }

            if (bot.GetCancellationToken().IsCancellationRequested) return false;

            bot.CheckoutHtml = checkoutHtml;
            return !string.IsNullOrEmpty(checkoutHtml);
        }

        public override void Execute() { // TODO: improve parser and verify we have all required fields
            var bot = (SupremeBot) GetBot();

            var checkoutDoc = new HtmlDocument();
            checkoutDoc.LoadHtml(bot.CheckoutHtml);

            //bot.CsrfToken = checkoutDoc.DocumentNode.SelectSingleNode("//meta[@name=\"csrf-token\"]").GetAttributeValue("content", "");

            HtmlNode form = null;
            foreach (var script in checkoutDoc.DocumentNode.Descendants("script")) {
                try {
                    var scriptDoc = new HtmlDocument();
                    scriptDoc.LoadHtml(script.InnerHtml);
                    if (!scriptDoc.DocumentNode.Descendants("form").First().GetAttributeValue("action", "")
                        .Equals("https://www.supremenewyork.com/checkout.json")) continue;
                    form = scriptDoc.DocumentNode.Descendants("form").First();
                }
                catch (Exception) { }
            }

            if (form is null) return;

            var formContentList = new ConcurrentQueue<KeyValuePair<string, string>>();
            var inputLoop = Task.Run(() => {
                foreach (var node in form.Descendants("input")) {
                    var nodeName = node.GetAttributeValue("name", "");
                    if (node.GetAttributeValue("type", "").Equals("hidden")) {
                        formContentList.Enqueue(nodeName.Equals("cookie-sub")
                            ? new KeyValuePair<string, string>(nodeName, $"{{\"{bot.ProductSize.Id}\":1}}")
                            : new KeyValuePair<string, string>(nodeName, node.GetAttributeValue("value", "")));
                    }
                    else {
                        if (nodeName.Contains("name")) {
                            formContentList.Enqueue(new KeyValuePair<string, string>(nodeName,
                                bot.BillingProfile.FirstName + " " + bot.BillingProfile.LastName));
                        }
                        else if (nodeName.Contains("email")) {
                            formContentList.Enqueue(
                                new KeyValuePair<string, string>(nodeName, bot.BillingProfile.Email));
                        }
                        else if (nodeName.Contains("tel")) {
                            formContentList.Enqueue(
                                new KeyValuePair<string, string>(nodeName, bot.BillingProfile.Phone));
                        }
                        else if (nodeName.Contains("address_2")) {
                            formContentList.Enqueue(
                                new KeyValuePair<string, string>(nodeName, bot.BillingProfile.Address2));
                        }
                        else if (nodeName.Contains("address_3")) {
                            formContentList.Enqueue(
                                new KeyValuePair<string, string>(nodeName, bot.BillingProfile.Address2));
                        }
                        else if (nodeName.Contains("billing_address")) {
                            formContentList.Enqueue(
                                new KeyValuePair<string, string>(nodeName, bot.BillingProfile.Address));
                        }
                        else if (nodeName.Contains("city")) {
                            formContentList.Enqueue(
                                new KeyValuePair<string, string>(nodeName, bot.BillingProfile.City));
                        }
                        else if (nodeName.Contains("zip")) {
                            formContentList.Enqueue(
                                new KeyValuePair<string, string>(nodeName, bot.BillingProfile.ZipCode));
                        }
                        else if (nodeName.Contains("vvv")) {
                            formContentList.Enqueue(new KeyValuePair<string, string>(nodeName, bot.BillingProfile.Cvv));
                        }
                        else if (nodeName.Contains("term")) {
                            formContentList.Enqueue(new KeyValuePair<string, string>(nodeName, "1"));
                        }
                        else {
                            var nodePlaceholder = node.GetAttributeValue("placeholder", "");
                            if (nodePlaceholder.Contains("number")) {
                                formContentList.Enqueue(
                                    new KeyValuePair<string, string>(nodeName, bot.BillingProfile.CcNumber));
                            }
                            else if (nodePlaceholder.Contains("cvv")) {
                                formContentList.Enqueue(
                                    new KeyValuePair<string, string>(nodeName, bot.BillingProfile.Cvv));
                            }
                        }
                    }
                }
            });

            var selectLoop = Task.Run(() => {
                foreach (var node in form.Descendants("select")) {
                    var nodeName = node.GetAttributeValue("name", "");
                    if (node.GetAttributeValue("type", "").Equals("hidden")) {
                        formContentList.Enqueue(
                            new KeyValuePair<string, string>(nodeName, node.GetAttributeValue("value", "")));
                    }
                    else {
                        if (nodeName.Contains("state")) {
                            formContentList.Enqueue(
                                new KeyValuePair<string, string>(nodeName, bot.BillingProfile.State));
                        }
                        else if (nodeName.Contains("country")) {
                            formContentList.Enqueue(
                                new KeyValuePair<string, string>(nodeName, bot.BillingProfile.Country));
                        }
                        else if (nodeName.Contains("month")) {
                            formContentList.Enqueue(new KeyValuePair<string, string>(nodeName,
                                bot.BillingProfile.CcExpiration.Month.ToString("d2")));
                        }
                        else if (nodeName.Contains("year")) {
                            formContentList.Enqueue(new KeyValuePair<string, string>(nodeName,
                                bot.BillingProfile.CcExpiration.Year.ToString()));
                        }
                        else if (nodeName.Contains("card[type]")) {
                            formContentList.Enqueue(new KeyValuePair<string, string>(nodeName,
                                bot.BillingProfile.CcType));
                        }
                        else {
                            if (node.SelectNodes("option").Single(node => node.GetAttributeValue("value", "").Equals("visa")) != null) {
                                formContentList.Enqueue(new KeyValuePair<string, string>(nodeName,
                                    bot.BillingProfile.CcType));
                            }
                        }
                    }
                }
            });

            Task.WaitAll(inputLoop, selectLoop);

            if (!bot.CaptchaBypass) {
                bot.Status = "Waiting for Captcha";

                var captchaKeyValuePair = new KeyValuePair<DateTime, string>();
                do {
                    if (!CaptchaHarvester.Tokens.ContainsKey("6LeWwRkUAAAAAOBsau7KpuC9AV-6J8mhw4AjC3Xz")) continue;
                    CaptchaHarvester.Tokens["6LeWwRkUAAAAAOBsau7KpuC9AV-6J8mhw4AjC3Xz"]
                        .TryDequeue(out captchaKeyValuePair);
                    if (string.IsNullOrEmpty(captchaKeyValuePair.Value)) continue;
                    if (captchaKeyValuePair.Key < bot.AtcTime
                        || DateTime.Now - captchaKeyValuePair.Key > TimeSpan.FromSeconds(110)) {
                        captchaKeyValuePair = new KeyValuePair<DateTime, string>();
                    }
                } while (captchaKeyValuePair.Value is null);

                formContentList.Enqueue(new KeyValuePair<string, string>("g-recaptcha-response", captchaKeyValuePair.Value));
            }

            bot.CheckoutFormUrlEncodedContent = new FormUrlEncodedContent(formContentList);
        }

        public override int Priority() {
            return 50;
        }

        public override string Description() {
            return "Parse Checkout";
        }
    }
}
