using Y10_Tools.ViewModels.Pages;
using Wpf.Ui.Controls;

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
        }
    }
}
