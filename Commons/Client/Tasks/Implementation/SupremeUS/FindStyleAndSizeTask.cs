using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common.Services;
using Common.Supreme;

namespace Common {
    public class FindStyleAndSizeTask : BotTask {
        public FindStyleAndSizeTask(Bot bot) : base(bot) { }

        public override bool Validate() {
            var bot = (SupremeUsBot)GetBot();
            if (bot.MobileStockProduct is null) return false;

            var clientHandler = bot.GetClientHandler();
            string productJson = null;
            HttpStatusCode? productStatus = null;
            using (var request = new HttpRequestMessage() {
                RequestUri = new Uri($"https://www.supremenewyork.com/shop/{bot.MobileStockProduct.Id}.json"),
                Method = HttpMethod.Get,
            }) {
                using (var client = new HttpClient(clientHandler, false)) {
                    request.Headers.TryAddWithoutValidation("User-Agent",
                        "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                    request.Headers.TryAddWithoutValidation("Accept", "*/*");
                    request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                    request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                    productJson = HttpHelper.GetStringSync(request, client, out productStatus, bot.GetCancellationToken());
                }
            }

            if (productJson is null) return false;

            bot.Product = Product.FromJson(productJson);
            return true;
        }

        public override void Execute() { // TODO: check the keyword logic
            var bot = (SupremeUsBot) GetBot();

            if (bot.SearchProduct.AnyStyle && bot.SearchProduct.AnySize) {
                Size anySize = null;
                var anyStyle = bot.Product.Styles.FirstOrDefault(potentialStyle => {
                    anySize = potentialStyle.Sizes.FirstOrDefault(potentialSize => potentialSize.StockLevel > 0);
                    return anySize != null;
                });

                if (anyStyle is null || anySize is null) return;

                bot.ProductStyle = anyStyle;
                bot.ProductSize = anySize;

                Console.WriteLine("Found Style: " + anyStyle.Name);
                Console.WriteLine("Found Size: " + anySize.Name);

                return;
            }

            Style style = null;
            if (bot.SearchProduct.AnyStyle) {
                style = bot.Product.Styles.FirstOrDefault(potentialStyle => potentialStyle.Sizes.FirstOrDefault(potentialSize => potentialSize.StockLevel > 0) != null);
            }
            else {
                style = bot.Product.Styles.FirstOrDefault(potentialStyle => bot.SearchProduct.StyleKeywords.Any(
                    keyword =>
                        !potentialStyle.Name.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase).Equals(-1)));
            }

            if (style is null) return;

            bot.ProductStyle = style;
            Console.WriteLine("Found Style: " + style.Name);

            Size size = null;
            if (bot.SearchProduct.AnySize) {
                size = bot.ProductStyle.Sizes.FirstOrDefault(potentialSize => potentialSize.StockLevel > 0);
            }
            else {
                size = bot.ProductStyle.Sizes.FirstOrDefault(potentialSize =>
                    potentialSize.Name.Equals(bot.SearchProduct.SizeKeyword,
                        StringComparison.CurrentCultureIgnoreCase));
            }

            if (size is null) return;

            bot.ProductSize = size;
            Console.WriteLine("Found Size: " + size.Name);
        }

        public override int Priority() {
            return 20;
        }

        public override string Description() {
            return "Find Style/Size";
        }
    }
}
