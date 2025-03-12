using Y10_Tools.ViewModels.Pages;
using Wpf.Ui.Controls;
using Y10_Tools.Helpers;

namespace Y10_Tools.Views.Pages
{
    public partial class BootPage : INavigableView<BootViewModel>
    {
        public BootViewModel ViewModel { get; }

        public BootPage(BootViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            EventManager.ShowOverlayRequested += AddBootPageWarnings;
            EventManager.HideOverlayRequested += RemoveBootPageWarnings;

            if (ADBHelper.GetDevice() != null)
            {
                RemoveBootPageWarnings();
            }
        }
        public void RemoveBootPageWarnings()
        {
            ConnectDeviceOverlay.Visibility = Visibility.Hidden;
            ConnectDeviceOverlayBlur.Radius = 0;
        }

        public void AddBootPageWarnings()
        {
            ConnectDeviceOverlay.Visibility = Visibility.Visible;
            ConnectDeviceOverlayBlur.Radius = 15;
        }
    }
}
