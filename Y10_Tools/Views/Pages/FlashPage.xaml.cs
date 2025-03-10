using Y10_Tools.ViewModels.Pages;
using Wpf.Ui.Controls;

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
        }
    }
}
