using Y10_Tools.ViewModels.Pages;
using Wpf.Ui.Controls;
using Y10_Tools.Helpers;
using System;
using System.IO;
using System.Text.Json;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Resources;
using System.Windows.Controls;
using System.Net.Http;
using AdvancedSharpAdbClient.Models;
using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.Receivers;
using AdvancedSharpAdbClient.Exceptions;
using System.Security.Policy;

namespace Y10_Tools.Views.Pages
{
    public class App
    {
        public string name { get; set; }
        public string description { get; set; }
        public string id { get; set; }
        public string icon { get; set; }
        public string provider { get; set; }
        public string? github { get; set; }
        public string? gitlab { get; set; }
        public string? direct { get; set; }
    }

    public partial class AppsPage : INavigableView<AppsViewModel>
    {
        public AppsViewModel ViewModel { get; }

        public AppsPage(AppsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            EventManager.ShowOverlayRequested += AddAppsPageWarnings;
            EventManager.HideOverlayRequested += RemoveAppsPageWarnings;
            
            EventManager.DeviceUpdatedRequested += populateApps;

            if (ADBHelper.GetDevice() != null)
            {
                RemoveAppsPageWarnings();
                populateApps();
            }
        }

        private async void populateApps()
        {
            List<string> packageList = [];
            var device = ADBHelper.GetDevice();
            if (device != null)
            {
                var packageListText = "";
                IShellOutputReceiver receiver = new ConsoleOutputReceiver();

                await ADBHelper.ADB.ExecuteShellCommandAsync((DeviceData)device, "pm list packages", receiver);
                
                packageListText = receiver.ToString();
                packageListText = packageListText.Replace("package:", "");
                packageList = packageListText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                             .Select(p => p.Trim())
                                             .ToList();
            }
            AppsStackPannel.Children.Clear();

            Uri uri = new Uri("pack://application:,,,/Assets/Apps/Apps.json");
            StreamResourceInfo resourceInfo = Application.GetResourceStream(uri);
            string AppsJson = "";

            using (StreamReader reader = new StreamReader(resourceInfo.Stream))
            {
                AppsJson = reader.ReadToEnd();
            }

            List<App> AppsList = JsonSerializer.Deserialize<List<App>>(AppsJson);

            foreach (App app in AppsList)
            {
                var card = new CardControl
                {
                    Margin = new Thickness(10),
                    VerticalAlignment = VerticalAlignment.Top,
                };

                var stackPanel = new StackPanel();

                var Icon = new Wpf.Ui.Controls.Image
                {
                    Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(app.icon, UriKind.RelativeOrAbsolute)),
                    CornerRadius = new CornerRadius(5),
                    Margin = new Thickness(0,0,10, 0),
                };

                var headerTextBlock = new Wpf.Ui.Controls.TextBlock
                {
                    Text = app.name,
                    FontTypography = FontTypography.BodyStrong,
                    Foreground = (System.Windows.Media.Brush)FindResource("TextFillColorPrimaryBrush")
                };

                var descriptionTextBlock = new Wpf.Ui.Controls.TextBlock
                {
                    Text = app.description,
                    FontTypography = FontTypography.Body,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = (System.Windows.Media.Brush)FindResource("TextFillColorSecondaryBrush")
                };

                stackPanel.Children.Add(headerTextBlock);
                stackPanel.Children.Add(descriptionTextBlock);

                var CardGrid = new Grid();

                CardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.05, GridUnitType.Star) });
                CardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star)});

                CardGrid.Children.Add(Icon);
                CardGrid.Children.Add(stackPanel);

                Grid.SetColumn(Icon, 0);
                Grid.SetColumn(stackPanel, 1);

                card.Header = CardGrid;

                var buttonsAndSpinnerGridYeahThisVarNameIsVeryLongSorryLOL = new Grid();

                var loading = new ProgressBar
                {
                    IsIndeterminate = true,
                    Visibility = Visibility.Hidden,
                    Margin = new Thickness(0,10,0,0)
                };
                stackPanel.Children.Add(loading);

                buttonsAndSpinnerGridYeahThisVarNameIsVeryLongSorryLOL.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                buttonsAndSpinnerGridYeahThisVarNameIsVeryLongSorryLOL.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var uninstallButton = new Wpf.Ui.Controls.Button
                {
                    Content = "Uninstall",
                    Margin = new Thickness(10),
                    Icon = new ImageIcon
                    {
                        Source = FilesHelper.CreateBitmapImage("delete.png", 30),
                        Height = 30,
                        Width = 30
                    },
                    Visibility = Visibility.Hidden
                };

                buttonsAndSpinnerGridYeahThisVarNameIsVeryLongSorryLOL.Children.Add(uninstallButton);
                Grid.SetColumn(uninstallButton, 0);

                var installButton = new Wpf.Ui.Controls.Button
                {
                    Content = "Install",
                    Icon = new ImageIcon
                    {
                        Source = FilesHelper.CreateBitmapImage("downloads.png", 30),
                        Height = 30,
                        Width = 30
                    },
                    Margin = new Thickness(10)
                };

                buttonsAndSpinnerGridYeahThisVarNameIsVeryLongSorryLOL.Children.Add(installButton);
                Grid.SetColumn(installButton, 1);

                installButton.Click += async (sender, e) =>
                {
                    if (sender is Wpf.Ui.Controls.Button button)
                    {
                        button.Content = "Installing...";
                        button.IsEnabled = false;
                        loading.Visibility = Visibility.Visible;
                        var succes = await installApp(app);
                        if (succes[0] == "true")
                        {
                            button.Content = "Update";
                            uninstallButton.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            button.Content = "Install";
                            UiElementsHelper.MessBox("Error", $"Ther was an error installing {app.name}\n\n{succes[1]}");
                        }
                        button.IsEnabled = true;
                        loading.Visibility = Visibility.Hidden;
                    }
                };

                uninstallButton.Click += async (sender, e) =>
                {
                    if (sender is Wpf.Ui.Controls.Button button)
                    {
                        uninstallButton.IsEnabled = false;
                        loading.Visibility = Visibility.Visible;
                        var succes = await uninstallApp(app);
                        if (succes[0] == "true")
                        {
                            installButton.Content = "Install";
                            loading.Visibility = Visibility.Hidden;
                            uninstallButton.Visibility = Visibility.Hidden;
                        } else
                        {
                            UiElementsHelper.MessBox("Error", $"Ther was an error uninstalling {app.name}\n\n{succes[1]}");
                        }
                        loading.Visibility = Visibility.Hidden;
                        uninstallButton.IsEnabled = true;
                        installButton.IsEnabled = true;
                    }
                };

                if (packageList.Contains(app.id)) {
                    uninstallButton.Visibility = Visibility.Visible;
                    installButton.Content = "Update";
                }

                card.Content = buttonsAndSpinnerGridYeahThisVarNameIsVeryLongSorryLOL;
                    
                AppsStackPannel.Children.Add(card);
            }
        }
        private async Task<List<String>> uninstallApp(App app)
        {
            try
            {
                var device = ADBHelper.GetDevice();

                if (device == null)
                {
                    throw new DeviceNotFoundException();
                }
                await Task.Run(() => ADBHelper.UninstallPackage(app.id, (DeviceData)device));
                return ["true", ""];
            }
            catch (Exception e)
            {
                return ["false", e.Message];
            }
        }
        private async Task<List<String>> installApp(App app)
        {
            try
            {
                var httpHandler = new SentryHttpMessageHandler();
                var http = new HttpClient(httpHandler);
                http.DefaultRequestHeaders.UserAgent.ParseAdd("ulysse2211/Y10_Tools");

                string downloadUrl = null;

                if (app.provider.ToLower() == "github")
                {
                    var response = await http.GetStringAsync($"https://api.github.com/repos/{app.github.Split("/")[0]}/{app.github.Split("/")[1]}/releases/latest");

                    using JsonDocument doc = JsonDocument.Parse(response);
                    var assets = doc.RootElement.GetProperty("assets");

                    foreach (var asset in assets.EnumerateArray())
                    {
                        if (asset.GetProperty("name").GetString().EndsWith(".apk"))
                        {
                            downloadUrl = asset.GetProperty("browser_download_url").GetString();
                            break;
                        }
                    }
                }
                else if (app.provider.ToLower() == "gitlab")
                {
                    var response = await http.GetStringAsync($"https://gitlab.com/api/v4/projects/{Uri.EscapeDataString(app.gitlab)}/releases");

                    using JsonDocument doc = JsonDocument.Parse(response);

                    var releases = doc.RootElement.EnumerateArray();
                    if (releases.Any())
                    {
                        var latestRelease = releases.First();

                        var links = latestRelease.GetProperty("assets").GetProperty("links");

                        foreach (var link in links.EnumerateArray())
                        {
                            var url = link.GetProperty("url").GetString();
                            if (url.EndsWith(".apk"))
                            {
                                downloadUrl = url;
                                break;
                            }
                        }
                    }
                }
                else if (app.provider.ToLower() == "direct")
                {
                    downloadUrl = app.direct;
                }


                if (downloadUrl == null)
                {
                    throw new FileNotFoundException();
                }

                if (!Directory.Exists(Path.Combine(FilesHelper.GetTempFilePath(), "AppsApk")))
                {
                    Directory.CreateDirectory(Path.Combine(FilesHelper.GetTempFilePath(), "AppsApk"));
                }

                await File.WriteAllBytesAsync(
                    Path.Combine(FilesHelper.GetTempFilePath(), "AppsApk", $@"{app.id}.apk"),
                    await http.GetByteArrayAsync(downloadUrl)
                );

                var device = ADBHelper.GetDevice();

                if (device == null)
                {
                    throw new DeviceNotFoundException();
                }

                ADBHelper.InstallApk(
                    Path.Combine(FilesHelper.GetTempFilePath(), "AppsApk", $@"{app.id}.apk"),
                    (DeviceData)device
                    );
                return ["true", ""];
            } catch (Exception e)
            {
                return ["false", e.Message];
            }
        }

        public void RemoveAppsPageWarnings()
        {
            ConnectDeviceOverlay.Visibility = Visibility.Hidden;
            ConnectDeviceOverlayBlur.Radius = 0;
        }

        public void AddAppsPageWarnings()
        {
            ConnectDeviceOverlay.Visibility = Visibility.Visible;
            ConnectDeviceOverlayBlur.Radius = 15;
        }
    }
}
