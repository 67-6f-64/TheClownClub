using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using Common;
using Common.Services;
using Common.Supreme;

namespace Supreme.Tasks {
    public class FindStyleAndSizeTask : BotTask {
        public FindStyleAndSizeTask(Bot bot) : base(bot) { }

        private bool UpdateProductJson() {
            var bot = (SupremeBot) GetBot();

            string productJson;
            do {
                using var request = new HttpRequestMessage {
                    RequestUri = new Uri($"https://www.supremenewyork.com/shop/{bot.MobileStockProduct.Id}.json"),
                    Method = HttpMethod.Get,
                };
                using var client = bot.GetNewHttpClient();
                request.Headers.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                request.Headers.TryAddWithoutValidation("Accept", "*/*");
                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                productJson = HttpHelper.GetStringSync(request, client, out _,
                    bot.GetCancellationToken());
                bot.RequestsList.Add(new Tuple<HttpRequestMessage, string>(request, productJson));
            } while (string.IsNullOrEmpty(productJson));

            if (bot.GetCancellationToken().IsCancellationRequested) return false;

            try {
                bot.Product = SupremeProduct.FromJson(productJson);
            }
            catch (Exception) {
                try {
                    bot.Product = SupremeProduct.FromJson(productJson);
                }
                catch (Exception) { }
            }

            return true;
        }

        public override bool Validate() {
            var bot = (SupremeBot) GetBot();
            return !bot.GetCancellationToken().IsCancellationRequested && bot.MobileStockProduct != null &&
                   UpdateProductJson();
        }

        public override void Execute() { // TODO: check the keyword logic
            var bot = (SupremeBot) GetBot();

            if (bot.SearchProduct.AnyStyle && bot.SearchProduct.AnySize) {
                do {
                    Size anySize = null;
                    var anyStyle = bot.Product.Styles.FirstOrDefault(potentialStyle => {
                        anySize = potentialStyle.Sizes.FirstOrDefault(potentialSize =>
                            potentialSize.StockLevel > 0);
                        return anySize != null;
                    });

                    if (anyStyle is null || anySize is null) {
                        UpdateProductJson();
                        continue;
                    }

                    bot.ProductStyle = anyStyle;
                    bot.ProductSize = anySize;
                } while (bot.ProductSize is null);

                return;
            }

            Style style;
            if (bot.SearchProduct.AnyStyle) {
                do {
                    style = bot.Product.Styles.FirstOrDefault(potentialStyle =>
                        potentialStyle.Sizes.FirstOrDefault(potentialSize => potentialSize.StockLevel > 0) != null);

                    if (style is null) UpdateProductJson();
                } while (style is null);
            }
            else {
                style = bot.Product.Styles.FirstOrDefault(potentialStyle => bot.SearchProduct.StyleKeywords.Any(
                    keyword =>
                        !potentialStyle.Name.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase)
                            .Equals(-1)));
            }

            if (style is null) return;

            bot.ProductStyle = style;

            Size size;
            if (bot.SearchProduct.AnySize) {
                do {
                    size = bot.ProductStyle.Sizes.FirstOrDefault(potentialSize => potentialSize.StockLevel > 0);

                    if (size is null) UpdateProductJson();
                } while (size is null);
            }
            else {
                size = bot.ProductStyle.Sizes.FirstOrDefault(potentialSize =>
                    potentialSize.Name.Equals(bot.SearchProduct.SizeKeyword,
                        StringComparison.CurrentCultureIgnoreCase));
            }

            if (size is null) return;

            bot.ProductSize = size;
        }

        public override int Priority() {
            return 20;
        }

        public override string Description() {
            return "Find Style/Size";
        }
    }
}
