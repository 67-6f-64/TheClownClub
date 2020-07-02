using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Common;
using Common.Services;
using Common.Supreme;
using Newtonsoft.Json.Linq;

namespace Supreme.Tasks {
    class AddToCartTask : BotTask {
        public AddToCartTask(Bot bot) : base(bot) { }

        public override bool Validate() {
            var bot = (SupremeBot)GetBot();
            return bot.MobileStockProduct != null && bot.ProductStyle != null && bot.ProductSize != null; // we don't use bot.Product
        }

        public override void Execute() {
            var bot = (SupremeBot)GetBot();

            string atcResponse;
            using (var addToCartRequest = new HttpRequestMessage() {
                RequestUri = new Uri($"https://www.supremenewyork.com/shop/{bot.MobileStockProduct.Id}/add.json"),
                Method = HttpMethod.Post
            }) {
                using var client = new HttpClient(bot.GetClientHandler(), false);
                addToCartRequest.Headers.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (iPhone; CPU iPhone OS 10_3_3 like Mac OS X) AppleWebKit/603.1.30 (KHTML, like Gecko) CriOS/62.0.3202.70 Mobile/14G60 Safari/602.1");
                addToCartRequest.Headers.TryAddWithoutValidation("Accept", "*/*");
                addToCartRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                addToCartRequest.Headers.TryAddWithoutValidation("Content-Type",
                    "application/x-www-form-urlencoded");
                addToCartRequest.Headers.TryAddWithoutValidation("Refer", "http://www.supremenewyork.com/mobile");
                addToCartRequest.Content = bot.MobileStockProduct.PriceEuro is null
                    ? new FormUrlEncodedContent(new[] {
                        new KeyValuePair<string, string>("st", bot.ProductStyle.Id.ToString()),
                        new KeyValuePair<string, string>("s", bot.ProductSize.Id.ToString()),
                        new KeyValuePair<string, string>("qty", "1")
                    })
                    : new FormUrlEncodedContent(new[] {
                        new KeyValuePair<string, string>("style", bot.ProductStyle.Id.ToString()),
                        new KeyValuePair<string, string>("size", bot.ProductSize.Id.ToString()),
                        new KeyValuePair<string, string>("qty", "1")
                    });

                atcResponse = HttpHelper.GetStringSync(addToCartRequest, client, out _,
                    bot.GetCancellationToken());

                bot.RequestsList.Add(new Tuple<HttpRequestMessage, string>(addToCartRequest, atcResponse));
            }

            CaptchaHarvester.AddWindow("supremenewyork.com", "6LeWwRkUAAAAAOBsau7KpuC9AV-6J8mhw4AjC3Xz");

            if (string.IsNullOrEmpty(atcResponse) || bot.GetCancellationToken().IsCancellationRequested) return;

            bot.AtcTime = DateTime.Now;
            bot.DelayStopwatch = Stopwatch.StartNew();
        }

        public override int Priority() {
            return 40;
        }

        public override string Description() {
            return "Add to Cart";
        }
    }
}
