using Y10_Tools.ViewModels.Pages;
using Wpf.Ui.Controls;
using Y10_Tools.Helpers;
using AdvancedSharpAdbClient.Models;

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
