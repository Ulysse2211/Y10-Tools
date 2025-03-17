using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Y10_Tools.Services;
using Y10_Tools.ViewModels.Pages;
using Y10_Tools.ViewModels.Windows;
using Y10_Tools.Views.Pages;
using Y10_Tools.Views.Windows;
using Wpf.Ui;
using AdvancedSharpAdbClient;
using Y10_Tools.Helpers;
using System.Diagnostics;
using Wpf.Ui.Appearance;

namespace Y10_Tools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c =>
            {
                var basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)
                               ?? AppDomain.CurrentDomain.BaseDirectory;
                c.SetBasePath(basePath);
            }).ConfigureServices((context, services) =>
            {
                services.AddHostedService<ApplicationHostService>();

                // Page resolver service
                services.AddSingleton<IPageService, PageService>();

                // Theme manipulation
                services.AddSingleton<IThemeService, ThemeService>();

                // TaskBar manipulation
                services.AddSingleton<ITaskBarService, TaskBarService>();

                // Service containing navigation, same as INavigationWindow... but without window
                services.AddSingleton<INavigationService, NavigationService>();

                // Main window with navigation
                services.AddSingleton<INavigationWindow, MainWindow>();
                services.AddSingleton<MainWindowViewModel>();

                services.AddSingleton<DashboardPage>();
                services.AddSingleton<DashboardViewModel>();


                services.AddSingleton<FlashPage>();
                services.AddSingleton<FlashViewModel>();
                services.AddSingleton<DumpPage>();
                services.AddSingleton<DumpViewModel>();
                services.AddSingleton<AppsPage>();
                services.AddSingleton<AppsViewModel>();
                services.AddSingleton<RootPage>();
                services.AddSingleton<RootViewModel>();
                services.AddSingleton<BootPage>();
                services.AddSingleton<BootViewModel>();
                services.AddSingleton<RecoveryPage>();
                services.AddSingleton<RecoveryViewModel>();
                services.AddSingleton<MiscPage>();
                services.AddSingleton<MiscViewModel>();

                services.AddSingleton<SettingsPage>();
                services.AddSingleton<SettingsViewModel>();
            }).Build();

        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T GetService<T>()
            where T : class
        {
            return _host.Services.GetService(typeof(T)) as T;
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>

        private void OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                DispatcherUnhandledException += App_DispatcherUnhandledException;
                SentrySdk.Init(o =>
                {
                    // Tells which project in Sentry to send events to:
                    o.Dsn = "https://dccb6a5c71184e7179667ee791bf033f@o4508994432532480.ingest.de.sentry.io/4508994455797840";
                    // When configuring for the first time, to see what the SDK is doing:
                    o.TracesSampleRate = 1.0;
                    o.ProfilesSampleRate = 1.0;
                    o.Debug = true;
                    o.AttachStacktrace = true;
                    o.CaptureFailedRequests = true;
                    o.DiagnosticLevel = SentryLevel.Debug;
                    o.AutoSessionTracking = true;
                    o.IsGlobalModeEnabled = true;
                    o.FailedRequestStatusCodes.Add((400, 499));
                    o.AddIntegration(new ProfilingIntegration(
                        // During startup, wait up to 500ms to profile the app startup code.
                        // This could make launching the app a bit slower so comment it out if you
                        // prefer profiling to start asynchronously
                        TimeSpan.FromMilliseconds(500)
                    ));
                });

                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                DispatcherUnhandledException += App_DispatcherUnhandledException;

                _host.Start();
                var adbserverstart = Process.Start(new ProcessStartInfo
                {
                    FileName = @"adb.exe",
                    Arguments = "start-server",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                adbserverstart.WaitForExit();

                ADBHelper.ADB = new AdbClient();

                ApplicationAccentColorManager.Apply(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#BA3030")
                );
            }
            catch (Exception ex)
            {
                UnhandledExceptionHandler3000WOWSuchAHandsomeFunctionNameNoJokes(ex);
            } 
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();

            Directory.Delete(FilesHelper.GetTempFilePath(), true);

            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            UnhandledExceptionHandler3000WOWSuchAHandsomeFunctionNameNoJokes(e.Exception);
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            UnhandledExceptionHandler3000WOWSuchAHandsomeFunctionNameNoJokes(e.Exception);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            UnhandledExceptionHandler3000WOWSuchAHandsomeFunctionNameNoJokes(e.ExceptionObject as Exception);
        }

        private static void UnhandledExceptionHandler3000WOWSuchAHandsomeFunctionNameNoJokes(Exception e)
        {
            try
            {
                File.WriteAllText("error_log.txt", e.ToString());
                SentrySdk.CaptureException(e);
            }
            catch (Exception ex2)
            {
                MessageBox.Show(e.ToString() + '\n' + ex2.ToString());
            }
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }
    }
}
