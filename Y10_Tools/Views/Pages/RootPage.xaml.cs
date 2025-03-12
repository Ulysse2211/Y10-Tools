using Y10_Tools.ViewModels.Pages;
using Wpf.Ui.Controls;
using Y10_Tools.Helpers;
using System.Windows.Input;
using System.Windows.Documents;
using System.Windows.Media;

namespace Y10_Tools.Views.Pages
{
    public partial class RootPage : INavigableView<RootViewModel>
    {
        public RootViewModel ViewModel { get; }
        private string? lastdirectory { get; set; }

        public RootPage(RootViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
            EventManager.ShowOverlayRequested += AddRootPageWarnings;
            EventManager.HideOverlayRequested += RemoveRootPageWarnings;

            if (ADBHelper.GetDevice() != null)
            {
                RemoveRootPageWarnings();
            }
        }
        private void RootC(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var autoSuggestBox = sender as AutoSuggestBox;
                if (autoSuggestBox != null)
                {
                    SendRootCommand(autoSuggestBox.Text);
                    autoSuggestBox.Text = "";
                }
            }
        }
        private async void SendRootCommand(string command)
        {
            var device = SettingsPage.SelectedDeviceADB;
            if (device == null)
            {
                return;
            }

            var response = await ADBHelper.RootShell((AdvancedSharpAdbClient.Models.DeviceData)device, $@"cd {lastdirectory} && {command} && pwd");

            var lines = response.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .ToList();

            if (lines.Count > 0 && lines.Last().Replace("/data/local/tmp/y10tempCommandRun.sh[2]: ", "").StartsWith("/"))
            {
                lastdirectory = lines.Last();
                lines.RemoveAt(lines.Count - 1);
            }

            string output = string.Join(Environment.NewLine, lines);

            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run($"root@{((AdvancedSharpAdbClient.Models.DeviceData)device).Serial} ") { Foreground = new SolidColorBrush(Colors.Green) });
            paragraph.Inlines.Add(new Run(lastdirectory));
            paragraph.Inlines.Add(new Run(" > ") { Foreground = new SolidColorBrush(Colors.Red) });
            paragraph.Inlines.Add(new Run(command));

            paragraph.Inlines.Add(new LineBreak());

            paragraph.Inlines.Add(new Run(output.Replace("/data/local/tmp/y10tempCommandRun.sh[2]: ", "")));

            RootShellOutputs.Document.Blocks.Add(paragraph);
        }

        public void RemoveRootPageWarnings()
        {
            ConnectDeviceOverlay.Visibility = Visibility.Hidden;
            ConnectDeviceOverlayBlur.Radius = 0;
        }
        public void AddRootPageWarnings()
        {
            ConnectDeviceOverlay.Visibility = Visibility.Visible;
            ConnectDeviceOverlayBlur.Radius = 15;
        }
    }
}
