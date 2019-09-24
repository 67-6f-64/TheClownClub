using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Common.Services;
using Newtonsoft.Json.Linq;

namespace Common {
    class AddToCartTask : BotTask {
        public AddToCartTask(Bot bot) : base(bot) { }

        public override bool Validate() {
            var bot = (SupremeUsBot)GetBot();
            return bot.MobileStockProduct != null && bot.ProductStyle != null && bot.ProductSize != null; // we don't use bot.Product
        }

        public override void Execute() {
            var bot = (SupremeUsBot)GetBot();

            string atcResponse = null;
            HttpStatusCode? atcStatusCode = null;
            using (var addToCartRequest = new HttpRequestMessage() {
                RequestUri = new Uri($"https://www.supremenewyork.com/shop/{bot.MobileStockProduct.Id}/add.json"),
                Method = HttpMethod.Post
            }) {
                using (var client = new HttpClient(bot.GetClientHandler(), false)) {
                    addToCartRequest.Headers.TryAddWithoutValidation("User-Agent",
                        "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                    addToCartRequest.Headers.TryAddWithoutValidation("Accept", "*/*");
                    addToCartRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                    addToCartRequest.Headers.TryAddWithoutValidation("Content-Type",
                        "application/x-www-form-urlencoded");
                    addToCartRequest.Headers.TryAddWithoutValidation("Refer", "http://www.supremenewyork.com/mobile");
                    addToCartRequest.Headers.TryAddWithoutValidation("Connection", "keep-alive");
                    addToCartRequest.Content = new FormUrlEncodedContent(new[] {
                        new KeyValuePair<string, string>("st", bot.ProductStyle.Id.ToString()),
                        new KeyValuePair<string, string>("s", bot.ProductSize.Id.ToString()),
                        new KeyValuePair<string, string>("qty", "1")
                    });

                    atcResponse = HttpHelper.GetStringSync(addToCartRequest, client, out atcStatusCode,
                        bot.GetCancellationToken());
                }
            }

            if (atcResponse is null) return;

            var atcResponseArray = JArray.Parse(atcResponse);
            if (atcResponseArray.Count <= 0) return;

            var atcResponseObject = JObject.Parse(atcResponseArray[0].ToString());
            if (atcResponseObject["size_id"].Value<string>().Equals(bot.ProductSize.Id.ToString())
                && atcResponseObject["in_stock"].Value<bool>())
                Console.WriteLine("Carted.");
        }

        public override int Priority() {
            return 30;
        }

        public override string Description() {
            return "Add to Cart";
        }
    }
}
