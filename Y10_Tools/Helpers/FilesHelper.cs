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
using System.Windows.Media.Imaging;

namespace Y10_Tools.Helpers
{
    class FilesHelper
    {
        public static string GetTempFilePath()
        {
            var temp = Path.GetTempPath();
            if (!Directory.Exists(Path.Combine(temp, "Y10ToolsTemp")))
            {
                Directory.CreateDirectory(Path.Combine(temp, "Y10ToolsTemp"));
            }
            return Path.Combine(temp, "Y10ToolsTemp");
        }

        public static BitmapImage CreateBitmapImage(string iconName, int size)
        {
            var uri = new Uri($"pack://application:,,,/Assets/Icons/{iconName}");
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.DecodePixelWidth = size;
            bitmap.DecodePixelHeight = size;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
    }
}
