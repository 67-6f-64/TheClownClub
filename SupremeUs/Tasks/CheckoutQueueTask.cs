using System;
using System.Net;
using System.Net.Http;
using Common;
using Common.Services;
using Newtonsoft.Json.Linq;

namespace Supreme.Tasks {
    class CheckoutQueueTask : BotTask {
        public CheckoutQueueTask(Bot bot) : base(bot) { }

        public override bool Validate() {
            var bot = (SupremeBot)GetBot();
            return !string.IsNullOrEmpty(bot.CheckoutSlug) && !bot.GetCancellationToken().IsCancellationRequested;
        }

        public override void Execute() {
            var bot = (SupremeBot)GetBot();
            do {
                string slugResponse;

                using (var request = new HttpRequestMessage() {
                    RequestUri = new Uri($"https://www.supremenewyork.com/checkout/{bot.CheckoutSlug}/status.json"),
                    Method = HttpMethod.Get,
                    Version = new Version(2, 0)
                }) {
                    using var client = new HttpClient(bot.GetClientHandler(), false);
                    request.Headers.TryAddWithoutValidation("User-Agent",
                        "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                    request.Headers.TryAddWithoutValidation("Accept", "*/*");
                    request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                    request.Headers.TryAddWithoutValidation("Refer", "https://www.supremenewyork.com/checkout");
                    request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                    request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.8,en-GB;q=0.6");
                    request.Headers.Add("X-CSRF-Token", bot.CsrfToken);

                    slugResponse = HttpHelper.GetStringSync(request, client, out _,
                        bot.GetCancellationToken());
                    bot.RequestsList.Add(new Tuple<HttpRequestMessage, string>(request, slugResponse));
                }

                if (string.IsNullOrEmpty(slugResponse) || bot.GetCancellationToken().IsCancellationRequested) return;

                bot.CheckoutJObject = JObject.Parse(slugResponse);
                var checkoutStatus = bot.CheckoutJObject["status"].Value<string>();
                bot.Status = $"Checkout: {char.ToUpper(checkoutStatus[0]) + checkoutStatus.Substring(1)}";
            } while (bot.CheckoutJObject["status"].Value<string>()
                .Equals("queued", StringComparison.CurrentCultureIgnoreCase));
        }

        public override int Priority() {
            return 70;
        }

        public override string Description() {
            return GetBot().Status;
        }
    }
}
