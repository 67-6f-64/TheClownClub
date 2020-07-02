using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Windows;
using Common;
using Common.Services;
using Newtonsoft.Json;
using PuppeteerSharp;

namespace ClownAIO {
    /// <summary>
    /// Interaction logic for LoadingDialog.xaml
    /// </summary>
    public partial class LoadingWindow : Window {
        public LoadingWindow() {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            var assemblyQueue = JsonConvert.DeserializeObject<Queue<byte[]>>(await Globals.Client.RemoteCall<string>("GetCommon"));

            var commonAssembly = AssemblyLoadContext.Default.LoadFromStream(
                new MemoryStream(assemblyQueue.Dequeue()));

            var botType = commonAssembly.GetExportedTypes().Single(t => t.Name.Equals("Bot"));

            foreach (var assembly in assemblyQueue) {
                var loadedAssembly = AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(assembly));
                foreach (var type in loadedAssembly.GetExportedTypes()
                    .Where(t => t.IsSubclassOf(botType))) {
                    var bot = (Bot)Activator.CreateInstance(type);
                    BotContext.BotTypes.Add(bot.BotType, type);
                }
            }

            new MainWindow().Show();

            Close();
        }
    }
}
