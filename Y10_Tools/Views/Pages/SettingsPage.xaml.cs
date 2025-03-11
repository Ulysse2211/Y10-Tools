using Y10_Tools.ViewModels.Pages;
using Wpf.Ui.Controls;
using AdvancedSharpAdbClient.Models;
using static System.Net.Mime.MediaTypeNames;
using AdvancedSharpAdbClient.Receivers;
using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient;
using System.IO;
using System.Text.RegularExpressions;
using Y10_Tools.Helpers;

namespace Y10_Tools.Views.Pages
{
    public partial class SettingsPage : INavigableView<SettingsViewModel>
    {
        public SettingsViewModel ViewModel { get; }
        public static DeviceData? SelectedDeviceADB { get; private set; }
        public static int? tabv { get; private set; }

        public SettingsPage(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
            LoadDevices();
        }
        private void RefreshDevices(object sender, RoutedEventArgs e)
        {
            devicesSelector.Items.Clear();
            dropDownADB.Content = "Select your device";
            SelectedDeviceADB = null;
            LoadDevices();
        }
        private async void LoadDevices()
        {
            var app = (App)System.Windows.Application.Current;
            var devices = await Task.Run(() => app.ADB.GetDevices());

            string devicesString = string.Join("\n", devices);
            devicesSelector.Items.Clear();
            foreach (DeviceData device in devices)
            {
                MenuItem deviceItem = new MenuItem
                {
                    Header = device.Serial
                };

                deviceItem.Click += (s, e) =>
                {
                    SelectDevice(device);
                };

                devicesSelector.Items.Add(deviceItem);
            }
        }

        private async void SelectDevice(DeviceData device)
        {
            dropDownADB.Content = device.Serial;
            SelectedDeviceADB = device;
            var buildprops = await ADBShellHelper.RootShell(device, "cat /vendor/build.prop");
            string bDate = null;
            if (buildprops != null)
            {
                foreach (var line in buildprops.Split('\n'))
                {
                    if (line.StartsWith("ro.vendor.build.date"))
                    {
                        bDate = line;
                        break;
                    }
                }
                if (bDate != null)
                {
                    var match = Regex.Match(bDate, @"\d{4}");

                    if (match.Success)
                    {
                        if (match.Value == "2019")
                        {
                            tabv = 0;
                        }
                        else if (match.Value == "2020")
                        {
                            tabv = 1;
                        } else
                        {
                            tabv = -1;
                            System.Windows.MessageBox.Show($"If you continue using this app with an unsuported tablet, you WILL run into unexpected problems. Please make sure you understand this and you may contact me on discord or github if you have an issue.\n\n Your tablet seems to has been made in {match.Value}.", "\U0001faf7🏻 STOP !");
                        }
                    }
                    else
                    {
                        tabv = -1;
                        System.Windows.MessageBox.Show("If you continue using this app with an unsuported tablet, you WILL run into unexpected problems. Please make sure you understand this and you may contact me on discord or github if you have an issue.", "\U0001faf7🏻 STOP !");
                    }
                } else {
                    tabv = -1;
                    System.Windows.MessageBox.Show("If you continue using this app with an unsuported tablet, you WILL run into unexpected problems. Please make sure you understand this and you may contact me on discord or github if you have an issue.", "\U0001faf7🏻 STOP !");
                }
            }
        }
    }
}
