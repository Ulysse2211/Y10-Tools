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

        public static async Task<string?> RootShell(DeviceData device, string command)
        {
            if (device == null)
            {
                return null;
            }

            IShellOutputReceiver CheckMTKSUBinary = new ConsoleOutputReceiver();
            await ADB.ExecuteShellCommandAsync(device, "[ -e \"/data/local/tmp/mtk-su\" ]  && { echo true; chmod 755 /data/local/tmp/mtk-su } || echo false", CheckMTKSUBinary);

            if (CheckMTKSUBinary.ToString() != "true")
            {
                using (SyncService service = new SyncService(device))
                {
                    using (FileStream stream = File.OpenRead("Assets/Exploit/mtk-su"))
                    {
                        await service.PushAsync(stream, "/data/local/tmp/mtk-su", UnixFileStatus.DefaultFileMode, DateTimeOffset.Now, null);
                    }
                }
                await ADB.ExecuteShellCommandAsync(device, "chmod 755 /data/local/tmp/mtk-su");
            }

            IShellOutputReceiver receiver = new ConsoleOutputReceiver();
            string createScript = $"echo -e \"#!/system/bin/sh\n{command}\" > /data/local/tmp/y10tempCommandRun.sh && chmod 755 /data/local/tmp/y10tempCommandRun.sh";
            await ADB.ExecuteShellCommandAsync(device, $@"{createScript} && /data/local/tmp/mtk-su -c '/data/local/tmp/y10tempCommandRun.sh' && rm /data/local/tmp/y10tempCommandRun.sh", receiver);

            return receiver.ToString();
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
    }
}
