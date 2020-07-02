using PuppeteerSharp;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ActivityGen.Tasks {
    class NewsTask : BotTask {
        public async Task<BotTaskResult> Do(Bot bot) {
            if (!bot.m_Page.Url.StartsWith("https://www.google.com/")) await bot.m_Page.GoToAsync("https://www.google.com/");
            try {
                bot.m_Status = "Browsing news...";
                await bot.m_Page.WaitForXPathAsync("//*[@id=\"gbwa\"]/div[1]/a");
                await bot.m_Page.XPathAsync("//*[@id=\"gbwa\"]/div[1]/a").ContinueWith(t => t.Result.FirstOrDefault().ClickAsync());

                await bot.m_Page.WaitForXPathAsync("//*[@id=\"gb5\"]/span[1]", new WaitForSelectorOptions { Visible = true });
                await bot.m_Page.XPathAsync("//*[@id=\"gb5\"]/span[1]").ContinueWith(t => t.Result.FirstOrDefault().ClickAsync());

                await bot.m_Page.WaitForNavigationAsync(new NavigationOptions { Timeout = 5000 });

                var articles = await bot.m_Page.QuerySelectorAllAsync("article");
                Array.Resize(ref articles, Utils.RandomNumber(1, 10));
                foreach (var article in articles) {
                    await article.HoverAsync();
                    await Task.Delay(Utils.RandomNumber(3000, 10000));
                    if (Utils.RandomNumber(0, 1000) > 800) {
                        bot.m_Status = "Reading article...";
                        await article.ClickAsync();
                        await Task.Delay(Utils.RandomNumber(10000, 90000));
                        bot.m_Status = "Browsing news...";
                    }
                }

                return BotTaskResult.Success;
            }
            catch (Exception) {
                return BotTaskResult.Failed;
            }
        }
    }
}
