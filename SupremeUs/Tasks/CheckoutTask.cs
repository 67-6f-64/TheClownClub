using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Common;
using Common.Services;
using Newtonsoft.Json.Linq;

namespace Supreme.Tasks {
    class CheckoutTask : BotTask {
        public CheckoutTask(Bot bot) : base(bot) { }

        public override bool Validate() {
            var bot = (SupremeBot)GetBot();
            return bot.CheckoutFormUrlEncodedContent != null && !bot.GetCancellationToken().IsCancellationRequested;
        }

        public override void Execute() {
            var bot = (SupremeBot) GetBot();
            string checkoutResponse;

            using (var request = new HttpRequestMessage {
                RequestUri = new Uri($"https://www.supremenewyork.com/checkout.json"),
                Method = HttpMethod.Post,
            }) {
                using var client = new HttpClient(bot.GetClientHandler(), false);
                request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                request.Headers.TryAddWithoutValidation("Accept", "*/*");
                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                request.Headers.TryAddWithoutValidation("Refer", "https://www.supremenewyork.com/mobile");
                request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.8,en-GB;q=0.6");
                //request.Headers.Add("X-CSRF-Token", bot.CsrfToken); // Not needed anymore, we use mobile endpoint
                request.Content = bot.CheckoutFormUrlEncodedContent;

                while (bot.CheckoutDelay > bot.DelayStopwatch.ElapsedMilliseconds) { }

                checkoutResponse = HttpHelper.GetStringSync(request, client, out _, bot.GetCancellationToken());
                bot.RequestsList.Add(new Tuple<HttpRequestMessage, string>(request, checkoutResponse));
            }

            if (string.IsNullOrEmpty(checkoutResponse) || bot.GetCancellationToken().IsCancellationRequested) return;

            bot.CheckoutJObject = JObject.Parse(checkoutResponse);
            
            var checkoutStatus = bot.CheckoutJObject["status"].Value<string>();
            if (checkoutStatus.Equals("queued")) bot.CheckoutSlug = bot.CheckoutJObject["slug"].Value<string>();
            bot.Status = $"Checkout: {char.ToUpper(checkoutStatus[0]) + checkoutStatus.Substring(1)}";
            Debug.WriteLine("checkout response: " + checkoutResponse);
        }

        public override int Priority() {
            return 60;
        }

        public override string Description() {
            return "Checkout";
        }
    }
}
