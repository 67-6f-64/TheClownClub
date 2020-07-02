using System;
using Common;
using Common.Shopify;
using Common.Supreme;
using Common.Types;
using Shopify.Tasks;

namespace Shopify {
    public class ShopifyBot : Bot {
        public string Domain { get; set; }
        public bool PregenCart { get; set; }
        public string CheckoutUrl { get; set; }
        public string CheckoutAction { get; set; }
        public ShopifyProducts ShopifyProducts { get; set; }
        public ShopifyProduct ShopifyProduct { get; set; }
        public ShopifyVariant ShopifyVariant { get; set; }
        public ShopifyBot() : base(BotType.Shopify, new BillingProfile(), new SearchProduct()) {
            Append(new PregenCartTask(this));
        }

        public ShopifyBot(BillingProfile profile, SearchProduct product) : base(BotType.Shopify, profile, product) {
            Append(new PregenCartTask(this));
        }
    }
}
