using Y10_Tools.ViewModels.Pages;
using Wpf.Ui.Controls;
using Y10_Tools.Helpers;

namespace Y10_Tools.Views.Pages
{
    public partial class FlashPage : INavigableView<FlashViewModel>
    {
        public FlashViewModel ViewModel { get; }

        public FlashPage(FlashViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            EventManager.ShowOverlayRequested += AddFlashPageWarnings;
            EventManager.HideOverlayRequested += RemoveFlashPageWarnings;

            if (ADBHelper.GetDevice() != null)
            {
                RemoveFlashPageWarnings();
            }
        }
        public void RemoveFlashPageWarnings()
        {
            ConnectDeviceOverlay.Visibility = Visibility.Hidden;
            ConnectDeviceOverlayBlur.Radius = 0;
        }
        public void AddFlashPageWarnings()
        {
            ConnectDeviceOverlay.Visibility = Visibility.Visible;
            ConnectDeviceOverlayBlur.Radius = 15;
        }
    }
}
