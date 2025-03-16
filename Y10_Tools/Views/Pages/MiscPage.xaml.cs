using Y10_Tools.ViewModels.Pages;
using Wpf.Ui.Controls;
using Y10_Tools.Helpers;
using System.Threading.Tasks;
using AdvancedSharpAdbClient.Models;

namespace Y10_Tools.Views.Pages
{
    public partial class MiscPage : INavigableView<MiscViewModel>
    {
        public MiscViewModel ViewModel { get; }

        public MiscPage(MiscViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            EventManager.ShowOverlayRequested += AddMiscPageWarnings;
            EventManager.HideOverlayRequested += RemoveMiscPageWarnings;

            EventManager.DeviceUpdatedRequested += UpdateProps;

            if (ADBHelper.GetDevice() != null)
            {
                RemoveMiscPageWarnings();
                UpdateProps();
            }
        }

        public void RemoveMiscPageWarnings()
        {
            ConnectDeviceOverlay.Visibility = Visibility.Hidden;
            ConnectDeviceOverlayBlur.Radius = 0;
        }

        public void AddMiscPageWarnings()
        {
            ConnectDeviceOverlay.Visibility = Visibility.Visible;
            ConnectDeviceOverlayBlur.Radius = 15;
        }

        public async void UpdateProps()
        {
            var device = ADBHelper.GetDevice();
            if (device == null)
            {
                dBProps.Content = "Why would you look at that when the device isn't even pluged in ?";
                dDProps.Content = "Why would you look at that when the device isn't even pluged in ?";
                return;
            }
            dBProps.Content = await ADBHelper.RootShell((DeviceData)device, "cat /vendor/build.prop");
            dDProps.Content = await ADBHelper.RootShell((DeviceData)device, "cat /vendor/default.prop");
        }

        public async void RebootToFB(object sender, RoutedEventArgs e)
        {
            var device = ADBHelper.GetDevice();
            if (device == null)
            {
                return;
            }
            FastbootHelper.FastbootDevice? newDevice =  await ADBHelper.ADBToFastboot((DeviceData)device);
            if (newDevice == null)
            {
                return;
            }
            FastbootHelper.SetDevice(newDevice);
        }

        public void RebootToRC(object sender, RoutedEventArgs e)
        {
            var device = ADBHelper.GetDevice();
            if (device == null)
            {
                return;
            }
            ADBHelper.RootShell((DeviceData)device, "reboot recovery");
        }

        public void Reboot(object sender, RoutedEventArgs e)
        {
            var device = ADBHelper.GetDevice();
            if (device == null)
            {
                return;
            }
            ADBHelper.RootShell((DeviceData)device, "reboot system");
        }

        public void Shutdown(object sender, RoutedEventArgs e)
        {
            var device = ADBHelper.GetDevice();
            if (device == null)
            {
                return;
            }
            ADBHelper.RootShell((DeviceData)device, "reboot -p");
        }
    }
}
