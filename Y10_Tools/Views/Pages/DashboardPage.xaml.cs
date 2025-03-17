using Y10_Tools.ViewModels.Pages;
using Wpf.Ui.Controls;
using Wpf.Ui.Appearance;
using Y10_Tools.Helpers;

namespace Y10_Tools.Views.Pages
{
    public partial class DashboardPage : INavigableView<DashboardViewModel>
    {
        public DashboardViewModel ViewModel { get; }

        public DashboardPage(DashboardViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
            EventManager.ShowOverlayRequested += ShowWarningMessage;
            EventManager.HideOverlayRequested += HideWarningMessage;

            if (ADBHelper.GetDevice() != null)
            {
                HideWarningMessage();
            }
        }

        private void ShowWarningMessage()
        {
            WarningConnectDevice.Visibility = Visibility.Visible;
        }

        private void HideWarningMessage()
        {
            WarningConnectDevice.Visibility = Visibility.Hidden;
        }

    }
}
