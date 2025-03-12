using Y10_Tools.ViewModels.Pages;
using Wpf.Ui.Controls;
using Y10_Tools.Helpers;

namespace Y10_Tools.Views.Pages
{
    public partial class RecoveryPage : INavigableView<RecoveryViewModel>
    {
        public RecoveryViewModel ViewModel { get; }

        public RecoveryPage(RecoveryViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            EventManager.ShowOverlayRequested += AddRecoveryPageWarnings;
            EventManager.HideOverlayRequested += RemoveRecoveryPageWarnings;

            if (ADBHelper.GetDevice() != null)
            {
                RemoveRecoveryPageWarnings();
            }
        }
        public void RemoveRecoveryPageWarnings()
        {
            ConnectDeviceOverlay.Visibility = Visibility.Hidden;
            ConnectDeviceOverlayBlur.Radius = 0;
        }
        public void AddRecoveryPageWarnings()
        {
            ConnectDeviceOverlay.Visibility = Visibility.Visible;
            ConnectDeviceOverlayBlur.Radius = 15;
        }
    }
}
