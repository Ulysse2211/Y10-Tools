using Y10_Tools.ViewModels.Pages;
using Wpf.Ui.Controls;
using Wpf.Ui.Appearance;

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
        }
    }
}
