using Y10_Tools.ViewModels.Pages;
using Wpf.Ui.Controls;
using AdvancedSharpAdbClient.Models;
using static System.Net.Mime.MediaTypeNames;
using AdvancedSharpAdbClient.Receivers;
using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient;
using System.IO;
using System.Text.RegularExpressions;

namespace Y10_Tools.Views.Pages
{
    public partial class SettingsPage : INavigableView<SettingsViewModel>
    {
        public SettingsViewModel ViewModel { get; }
        public DeviceData? SelectedDeviceADB { get; private set; }
        public int? tabv { get; private set; }

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

        private async Task<string?> RootShell(DeviceData device, string command)
        {
            var app = (App)System.Windows.Application.Current;
            var client = app.ADB;

            using (SyncService service = new SyncService(device))
            {
                using (FileStream stream = File.OpenRead("Assets/Exploit/mtk-su"))
                {
                    await service.PushAsync(stream, "/data/local/tmp/mtk-su", UnixFileStatus.DefaultFileMode, DateTimeOffset.Now, null);
                }
            }
            IShellOutputReceiver receiver = new ConsoleOutputReceiver();
            string createScript = $"echo -e \"#!/system/bin/sh\n{command}\" > /data/local/tmp/y10tempCommandRun.sh && chmod 755 /data/local/tmp/y10tempCommandRun.sh";
            await client.ExecuteShellCommandAsync(device, $@"chmod 755 /data/local/tmp/mtk-su && {createScript} && /data/local/tmp/mtk-su -c '/data/local/tmp/y10tempCommandRun.sh' && rm /data/local/tmp/y10tempCommandRun.sh", receiver);

            return receiver.ToString();
        }

        private async void SelectDevice(DeviceData device)
        {
            dropDownADB.Content = device.Serial;
            SelectedDeviceADB = device;
            var buildprops = await RootShell(device, "cat /vendor/build.prop");
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
                            System.Windows.MessageBox.Show("If you continue using this device on an unsuported tablet, you WILL run into unexpected problems. Please make sure you understand this and you may contact me on discord or github if you have an issue.", "\U0001faf7🏻 STOP !");
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("caca");
                    }
                }
            }
        }
    }
}
