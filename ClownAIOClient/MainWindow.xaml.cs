using System;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;
using Common.Services;
using FirstFloor.ModernUI.Windows.Controls;

namespace ClownAIO {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow {
        public MainWindow() {
            if (Globals.Client is null) Environment.Exit(0);
            Cef.Initialize(new CefSettings());
            Task.Run(async () => {
                while (true) {
                    await CaptchaHarvester.RequestWindowChannel.Reader.WaitToReadAsync();
                    var (domain, sitekey) = await CaptchaHarvester.RequestWindowChannel.Reader.ReadAsync();
                    if (string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(sitekey)) continue;
                    await CaptchaHarvester.ReceiveWindowChannel.Writer.WaitToWriteAsync();
                    Application.Current.Dispatcher.Invoke(() => {
                        var window = new CaptchaHarvesterWindow(domain, sitekey);
                        window.Show();
                        CaptchaHarvester.ReceiveWindowChannel.Writer.WriteAsync(window);
                    });
                }
            });
            CaptchaHarvester.WindowType = typeof(CaptchaHarvesterWindow);
            InitializeComponent();
        }
    }
}
