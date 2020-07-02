using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Common;
using Common.Supreme;
using Common.Types;
using Microsoft.Win32;
using Newtonsoft.Json;
using Supreme;

namespace ClownAIO.Pages {
    /// <summary>
    /// Interaction logic for TasksPage.xaml
    /// </summary>
    public partial class TasksPage : UserControl {
        public TasksPage() {
            InitializeComponent();

            TaskPropertyGrid.DataContext = new TaskGridViewModel();
            TaskListView.ItemsSource = BotContext.Bots;
            BotContext.Bots.CollectionChanged += (sender, args) => { TaskListView.ItemsSource = BotContext.Bots; };
            BillingCombo.ItemsSource = BotContext.Profiles;
            BotContext.Profiles.CollectionChanged += (sender, args) => { BillingCombo.ItemsSource = BotContext.Profiles; };
            BotType.ItemsSource = BotContext.BotTypes;
            BotContext.BotTypes.CollectionChanged += (sender, args) => { BotType.ItemsSource = BotContext.BotTypes; };
        }

        private void AddTaskButton_OnClick(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(ProductKeywords.Text)
                || BillingCombo.SelectedIndex.Equals(-1)) return;

            if (string.IsNullOrWhiteSpace(ColorKeywords.Text) && SizeCombo.Text.Equals("Any")) {
                BotContext.Bots.Add((Bot)Activator.CreateInstance(BotContext.BotTypes[Common.BotType.Supreme],
                    ((KeyValuePair<string, BillingProfile>) BillingCombo.SelectedItem).Value,
                    new SearchProduct(CategoryCombo.Text, ProductKeywords.Text.Trim().Split(',').ToList())));
            }
            else if (string.IsNullOrWhiteSpace(ColorKeywords.Text)) {
                BotContext.Bots.Add((Bot)Activator.CreateInstance(BotContext.BotTypes[Common.BotType.Supreme],
                    ((KeyValuePair<string, BillingProfile>) BillingCombo.SelectedItem).Value,
                    new SearchProduct(CategoryCombo.Text, ProductKeywords.Text.Trim().Split(',').ToList(),
                        SizeCombo.Text)));
            }
            else if (SizeCombo.Text.Equals("Any")) {
                BotContext.Bots.Add((Bot)Activator.CreateInstance(BotContext.BotTypes[Common.BotType.Supreme],
                    ((KeyValuePair<string, BillingProfile>) BillingCombo.SelectedItem).Value,
                    new SearchProduct(CategoryCombo.Text, ProductKeywords.Text.Trim().Split(',').ToList(),
                        ColorKeywords.Text.Trim().Split(',').ToList())));
            }
            else {
                BotContext.Bots.Add((Bot)Activator.CreateInstance(BotContext.BotTypes[Common.BotType.Supreme],
                    ((KeyValuePair<string, BillingProfile>) BillingCombo.SelectedItem).Value,
                    new SearchProduct(CategoryCombo.Text, ProductKeywords.Text.Trim().Split(',').ToList(),
                        ColorKeywords.Text.Trim().Split(',').ToList(), SizeCombo.Text)));
            }

            BotContext.Bots.Last().Proxy = ProxyAddress.Text;
            ((Supreme.SupremeBot) BotContext.Bots.Last()).CheckoutDelay = CheckoutDelaySlider.Value;
            ((Supreme.SupremeBot) BotContext.Bots.Last()).CaptchaBypass = CaptchaBypassCheckbox.IsChecked.Equals(true);
        }

        private void TaskListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            ((TaskGridViewModel) TaskPropertyGrid.DataContext).Bot = BotContext.Bots[TaskListView.SelectedIndex];
        }

        private void StartTaskButton_OnClick(object sender, RoutedEventArgs e) {
            foreach (var task in TaskListView.SelectedItems) {
                ((Bot)task).Execute();
            }
        }

        private void StopTaskButton_OnClick(object sender, RoutedEventArgs e) {
            foreach (var task in TaskListView.SelectedItems) {
                ((Bot)task).Abort();
            }
        }

        private void SaveTaskButton_OnClick(object sender, RoutedEventArgs e) {
            if (TaskListView.SelectedItems.Count <= 0) return;

            var sfd = new SaveFileDialog
                {Title = "Choose file to export tasks to...", Filter = "JSON|*.json|All Files|*"};
            var result = sfd.ShowDialog();
            if (result != true) return;

            try {
                File.WriteAllText(sfd.FileName, JsonConvert.SerializeObject(TaskListView.SelectedItems));
            }
            catch (Exception) {
                MessageBox.Show("Failed to save tasks.");
            }
        }

        private void LoadTaskButton_OnClick(object sender, RoutedEventArgs e) {
            var ofd = new OpenFileDialog
                {Title = "Open exported tasks file...", Filter = "JSON|*.json|All Files|*"};
            var result = ofd.ShowDialog();
            if (result != true) return;

            try {
                var bots =
                    JsonConvert.DeserializeObject<Bot[]>(File.ReadAllText(ofd.FileName), new BotConverter());

                foreach (var bot in bots) {
                    BotContext.Bots.Add(bot);
                }
            }
            catch (Exception) {
                MessageBox.Show("Failed to load tasks.");
            }
        }

        private void BotType_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            switch (((KeyValuePair<BotType, Type>)BotType.SelectedItem).Key) {
                case Common.BotType.Supreme: {
                    SupremeUs.Visibility = Visibility.Visible;
                    break;
                }
                default: {
                    SupremeUs.Visibility = Visibility.Hidden;
                    break;
                }
            }
        }
    }

    public class TaskGridViewModel : INotifyPropertyChanged {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private Bot _bot;

        public Bot Bot {
            get => _bot;

            set {
                _bot = value;
                OnPropertyChanged();
            }
        }
    }
}
