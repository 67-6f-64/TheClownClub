using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Common;
using Common.Types;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace ClownAIO.Pages {
    /// <summary>
    /// Interaction logic for PropertyGrid.xaml
    /// </summary>
    public partial class ProfilesPage : UserControl {
        public ProfilesPage() {
            InitializeComponent();

            ProfilePropertyGrid.DataContext = new ProfileTaskGridViewModel();
            ProfilesListView.ItemsSource = BotContext.Profiles;
            BotContext.Profiles.CollectionChanged += (sender, args) => { ProfilesListView.ItemsSource = BotContext.Profiles; };
        }

        private void AddProfileButton_OnClick(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(FirstName.Text)
                || string.IsNullOrWhiteSpace(LastName.Text)) {
                MessageBox.Show("First/last name cannot be blank.");
                return;
            }

            try {
                var mailAddress = new MailAddress(Email.Text);
            }
            catch (FormatException) {
                MessageBox.Show("Invalid email.");
                return;
            }

            if (!Regex.Match(Phone.Text, @"\d\d\d-\d\d\d-\d\d\d\d").Success) {
                MessageBox.Show("Invalid phone number.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Address.Text)) {
                MessageBox.Show("Address cannot be blank.");
                return;
            }

            if (string.IsNullOrWhiteSpace(City.Text)) {
                MessageBox.Show("City cannot be blank.");
                return;
            }

            if (!Regex.Match(CcNumber.Text, @"\d\d\d\d-\d\d\d\d-\d\d\d\d-\d\d\d\d").Success) {
                MessageBox.Show("Invalid card number.");
                return;
            }

            if (!Regex.Match(CcCvv.Text, @"\d\d\d").Success) {
                MessageBox.Show("Invalid CVV.");
                return;
            }

            BotContext.Profiles.Add(ProfileName.Text, new BillingProfile(FirstName.Text, LastName.Text, Email.Text,
                Phone.Text,
                Address.Text, Address2.Text, Address3.Text, Zip.Text, City.Text, State.Text, Country.Text,
                CcType.Text.ToLower().Replace(" ", "_"), CcNumber.Text, CcExpir.Value ?? DateTime.Now, CcCvv.Text));
        }

        private void TaskListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            ((ProfileTaskGridViewModel) ProfilePropertyGrid.DataContext).BillingProfile = BotContext.Profiles.First().Value;
        }

        private void SaveProfileButton_OnClick(object sender, RoutedEventArgs e) {
            if (ProfilesListView.SelectedItems.Count <= 0) return;

            var sfd = new SaveFileDialog
                {Title = "Choose file to export profiles to...", Filter = "JSON|*.json|All Files|*"};
            var result = sfd.ShowDialog();
            if (result != true) return;

            try {
                File.WriteAllText(sfd.FileName, JsonConvert.SerializeObject(ProfilesListView.SelectedItems));
            }
            catch (Exception) {
                MessageBox.Show("Failed to save profiles.");
            }
        }

        private void LoadProfileButton_OnClick(object sender, RoutedEventArgs e) {
            var ofd = new OpenFileDialog
                {Title = "Open exported profiles...", Filter = "JSON|*.json|All Files|*"};
            var result = ofd.ShowDialog();
            if (result != true) return;

            try {
                var profiles =
                    JsonConvert.DeserializeObject<BillingProfile[]>(File.ReadAllText(ofd.FileName));

                foreach (var profile in profiles) {
                    BotContext.Profiles.Add(profile.Email, profile);
                }
            }
            catch (Exception) {
                MessageBox.Show("Failed to load profiles.");
            }
        }
    }

    public sealed class ProfileTaskGridViewModel : INotifyPropertyChanged {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private BillingProfile _billingProfile;

        public BillingProfile BillingProfile {
            get => _billingProfile;

            set {
                _billingProfile = value;
                OnPropertyChanged();
            }
        }
    }
}
