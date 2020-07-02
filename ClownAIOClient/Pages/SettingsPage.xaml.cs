using System;
using System.Windows;
using System.Windows.Controls;
using Common.Services;

namespace ClownAIO.Pages {
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl {
        public SettingsPage() {
            InitializeComponent();
        }

        private void RefreshSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            SupremeMonitor.RefreshInterval = TimeSpan.FromMilliseconds(e.NewValue);
        }

        private void StartMonitorButton_OnClick(object sender, RoutedEventArgs e) {
            try {
                SupremeMonitor.SetProxy(MonitorProxy.Text);
            }
            catch (Exception) {
                MessageBox.Show("Failed to set proxy. Invalid format?");
            }

            SupremeMonitor.Start();
        }

        private void StopMonitorButton_OnClick(object sender, RoutedEventArgs e) {
            try {
                SupremeMonitor.Stop();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
