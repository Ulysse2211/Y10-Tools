using Y10_Tools.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace Y10_Tools.Views.Pages
{
    public partial class RootPage : INavigableView<RootViewModel>
    {
        public RootViewModel ViewModel { get; }

        public RootPage(RootViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
