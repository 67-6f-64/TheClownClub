using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Reflection;
using System.Runtime.Loader;
using FirstFloor.ModernUI.Windows.Controls;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Common;
using Newtonsoft.Json;
using NotLiteCode.Client;
using NotLiteCode.Network;
using NotLiteCode.Serialization;

namespace ClownAIO {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : ModernWindow {
        public static async Task<bool> Auth(string authKey, string hwid) =>
            await Globals.Client.RemoteCall<bool>("Auth", authKey, hwid);

        public LoginWindow() {
            InitializeComponent();
            Globals.Client.Connect("localhost", 1338);
        }

        private async void Button_Click(object sender, RoutedEventArgs e) {
            var authKey = AuthKey.Text.Trim();
            var authed = await Auth(authKey, null);
            if (!authed) return;

            new LoadingWindow().Show();

            Close();
        }
    }
}
