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
using System.Diagnostics;

namespace Y10_Tools.Helpers
{
    class FastbootHelper
    {
        public static FastbootDevice? SelectedFastbootDevice { get; private set; }

        public class FastbootDevice
        {
            public string Serial { get; set; }
            public string State { get; set; }
        }

        public static FastbootDevice? GetDevice()
        {
            return SelectedFastbootDevice;
        }

        public static FastbootDevice? SetDevice(FastbootDevice? device)
        {
            SelectedFastbootDevice = device;
            EventManager.FastbootDeviceUpdated();
            return SelectedFastbootDevice;
        }

        public async static Task<string> FastbootCommand(string command)
        {
            var fb = Process.Start(new ProcessStartInfo
            {
                FileName = @"fastboot.exe",
                Arguments = command,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            });
            fb.WaitForExit();
            return await fb.StandardOutput.ReadToEndAsync();
        }

        public static async Task<List<FastbootDevice>> GetDevices()
        {
            string response = await FastbootCommand("devices");
            List<string> dList = response.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            List<FastbootDevice> devices = new List<FastbootDevice>();

            for (int i = 0; i < dList.Count; i++)
            {
                if (i % 2 == 0)
                {
                    FastbootDevice device = new FastbootDevice
                    {
                        Serial = dList[i].Normalize().Trim(),
                        State = dList[i + 1].ToLower().Normalize().Trim()
                    };
                    devices.Add(device);
                }
            }

            return devices;
        }

        public async static Task<FastbootDevice?> FindFastbootDeviceFromSerial(string Serial, int timeoutMilliseconds = 60000)
        {
            List<FastbootDevice> devices = await GetDevices();
            var timeoutTask = Task.Delay(timeoutMilliseconds);

            while (true)
            {
                foreach (FastbootDevice dev in devices)
                {
                    if (dev.Serial == Serial)
                    {
                        return dev;
                    }
                }
                await Task.Delay(5000);
                devices = await GetDevices();
                if (await Task.WhenAny(Task.Delay(1000), timeoutTask) == timeoutTask)
                {
                    return null;
                }
            }
        }

        public static async Task<DeviceData?> FastbootDeviceToDeviceData(FastbootDevice device, int timeoutMilliseconds = 900000)
        {
            if (device.State != "fastboot")
            {
                return null;
            }

            await FastbootCommand($@"-s {device.Serial} reboot");

            DeviceData? adbDevice = null;
            var timeoutTask = Task.Delay(timeoutMilliseconds);

            while (adbDevice == null)
            {
                var tempAdbDevices = ADBHelper.ADB.GetDevices();
                foreach (DeviceData adb in tempAdbDevices)
                {
                    if (adb.Serial == device.Serial)
                    {
                        try
                        {
                            if (adb.State != DeviceState.Online)
                            {
                                await Task.Delay(2000);
                                continue;
                            }

                            var a = await ADBHelper.RootShell(adb, "echo hi");
                            if (a == "no.")
                            {
                                UiElementsHelper.MessBox("Device locked", "The device seems to be locked, please close this message once it has been unlocked.");
                                await Task.Delay(10000);
                                continue;
                            }

                            adbDevice = adb;
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }

                if (await Task.WhenAny(Task.Delay(1000), timeoutTask) == timeoutTask)
                {
                    return null;
                }
            }

            return adbDevice;
        }
    }
}
