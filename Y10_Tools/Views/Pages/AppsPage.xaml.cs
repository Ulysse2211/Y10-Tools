using Y10_Tools.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace Y10_Tools.Views.Pages
{
    public partial class MiscPage : INavigableView<MiscViewModel>
    {
        public MiscViewModel ViewModel { get; }

        public MiscPage(MiscViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
