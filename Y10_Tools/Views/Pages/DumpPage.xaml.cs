using Y10_Tools.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace Y10_Tools.Views.Pages
{
    public partial class DumpPage : INavigableView<DumpViewModel>
    {
        public DumpViewModel ViewModel { get; }

        public DumpPage(DumpViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
