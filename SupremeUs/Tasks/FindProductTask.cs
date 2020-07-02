using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Services;
using Common.Supreme;

namespace Supreme.Tasks {
    class FindProductTask : BotTask {
        public FindProductTask(Bot bot) : base(bot) { }

        public override bool Validate() {
            var bot = (SupremeBot) GetBot();
            bot.Status = "Waiting for Monitor";

            while (SupremeMonitor.MobileStock is null && !bot.GetCancellationToken().IsCancellationRequested) { }

            return !bot.GetCancellationToken().IsCancellationRequested &&
                   SupremeMonitor.MobileStock.ProductsAndCategories.ContainsKey(bot.SearchProduct.Category);
        }

        public override void Execute() { // TODO: check keyword logic
            var bot = (SupremeBot) GetBot();
            do {
                bot.MobileStock = (MobileStock) SupremeMonitor.MobileStock.Clone();

                Parallel.ForEach(bot.MobileStock.ProductsAndCategories[bot.SearchProduct.Category],
                    (product, state) => {
                        var isValid = !bot.SearchProduct.ProductKeywords.Any(keyword =>
                            keyword.StartsWith("-")
                                ? !product.Name.IndexOf(keyword[1..],
                                    StringComparison.CurrentCultureIgnoreCase).Equals(-1)
                                : product.Name.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase).Equals(-1));
                        if (!isValid) return;
                        bot.MobileStockProduct = product;
                        state.Stop();
                    });
            } while (bot.MobileStockProduct is null);
        }


        public override int Priority() {
            return 10;
        }

        public override string Description() {
            return "Find Product";
        }
    }
}
