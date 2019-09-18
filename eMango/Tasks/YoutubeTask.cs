using System;
using System.Threading.Tasks;
using YoutubeExplode;

namespace ActivityGen.Tasks {
    class YoutubeTask : BotTask {
        public async Task<BotTaskResult> Do(Bot bot) {
            try {
                bot.m_Status = "Browsing YouTube...";
                await bot.m_Page.GoToAsync("https://www.youtube.com/");
                await bot.m_Page.WaitForXPathAsync("//*[@id=\"video-title\"]");
                var videos = await bot.m_Page.XPathAsync("//*[@id=\"video-title\"]");
                int clickNum = Utils.RandomNumber(1, videos.Length);
                int i = 1;
                foreach (var video in videos) {
                    await video.HoverAsync();
                    await Task.Delay(Utils.RandomNumber(250, 2500));
                    if (i.Equals(clickNum)) {
                        await video.ClickAsync();
                        break;
                    }
                    i++;
                }
                await bot.m_Page.WaitForNavigationAsync();
            }
            catch (Exception) {
                return BotTaskResult.Failed;
            }

            TimeSpan duration = TimeSpan.FromMilliseconds(60000);
            try {
                var id = YoutubeClient.ParseVideoId(bot.m_Page.Url);
                var client = new YoutubeClient();
                var video = await client.GetVideoAsync(id);
                duration = video.Duration;
            }
            catch (Exception) { }
            bot.m_Status = "Watching YouTube...";
            await Task.Delay(duration);

            //if (Utils.RandomNumber(1, 1000) > 900) {
            //    try {
            //        await bot.m_Page.WaitForSelectorAsync("#subscribe-button");
            //        await bot.m_Page.ClickAsync("#subscribe-button");
            //        await bot.m_Page.WaitForXPathAsync("//button[starts-with(\"aria-label\", \"like this\")]");
            //        await bot.m_Page.XPathAsync("//button[starts-with(\"aria-label\", \"like this\")]").ContinueWith(t => t.Result.FirstOrDefault().ClickAsync());
            //        MessageBox.Show("liked and subscribed");
            //    }
            //    catch (Exception) { }
            //}
            //else if (Utils.RandomNumber(1, 100) > 30) {
            //    try {
            //        await bot.m_Page.WaitForXPathAsync("//button[starts-with(\"aria-label\", \"like this\")]");
            //        await bot.m_Page.XPathAsync("//button[starts-with(\"aria-label\", \"like this\")]").ContinueWith(t => t.Result.FirstOrDefault().ClickAsync());
            //        MessageBox.Show("liked");
            //    }
            //    catch (Exception) { }
            //}
            //else {
            //    try {
            //        await bot.m_Page.WaitForXPathAsync("//button[starts-with(\"aria-label\", \"dislike this\")]");
            //        await bot.m_Page.XPathAsync("//button[starts-with(\"aria-label\", \"dislike this\")]").ContinueWith(t => t.Result.FirstOrDefault().ClickAsync());
            //        MessageBox.Show("disliked");
            //    }
            //    catch (Exception) { }
            //}

            return BotTaskResult.Success;
        }
    }
}