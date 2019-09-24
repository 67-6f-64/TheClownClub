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
    class FindProductTask : BotTask {
        public FindProductTask(Bot bot) : base(bot) { }

        public override bool Validate() {
            var bot = (SupremeUsBot)GetBot();
            var clientHandler = bot.GetClientHandler();

            string mobileStockJson = null;
            HttpStatusCode? mobileStockStatus = null;

            using (var request = new HttpRequestMessage() {
                RequestUri = new Uri("https://www.supremenewyork.com/mobile_stock.json"),
                Method = HttpMethod.Get,
            }) {
                using (var client = new HttpClient(clientHandler, false)) {
                    request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                    request.Headers.TryAddWithoutValidation("Accept", "*/*");
                    request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                    request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                    mobileStockJson = HttpHelper.GetStringSync(request, client, out mobileStockStatus, bot.GetCancellationToken());
                }
            }

            if (mobileStockJson is null) return false;

            bot.MobileStock = MobileStock.FromJson(mobileStockJson);
            return true;
        }

        public override void Execute() { // TODO: check keyword logic
            var bot = (SupremeUsBot) GetBot();

            Parallel.ForEach(bot.MobileStock.ProductsAndCategories[bot.SearchProduct.Category], (product, state) => {
                var isValid = !bot.SearchProduct.ProductKeywords.Any(keyword =>
                    keyword.StartsWith("-")
                        ? !product.Name.IndexOf(keyword.Substring(1, keyword.Length - 1),
                            StringComparison.CurrentCultureIgnoreCase).Equals(-1)
                        : product.Name.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase).Equals(-1));
                if (!isValid) return;
                Console.WriteLine("Found Product: " + product.Name);
                bot.MobileStockProduct = product;
                state.Stop();
            });
        }


        public override int Priority() {
            return 10;
        }

        public override string Description() {
            return "Find Product";
        }
    }
}
