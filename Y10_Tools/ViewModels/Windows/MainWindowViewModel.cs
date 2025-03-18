using System;
using System.Collections.ObjectModel;
using Wpf.Ui.Controls;
using System.Windows.Media.Imaging;
using Y10_Tools.Helpers;

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
                Icon = new ImageIcon { Height = 24, Width = 24, Source = FilesHelper.CreateBitmapImage("control-panel.png", 24) },
                TargetPageType = typeof(Views.Pages.DashboardPage)
            },
            new NavigationViewItem()
            {
                Content = "Flash",
                Icon = new ImageIcon { Height = 24, Width = 24, Source = FilesHelper.CreateBitmapImage("pen-drive.png", 24) },
                TargetPageType = typeof(Views.Pages.FlashPage)
            },
            new NavigationViewItem()
            {
                Content = "Dump",
                Icon = new ImageIcon { Height = 24, Width = 24, Source = FilesHelper.CreateBitmapImage("save.png", 24) },
                TargetPageType = typeof(Views.Pages.DumpPage)
            },
            new NavigationViewItem()
            {
                Content = "Apps",
                Icon = new ImageIcon { Height = 24, Width = 24, Source = FilesHelper.CreateBitmapImage("downloads.png", 24) },
                TargetPageType = typeof(Views.Pages.AppsPage)
            },
            new NavigationViewItem()
            {
                Content = "Boot",
                Icon = new ImageIcon { Height = 24, Width = 24, Source = FilesHelper.CreateBitmapImage("create-new.png", 24) },
                TargetPageType = typeof(Views.Pages.BootPage)
            },
            new NavigationViewItem()
            {
                Content = "Root",
                Icon = new ImageIcon { Height = 24, Width = 24, Source = FilesHelper.CreateBitmapImage("administrator-male.png", 24) },
                TargetPageType = typeof(Views.Pages.RootPage)
            },
            new NavigationViewItem()
            {
                Content = "Recovery",
                Icon = new ImageIcon { Height = 24, Width = 24, Source = FilesHelper.CreateBitmapImage("doctors-bag.png", 24) },
                TargetPageType = typeof(Views.Pages.RecoveryPage)
            },
            new NavigationViewItem()
            {
                Content = "Misc",
                Icon = new ImageIcon { Height = 24, Width = 24, Source = FilesHelper.CreateBitmapImage("categorize.png", 24) },
                TargetPageType = typeof(Views.Pages.MiscPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<object> _footerMenuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Settings",
                Icon = new ImageIcon { Height = 24, Width = 24, Source = FilesHelper.CreateBitmapImage("gear.png", 24) },
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
