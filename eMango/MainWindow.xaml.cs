using DiscordRPC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ActivityGen {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        bool m_IsRunning = false;
        public static int m_TaskDelay = 10000;
        public static int m_LoginDelay = 10000;
        public static ObservableCollection<Bot> m_Bots = new ObservableCollection<Bot>();
        DiscordRpcClient m_DiscordClient;
        CancellationTokenSource m_TokenSource = new CancellationTokenSource();
        System.Timers.Timer m_UpdateTimer = new System.Timers.Timer(1000);

        public MainWindow() {
            InitializeComponent();

            #region Discord RPC
            m_DiscordClient = new DiscordRpcClient("620789728958087178");

            //Set the logger
            //client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

            //Subscribe to events
            //client.OnReady += (sender, e) =>
            //{
            //    Console.WriteLine("Received Ready from user {0}", e.User.Username);
            //};
            //
            //client.OnPresenceUpdate += (sender, e) =>
            //{
            //    Console.WriteLine("Received Update! {0}", e.Presence);
            //};

            m_DiscordClient.Initialize();
            m_DiscordClient.SetPresence(new RichPresence() {
                Details = "You a clown.",
                State = "Generating one clicks",
                Assets = new Assets() {
                    LargeImageKey = "clown",
                    LargeImageText = "TheClown.Club",
                    SmallImageKey = "clown"
                }
            });
            #endregion

            m_Bots.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) => {
                lvBots.ItemsSource = m_Bots;
            };

            m_UpdateTimer.Elapsed += (object source, ElapsedEventArgs e) => {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, 
                    new Action(() => { lvBots.ItemsSource = m_Bots; lvBots.Items.Refresh(); }));
            };
            m_UpdateTimer.Start();
        }

        private async void Start_Click(object sender, RoutedEventArgs e) {
            //var bot = new Bot(txtEmail.Text, txtPassword.Password); // https://bot.sannysoft.com
            //await bot.Init();
            //await bot.Run(new NewsTask());
            //await bot.Run(new YoutubeTask());
            //await bot.m_Page.ScreenshotAsync("headless.png");
            //await bot.m_Page.PdfAsync("headless.pdf");
            //MessageBox.Show("done.");
            if (m_IsRunning)
                return;
            m_TokenSource = new CancellationTokenSource();

            var tasks = new List<Task>();

            for (var i = 0; i < m_Bots.Count; i++) {
                int index = i;
                tasks.Add(Task.Run(async () => {
                    await m_Bots[index].Init();
                    if (m_Bots[index].m_Status.StartsWith("Signed in.")) {
                        await m_Bots[index].Run(m_TokenSource.Token);
                    }
                }));
            }

            m_IsRunning = true;
            await Task.WhenAll(tasks);
        }

        private void Stop_Click(object sender, RoutedEventArgs e) {
            if (!m_IsRunning)
                return;
            m_TokenSource.Cancel();
            m_IsRunning = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            try {
                MailAddress m = new MailAddress(txtEmail.Text);
            }
            catch (FormatException) {
                MessageBox.Show("Invalid email.");
                return;
            }

            if (!string.IsNullOrEmpty(txtProxy.Text) && 
                !Regex.IsMatch(txtProxy.Text, @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5]):[\d]+$")) {
                MessageBox.Show("Invalid proxy.");
                return;
            }

            m_Bots.Add(new Bot(txtEmail.Text, txtPassword.Password, txtProxy.Text));
        }

        private void SliderDelay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            int newVal = Convert.ToInt32(e.NewValue);
            m_TaskDelay = newVal * 1000;
            if (lblDelay != null && lblDelay.IsLoaded)
                lblDelay.Content = newVal;
        }

        private void SliderLoginDelay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            int newVal = Convert.ToInt32(e.NewValue);
            m_LoginDelay = newVal * 1000;
            if (lblDelaySignin != null && lblDelaySignin.IsLoaded)
                lblDelaySignin.Content = newVal;
        }

        private async void Screenshot_Click(object sender, RoutedEventArgs e) {
            if (lvBots.SelectedIndex == -1 || m_Bots[lvBots.SelectedIndex].m_Page == null)
                return;

            var img = await m_Bots[lvBots.SelectedIndex].m_Page.ScreenshotDataAsync();
            using (var ms = new MemoryStream(img)) {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                imgScreenshot.Source = image;
            }
        }

        private void Captcha_Click(object sender, RoutedEventArgs e) {
            if (lvBots.SelectedIndex == -1 || m_Bots[lvBots.SelectedIndex].m_Page == null)
                return;

            if (m_Bots[lvBots.SelectedIndex].m_CatpchaImage != null) {
                using (var ms = new MemoryStream(m_Bots[lvBots.SelectedIndex].m_CatpchaImage)) {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    imgScreenshot.Source = image;
                }
            }
            else {
                MessageBox.Show("null");
            }
        }

        private void SubmitCaptcha_Click(object sender, RoutedEventArgs e) {
            if (lvBots.SelectedIndex == -1 || m_Bots[lvBots.SelectedIndex].m_Page == null)
                return;

            if (!string.IsNullOrWhiteSpace(txtCaptcha.Text)) {
                m_Bots[lvBots.SelectedIndex].m_CaptchaText = txtCaptcha.Text;
            }
        }
    }
}
