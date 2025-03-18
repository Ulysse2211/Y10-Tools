using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.Models;
using AdvancedSharpAdbClient.Receivers;
using AdvancedSharpAdbClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Y10_Tools.Views.Pages;
using AdvancedSharpAdbClient.Exceptions;
using System.Reflection;

namespace Y10_Tools.Helpers
{
    class ADBHelper
    {
        public static DeviceData? GetDevice()
        {
            if (SettingsPage.SelectedDeviceADB != null)
            {
                return SettingsPage.SelectedDeviceADB;
            }
            return null;
        }

        public async static Task<List<string>> GetPackagesList(DeviceData device)
        {
            var packageListText = "";
            IShellOutputReceiver receiver = new ConsoleOutputReceiver();

            await ADBHelper.ADB.ExecuteShellCommandAsync((DeviceData)device, "pm list packages", receiver);

            packageListText = receiver.ToString();
            packageListText = packageListText.Replace("package:", "");
            return packageListText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                         .Select(p => p.Trim())
                                         .ToList();
        }

        public static async Task<string?> RootShell(DeviceData device, string command)
        {
            try
            {
                IShellOutputReceiver CheckMTKSUBinary = new ConsoleOutputReceiver();
                await ADB.ExecuteShellCommandAsync(device, "[ -e \"/data/local/tmp/mtk-su\" ]  && { echo true; chmod 755 /data/local/tmp/mtk-su } || echo false", CheckMTKSUBinary);

                if (CheckMTKSUBinary.ToString() != "true")
                {
                    using (SyncService service = new SyncService(device))
                    {
                        var assembly = Assembly.GetExecutingAssembly();
                        string resourceName = "Y10_Tools.Assets.Exploit.mtk-su";

                        using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
                        {
                            await service.PushAsync(resourceStream, "/data/local/tmp/mtk-su", UnixFileStatus.DefaultFileMode, DateTimeOffset.Now, null);
                        }
                    }
                    await ADB.ExecuteShellCommandAsync(device, "chmod 755 /data/local/tmp/mtk-su");
                }

                IShellOutputReceiver receiver = new ConsoleOutputReceiver();
                string createScript = $"echo -e \"#!/system/bin/sh\n" + @$"{command}" + "\" > /data/local/tmp/y10tempCommandRun.sh && chmod 755 /data/local/tmp/y10tempCommandRun.sh";
                try
                {
                    await ADB.ExecuteShellCommandAsync(device, $@"{createScript} && /data/local/tmp/mtk-su -c '/data/local/tmp/y10tempCommandRun.sh' && rm /data/local/tmp/y10tempCommandRun.sh", receiver);
                }
                catch (PermissionDeniedException)
                {
                    return "no.";
                }

                return receiver.ToString();

            } catch
            {
                return null;
            }
        }
        public static AdbClient ADB { get; set; }

        public async static void InstallApk(string path, DeviceData device)
        {
            PackageManager manager = new PackageManager(ADB, device);
            await manager.InstallPackageAsync(path, new Action<InstallProgressEventArgs>(o => { }));
        }
        public async static void UninstallPackage(string package, DeviceData device)
        {
            PackageManager manager = new PackageManager(ADB, device);
            await manager.UninstallPackageAsync(package);
        }

        public async static Task<FastbootHelper.FastbootDevice?> ADBToFastboot(DeviceData device, int timeoutMilliseconds = 60000)
        {
            await RootShell((DeviceData)device, "reboot bootloader");
            List<FastbootHelper.FastbootDevice> devices = await FastbootHelper.GetDevices();
            var timeoutTask = Task.Delay(timeoutMilliseconds);

            while (true)
            {
                foreach (FastbootHelper.FastbootDevice dev in devices)
                {
                    if (dev.Serial == device.Serial)
                    {
                        return dev;
                    }
                }
                await Task.Delay(2000);
                devices = await FastbootHelper.GetDevices();
                if (await Task.WhenAny(Task.Delay(1000), timeoutTask) == timeoutTask)
                {
                    return null;
                }
            }
        }

        public static async Task<double> getTemp(DeviceData device, string sensor)
        {
            IShellOutputReceiver receiver = new ConsoleOutputReceiver();
            await ADB.ExecuteShellCommandAsync(device, "cat /sys/class/thermal/thermal_zone*/type", receiver);

            int i = 0;
            foreach (string sensorName in receiver.ToString().Trim().ToLower().Split("\n"))
            {
                if (sensorName.Contains(sensor))
                {
                    IShellOutputReceiver receiver2 = new ConsoleOutputReceiver();
                    await ADB.ExecuteShellCommandAsync(device, $"cat /sys/class/thermal/thermal_zone{i}/temp", receiver2);

                    if (double.TryParse(receiver2.ToString().ToLower().Trim().Replace("\n", "").Replace("\r", "").Replace(" ", ""), out double value))
                    {
                        return value;
                    }
                }
                i += 1;
            }

            return 0;
        }
    }
}
