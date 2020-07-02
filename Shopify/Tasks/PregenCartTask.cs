using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Common;
using Common.Services;
using HtmlAgilityPack;

namespace Shopify.Tasks {
    class PregenCartTask : BotTask {
        public PregenCartTask(Bot bot) : base(bot) { }

        public override string Description() {
            return "Generating Cart";
        }

        public override void Execute() {
            var bot = (ShopifyBot)GetBot();
            using (var request = new HttpRequestMessage {
                RequestUri = new Uri($"https://{bot.Domain}/cart/add.js?quantity=1&id={bot.ShopifyProducts.ProductsList.First().Variants.First().Id}"),
                Method = HttpMethod.Get,
            }) {
                using var client = bot.GetNewHttpClient();

                request.Headers.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                request.Headers.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");

                var atcResponse = HttpHelper.GetStringSync(request, client, out _, bot.GetCancellationToken());
            }

            using (var request = new HttpRequestMessage {
                RequestUri = new Uri($"https://{bot.Domain}/checkout.json"),
                Method = HttpMethod.Get,
            }) {
                using var client = bot.GetNewHttpClient();

                request.Headers.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                request.Headers.TryAddWithoutValidation("Accept", "*/*");
                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");

                var beginCheckoutResponse = HttpHelper.GetResponse(request, client, bot.GetCancellationToken()).Result;
                var checkoutDoc = new HtmlDocument();
                checkoutDoc.LoadHtml(beginCheckoutResponse.Content.ReadAsStringAsync().Result);
                bot.CheckoutUrl = beginCheckoutResponse.RequestMessage.RequestUri.ToString();
                bot.CheckoutAction = checkoutDoc.DocumentNode.SelectSingleNode("//form[@class=\"edit_checkout\"]").GetAttributeValue("action", "");
            }
        }

        public override int Priority() {
            return 10;
        }

        public override bool Validate() {
            if (!((ShopifyBot)GetBot()).PregenCart) return false;
            GetBot().Status = "Waiting for Monitor";
            while (((ShopifyBot)GetBot()).ShopifyProducts is null) { }
            return true;
        }
    }
}
