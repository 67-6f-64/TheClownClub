using System;
using System.Net;
using System.Net.Http;
using Common.Services;

namespace Common {
    class CheckoutTask : BotTask {
        public CheckoutTask(Bot bot) : base(bot) { }

        public override bool Validate() {
            var bot = (SupremeUsBot)GetBot();
            return !string.IsNullOrEmpty(bot.CsrfToken) && bot.CheckoutFormUrlEncodedContent != null;
        }

        public override void Execute() {
            var bot = (SupremeUsBot) GetBot();
            string checkoutResponse = null;
            HttpStatusCode? checkoutRequestStatus = null;

            using (var request = new HttpRequestMessage() {
                RequestUri = new Uri($"https://www.supremenewyork.com/checkout.json"),
                Method = HttpMethod.Post,
            }) {
                using (var client = new HttpClient(bot.GetClientHandler(), false)) {
                    request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
                    request.Headers.TryAddWithoutValidation("Accept", "*/*");
                    request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                    request.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
                    request.Headers.TryAddWithoutValidation("Refer", "https://www.supremenewyork.com/checkout");
                    request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                    request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.8,en-GB;q=0.6");
                    request.Headers.Add("X-CSRF-Token", bot.CsrfToken);
                    request.Content = bot.CheckoutFormUrlEncodedContent;

                    checkoutResponse = HttpHelper.GetStringSync(request, client, out checkoutRequestStatus, bot.GetCancellationToken());
                }
            }

            if (checkoutResponse is null) return;

            Console.WriteLine(checkoutResponse);
        }

        public override int Priority() {
            return 50;
        }

        public override string Description() {
            return "Checkout";
        }
    }
}
