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
    }
}
