using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace Y10_Tools.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _applicationTitle = "Y10 Tools";

        [ObservableProperty]
        private ObservableCollection<object> _menuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Home",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 },
                TargetPageType = typeof(Views.Pages.DashboardPage)
            },
            new NavigationViewItem()
            {
                Content = "Flash",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Flash24 },
                TargetPageType = typeof(Views.Pages.FlashPage)
            },
            new NavigationViewItem()
            {
                Content = "Dump",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Save24 },
                TargetPageType = typeof(Views.Pages.DumpPage)
            },
            new NavigationViewItem()
            {
                Content = "Apps",
                Icon = new SymbolIcon { Symbol = SymbolRegular.AppsAddIn24 },
                TargetPageType = typeof(Views.Pages.AppsPage)
            },
            new NavigationViewItem()
            {
                Content = "Boot",
                Icon = new SymbolIcon { Symbol = SymbolRegular.DrawImage24 },
                TargetPageType = typeof(Views.Pages.BootPage)
            },
            new NavigationViewItem()
            {
                Content = "Root",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Braces24 },
                TargetPageType = typeof(Views.Pages.RootPage)
            },
            new NavigationViewItem()
            {
                Content = "Recovery",
                Icon = new SymbolIcon { Symbol = SymbolRegular.BriefcaseMedical24 },
                TargetPageType = typeof(Views.Pages.RecoveryPage)
            },
            new NavigationViewItem()
            {
                Content = "Misc",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Toolbox24 },
                TargetPageType = typeof(Views.Pages.MiscPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<object> _footerMenuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Settings",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Views.Pages.SettingsPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new()
        {
            new MenuItem { Header = "Home", Tag = "tray_home" }
        };
    }
}
