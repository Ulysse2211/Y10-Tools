using Y10_Tools.ViewModels.Pages;
using Wpf.Ui.Controls;
using Y10_Tools.Helpers;

namespace Y10_Tools.Views.Pages
{
    public partial class AppsPage : INavigableView<AppsViewModel>
    {
        public AppsViewModel ViewModel { get; }

        public AppsPage(AppsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            EventManager.ShowOverlayRequested += AddAppsPageWarnings;
            EventManager.HideOverlayRequested += RemoveAppsPageWarnings;

            if (ADBHelper.GetDevice() != null)
            {
                RemoveAppsPageWarnings();
            }
        }
        public void RemoveAppsPageWarnings()
        {
            ConnectDeviceOverlay.Visibility = Visibility.Hidden;
            ConnectDeviceOverlayBlur.Radius = 0;
        }

        public void AddAppsPageWarnings()
        {
            ConnectDeviceOverlay.Visibility = Visibility.Visible;
            ConnectDeviceOverlayBlur.Radius = 15;
        }

    }
}
