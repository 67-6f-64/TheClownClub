using ClownScript;
using HtmlAgilityPack;
using SupremeBot.Services;
using SupremeBot.Templates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Size = SupremeBot.Templates.Size;
using Style = SupremeBot.Templates.Style;
using CSScript = System.Func<ClownScript.CSManager, int>;
using Common;

namespace SupremeBot {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancelToken = cancellationTokenSource.Token;
            var searchProduct = new Common.Supreme.SearchProduct(txtKeywords.Text.Split(',').ToList());
            Console.WriteLine(@"---- keywords ----");
            foreach (var keyword in searchProduct.ProductKeywords) {
                Console.WriteLine(keyword);
            }
            Console.WriteLine(@"---- keywords ----");
            var bot = new SupremeUsBot(searchProduct, cancelToken);
            var funTimer = Stopwatch.StartNew();
            bot.Execute();
            funTimer.Stop();
            Console.WriteLine(funTimer.ElapsedMilliseconds.ToString());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            
        }
    }
}
