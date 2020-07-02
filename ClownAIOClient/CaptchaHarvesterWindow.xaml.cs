using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CefSharp;
using Common.Services;
using CefSharp.Wpf;
using ClownAIO.Classes;

namespace ClownAIO {
    /// <summary>
    /// Interaction logic for CaptchaHarvesterWindow.xaml
    /// </summary>
    public partial class CaptchaHarvesterWindow : ModernWindow {
        private string Domain { get; }
        private string SiteKey { get; }
        public CaptchaHarvesterWindow(string domain, string sitekey) {
            InitializeComponent();
            Domain = domain;
            SiteKey = sitekey;
            Title = Domain;
            ChromiumBrowser.Address = $"https://{Domain}/recaptcha";
            ChromiumBrowser.MenuHandler = new NoContextMenuHandler();
            ChromiumBrowser.RequestHandler = new CaptchaRequestHandler(domain, sitekey);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            ChromiumBrowser.Load($"https://{Domain}/recaptcha");
        }

        private void Button2_Click(object sender, RoutedEventArgs e) {
            ChromiumBrowser.Load("https://www.google.com/");
        }

        private void ModernWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            CaptchaHarvester.RemoveWindow(SiteKey);
        }
    }
}
