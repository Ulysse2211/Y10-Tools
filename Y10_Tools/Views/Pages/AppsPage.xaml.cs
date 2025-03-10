using Y10_Tools.ViewModels.Pages;
using Wpf.Ui.Controls;

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
        }
    }
}
