using Y10_Tools.ViewModels.Pages;
using Wpf.Ui.Controls;
using Y10_Tools.Helpers;
using AdvancedSharpAdbClient.Models;
using System.Configuration;
using System.Windows.Controls;

namespace Y10_Tools.Views.Pages
{
    public partial class DumpPage : INavigableView<DumpViewModel>
    {
        public DumpViewModel ViewModel { get; }

        public DumpPage(DumpViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            EventManager.ShowOverlayRequested += AddDumpPageWarnings;
            EventManager.HideOverlayRequested += RemoveDumpPageWarnings;

            if (ADBHelper.GetDevice() != null)
            {
                RemoveDumpPageWarnings();
            }
        }

        private async void StartDump(object sender, RoutedEventArgs e)
        {
            pList.IsEnabled = false;
            dButton.IsEnabled = false;
            List<Task> tasks = new List<Task>();

            foreach (UIElement partition in pList.Children)
            {
                if (partition is CheckBox checkBox && checkBox.IsChecked == true)
                {
                    string partitionName = checkBox.Content.ToString().Trim().ToLower();
                    var task = ADBHelper.RootShell((DeviceData)ADBHelper.GetDevice(), $"dd if=/dev/block/by-name/{partitionName} of=/data/local/tmp/{partitionName}.img");
                    tasks.Add(task);
                }
            }

            await Task.WhenAll(tasks);
            pList.IsEnabled = true;
            dButton.IsEnabled = true;
        }

        public void RemoveDumpPageWarnings()
        {
            ConnectDeviceOverlay.Visibility = Visibility.Hidden;
            ConnectDeviceOverlayBlur.Radius = 0;
        }

        public void AddDumpPageWarnings()
        {
            ConnectDeviceOverlay.Visibility = Visibility.Visible;
            ConnectDeviceOverlayBlur.Radius = 15;
        }
    }
}
