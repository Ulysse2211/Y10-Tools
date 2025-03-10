using Y10_Tools.ViewModels.Pages;
using Wpf.Ui.Controls;

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
        }
    }
}
