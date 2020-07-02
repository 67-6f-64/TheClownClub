using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using Common;
using Common.Services;
using Common.Supreme;

namespace Supreme.Tasks {
    class WaitForStockTask : BotTask {
        public WaitForStockTask(Bot bot) : base(bot) { }

        public override bool Validate() {
            var bot = (SupremeBot) GetBot();
            return bot.ProductSize != null && bot.ProductSize.StockLevel.Equals(0);
        }

        public override void Execute() { // TODO: check keyword logic
            var bot = (SupremeBot) GetBot();

            do {
                string productJson;
                using (var request = new HttpRequestMessage() {
                    RequestUri = new Uri($"https://www.supremenewyork.com/shop/{bot.MobileStockProduct.Id}.json"),
                    Method = HttpMethod.Get,
                }) {
                    using var client = bot.GetNewHttpClient();
                    request.Headers.TryAddWithoutValidation("User-Agent",
                        "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                    request.Headers.TryAddWithoutValidation("Accept", "*/*");
                    request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                    request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                    productJson = HttpHelper.GetStringSync(request, client, out _, bot.GetCancellationToken());
                    bot.RequestsList.Add(new Tuple<HttpRequestMessage, string>(request, productJson));
                }

                if (string.IsNullOrEmpty(productJson) || bot.GetCancellationToken().IsCancellationRequested) return;

                try {
                    bot.Product = SupremeProduct.FromJson(productJson);
                    bot.ProductStyle = bot.Product.Styles.First(style => style.Id.Equals(bot.ProductStyle.Id));
                    bot.ProductSize = bot.ProductStyle.Sizes.First(size => size.Id.Equals(bot.ProductSize.Id));
                }
                catch (Exception) { }
            } while (bot.ProductSize.StockLevel.Equals(0));
        }


        public override int Priority() {
            return 30;
        }

        public override string Description() {
            return "Wait For Stock";
        }
    }
}
