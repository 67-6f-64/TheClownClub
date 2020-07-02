using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Services;
using HtmlAgilityPack;

namespace Shopify.Tasks {
    class FindProductTask : BotTask {
        public FindProductTask(Bot bot) : base(bot) { }

        public override string Description() {
            return "Find Product";
        }

        public override void Execute() {
            var bot = (ShopifyBot) GetBot();
            do {
                Parallel.ForEach(bot.ShopifyProducts.ProductsList,
                    (product, state) => {
                        var isValid = !bot.SearchProduct.ProductKeywords.Any(keyword =>
                            keyword.StartsWith("-")
                                ? !product.Title.IndexOf(keyword[1..],
                                    StringComparison.CurrentCultureIgnoreCase).Equals(-1)
                                : product.Title.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase).Equals(-1));
                        if (!isValid) return;
                        bot.ShopifyProduct = product;
                        state.Stop();
                    });
            } while (bot.ShopifyProduct is null);
        }

        public override int Priority() {
            return 20;
        }

        public override bool Validate() {
            GetBot().Status = "Waiting for Monitor";
            while (((ShopifyBot)GetBot()).ShopifyProducts is null) { }
            return true;
        }
    }
}
