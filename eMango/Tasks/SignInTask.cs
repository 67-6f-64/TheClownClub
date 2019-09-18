using PuppeteerSharp;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ActivityGen.Tasks {
    class SignInTask : BotTask {
        public async Task<BotTaskResult> Do(Bot bot) {
            bot.m_Status = "Signing into Google...";
            try {
                await bot.m_Page.GoToAsync("https://www.google.com/");
                await bot.m_Page.WaitForXPathAsync("//*[@id=\"gb_70\"]");
                await bot.m_Page.XPathAsync("//*[@id=\"gb_70\"]").ContinueWith(t => t.Result.FirstOrDefault().ClickAsync());
                await bot.m_Page.WaitForSelectorAsync("input[type=\"email\"]");
                await bot.m_Page.TypeAsync("input[type=\"email\"]", bot.m_Email);
                await bot.m_Page.Keyboard.PressAsync("Enter");
                await bot.m_Page.WaitForSelectorAsync("input[type=\"password\"]", new WaitForSelectorOptions { Visible = true });
                await bot.m_Page.TypeAsync("input[type=\"password\"]", bot.m_Password);
                await bot.m_Page.Keyboard.PressAsync("Enter");
                await bot.m_Page.WaitForNavigationAsync();
                try {
                    do {
                        await bot.m_Page.WaitForXPathAsync("//*[@id=\"captchaimg\"]", new WaitForSelectorOptions { Timeout = 3000 });
                        //var captchaimgxpath = await bot.m_Page.XPathAsync("//*[@id=\"captchaimg\"]");
                        //var captchaimg = captchaimgxpath.FirstOrDefault();
                        //var bbox = await captchaimg.BoundingBoxAsync();
                        //bot.m_CatpchaImage = await bot.m_Page.ScreenshotDataAsync(new ScreenshotOptions { Clip = new PuppeteerSharp.Media.Clip { Height = bbox.Height, Width = bbox.Width, X = bbox.X, Y = bbox.Y } });
                        bot.m_Status = "Waiting for Captcha...";
                        while (string.IsNullOrWhiteSpace(bot.m_CaptchaText)) { await Task.Delay(1000); }
                        var xpaths = await bot.m_Page.XPathAsync("//*[@id=\"ca\"]");
                        await xpaths.FirstOrDefault().TypeAsync(bot.m_CaptchaText);
                        bot.m_CaptchaText = null;
                        await bot.m_Page.TypeAsync("input[type=\"password\"]", bot.m_Password);
                        await bot.m_Page.Keyboard.PressAsync("Enter");
                        await bot.m_Page.WaitForNavigationAsync();
                        try {
                            await bot.m_Page.WaitForXPathAsync("//*[@id=\"gb_71\"]", new WaitForSelectorOptions { Timeout = 5000 });
                            return BotTaskResult.Success;
                        }
                        catch (Exception) { }
                    } while (true);
                }
                catch (Exception) { }
                await bot.m_Page.WaitForXPathAsync("//*[@id=\"gb_71\"]", new WaitForSelectorOptions { Timeout = 5000 });
                return BotTaskResult.Success;
            }
            catch (Exception) {
                return BotTaskResult.Failed;
            }
        }
    }
}
