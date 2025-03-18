using Y10_Tools.ViewModels.Pages;
using Wpf.Ui.Controls;
using AdvancedSharpAdbClient.Models;
using AdvancedSharpAdbClient.Receivers;
using AdvancedSharpAdbClient.DeviceCommands;
using System.Text.RegularExpressions;
using Y10_Tools.Helpers;
using AdvancedSharpAdbClient;
using System.Net;
using Sentry.Protocol;

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
            dropDownADB.Icon = new ImageIcon {
                Source = FilesHelper.CreateBitmapImage("android-os.png", 24),
                Height = 24,
                Width = 24
            };

            refreshButton.Icon = new ImageIcon
            {
                Source = FilesHelper.CreateBitmapImage("refresh.png", 24),
                Height = 24,
                Width = 24
            };

            EventManager.DeviceUpdatedRequested += UpdateDeviceInfos;

            if (ADBHelper.GetDevice() != null)
            {
                UpdateDeviceInfos();
            }

            LoadDevices();
        }

        private void UpdateDeviceInfos()
        {
            if (ADBHelper.GetDevice() == null)
            {
                return;
            }
            try
            {
                UpdateBatteryLevel();
                UpdateSerialNumber();
                UpdateAndroidVersion();
                UpdateManufacturer();
                UpdateChipsetInfos();
                UpdatePackageNumber();
                // UpdateStorageAndMemory();
                // UpdateSELinuxStatus();
                // UpdateIP();
            } catch (Exception e)
            {
                SentrySdk.CaptureException(e);
            }
        }
        private async void UpdatePackageNumber()
        {
            List<string> packageList = await ADBHelper.GetPackagesList((DeviceData)ADBHelper.GetDevice());
            PackagesNLabel.Text = $"Number of packages: {packageList.Count}";
        }
        private async void UpdateChipsetInfos()
        {
            IShellOutputReceiver reciever = new ConsoleOutputReceiver();
            await ADBHelper.ADB.ExecuteShellCommandAsync((DeviceData)ADBHelper.GetDevice(), "cat /proc/cpuinfo", reciever);
            var chipset = GetLastValueOfLine("Hardware", reciever.ToString());
            ChipsetLabel.Text = "Chipset: " + chipset;

            IShellOutputReceiver receiver = new ConsoleOutputReceiver();
            await ADBHelper.ADB.ExecuteShellCommandAsync((DeviceData)ADBHelper.GetDevice(), "getprop ro.product.cpu.abi", receiver);
            string arch = receiver.ToString().Trim().Replace("\n", "");
            CpuArchLabel.Text = "CPU architecture: " + arch;
            if (arch.Contains("64"))
            {
                Cpu64.Visibility = Visibility.Visible;
                Cpu32.Visibility = Visibility.Hidden;
            } else
            {
                Cpu64.Visibility = Visibility.Hidden;
                Cpu32.Visibility = Visibility.Visible;
            }

            double temp = await ADBHelper.getTemp((DeviceData) ADBHelper.GetDevice(), "cpu") / 1000;

            CpuTempLabel.Text = $"CPU Temerature: {temp}°C";

        }
        private async void UpdateManufacturer()
        {
            var buildprops = await ADBHelper.RootShell((DeviceData)ADBHelper.GetDevice(), "cat /vendor/build.prop");
            if (buildprops != null)
            {
                string manufacturer = GetLastValueOfLine("ro.product.vendor.manufacturer=", buildprops.Replace("\r", "").Replace(" ", "").Trim().ToLower());
                ManuLabel.Text = "Manufacturer: " + manufacturer;
            }
        }
        private async void UpdateAndroidVersion()
        {
            IShellOutputReceiver receiver = new ConsoleOutputReceiver();
            await ADBHelper.ADB.ExecuteShellCommandAsync((DeviceData)ADBHelper.GetDevice(), "getprop ro.build.version.release", receiver);
            AndroidLabel.Text = "Android Version: " + receiver.ToString().Trim().Replace("\n", "");
        }
        private void UpdateSerialNumber()
        {
            DeviceData device = (DeviceData)ADBHelper.GetDevice();
            SerialNumberLabel.Text = $"Serial Number: {device.Serial}";
        }
        private async void UpdateBatteryLevel()
        {
            IShellOutputReceiver receiver = new ConsoleOutputReceiver();
            await ADBHelper.ADB.ExecuteShellCommandAsync((DeviceData)ADBHelper.GetDevice(), "dumpsys battery", receiver);
            string batteryLevelString = GetLastValueOfLine("level:", receiver.ToString().Replace(" ", "").Trim().ToLower());
            if (double.TryParse(batteryLevelString, out double value))
            {
                BatteryLevelBar.Value = value;
            }
            BatteryLevelLabel.Text = $"Battery Level: ({batteryLevelString}%) ";
        }

        ////////////////////////////////////////////////
        ///              Device selector             ///
        ////////////////////////////////////////////////

        private void RefreshDevices(object sender, RoutedEventArgs e)
        {
            EventManager.TriggerShowOverlay();
            devicesSelector.Items.Clear();
            dropDownADB.Content = "Select your device";
            SelectedDeviceADB = null;
            FastbootHelper.SetDevice(null);
            LoadDevices();
        }
        private async void LoadDevices()
        {
            devicesSelector.Items.Clear();

            var devices = await Task.Run(() => ADBHelper.ADB.GetDevices());
            foreach (FastbootHelper.FastbootDevice device in await FastbootHelper.GetDevices())
            {
                MenuItem deviceItem = new MenuItem
                {
                    Header = device.Serial + "(Fastboot) ⚠️",
                    ToolTip = "The device will be rebooted into the system to establish an ADB connection."
                };

                deviceItem.Click += async (s, e) =>
                {
                    deviceLoader.Visibility = Visibility.Visible;
                    refreshButton.IsEnabled = false;
                    devicesSelector.IsEnabled = false;

                    DeviceData? adbDevice = await FastbootHelper.FastbootDeviceToDeviceData(device);

                    if (adbDevice == null)
                    {
                        UiElementsHelper.MessBox("Error", "An error occured while trying to reboot the device.\nCheck that you were not flashing anything on it while rebooting\nCheck that if the device is locked, you unlocked it.");
                        deviceLoader.Visibility = Visibility.Hidden;
                        refreshButton.IsEnabled = true;
                        devicesSelector.IsEnabled = true;
                        return;
                    } else
                    {
                        devicesSelector.Items.Clear();
                        SelectDevice((DeviceData) adbDevice);
                        SelectedDeviceADB = null;
                        LoadDevices();
                    }
                };

                devicesSelector.Items.Add(deviceItem);
            }

            string devicesString = string.Join("\n", devices);
            foreach (DeviceData device in devices)
            {
                var Selectable = true;
                if (device.State != DeviceState.Online)
                {
                    Selectable = false;
                }
                MenuItem deviceItem = new MenuItem
                {
                    Header = device.Serial,
                    IsEnabled = Selectable
                };

                deviceItem.Click += (s, e) =>
                {
                    SelectDevice(device);
                };

                devicesSelector.Items.Add(deviceItem);
            }
        }
        private static string? GetLastValueOfLine(string line, string data)
        {
            var linefound = "";
            foreach (var a in data.Split('\n'))
            {
                if (a.StartsWith(line))
                {
                    linefound = a;
                }
            }
            if (linefound != "")
            {
                var match = Regex.Match(linefound, @"\b\w+\b(?=\W*$)");
                if (match.Success)
                {
                    return match.Value;
                } else
                {
                    return null;
                }
            }
            return null;
        }
        private async void SelectDevice(DeviceData device)
        {
            FastbootHelper.SetDevice(null);
            deviceLoader.Visibility = Visibility.Visible;
            refreshButton.IsEnabled = false;
            devicesSelector.IsEnabled = false;
            IShellOutputReceiver CheckMTKChipset = new ConsoleOutputReceiver();
            await ADBHelper.ADB.ExecuteShellCommandAsync(device, "cat /proc/cpuinfo", CheckMTKChipset);
            var chipset = GetLastValueOfLine("Hardware", CheckMTKChipset.ToString());

            if (chipset == null && !chipset.StartsWith("MT8")) {
                return; 
            } else if (!chipset.StartsWith("MT81")) {
                UiElementsHelper.MessBox("Unsupported Chipset", $"If you continue using this app with an unsuported tablet, you WILL run into unexpected problems. Please make sure you understand this and you may contact me on discord or github if you have an issue.\n\nYour tablet seems to have a {chipset}.");
            }

            dropDownADB.Content = device.Serial;
            SelectedDeviceADB = device;
            var buildprops = await ADBHelper.RootShell(device, "cat /vendor/build.prop");
            while (buildprops == "no.")
            {
                UiElementsHelper.MessBox("Device locked", "Your device seems to be locked, please start by unlocking it then pressing close.");
                await Task.Delay(10000);
                buildprops = await ADBHelper.RootShell(device, "cat /vendor/build.prop");
            }
            string bDate = null;
            if (buildprops != null)
            {
                bDate = GetLastValueOfLine("ro.vendor.build.date=", buildprops);

                if (bDate != null)
                {
                    if (bDate == "2019")
                    {
                        tabv = 0;
                    }
                    else if (bDate == "2020")
                    {
                        tabv = 1;
                    } else
                    {
                        bDate = null;
                    }
                }
                if (bDate == null)
                {
                    tabv = -1;
                    UiElementsHelper.MessBox("Unsupported version", $"If you continue using this app with an unsuported tablet, you WILL run into unexpected problems. Please make sure you understand this and you may contact me on discord or github if you have an issue.\n\nYour tablet seems to has been made in {bDate}.");
                }
            } else
            {
                tabv = -1;
            }

            EventManager.TriggerHideOverlay();
            EventManager.DeviceUpdated();
            refreshButton.IsEnabled = true;
            devicesSelector.IsEnabled = true;
            deviceLoader.Visibility = Visibility.Hidden;

            string serial = device.Serial;

            var checkDevice = Task.Run(async () =>
            {
                while (true)
                {
                    if (SelectedDeviceADB == null)
                    {
                        break;
                    }

                    var tempDevices = await Task.Run(() => ADBHelper.ADB.GetDevices().ToList());

                    if (!tempDevices.Contains((DeviceData)SelectedDeviceADB))
                    {
                        await Application.Current.Dispatcher.InvokeAsync(async () =>
                        {
                            dropDownADB.Content = "Select your device";
                            SelectedDeviceADB = null;
                            LoadDevices();
                            EventManager.DeviceUpdated();
                            EventManager.TriggerShowOverlay();
                            FastbootHelper.FastbootDevice? newDevice = await FastbootHelper.FindFastbootDeviceFromSerial(serial);
                            if (newDevice == null)
                            {
                                return;
                            }
                            FastbootHelper.SetDevice(newDevice);
                        });
                        break;
                    }

                    await Task.Delay(500);
                }
            });

            while (true)
            {
                if (SelectedDeviceADB == null)
                {
                    break;
                }

                UpdateDeviceInfos();

                await Task.Delay(10000);
            }
        }
    }
}
