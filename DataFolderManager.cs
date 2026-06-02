using System;
using System.IO;

namespace PeekMemo
{
    public static class DataFolderManager
    {
        public static string GetDefaultDataFolder()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PeekMemo");
        }

        public static string GetDataFolder()
        {
            AppSettings settings = SettingsService.Load();

            string folder = string.IsNullOrWhiteSpace(settings.DataFolder)
                ? GetDefaultDataFolder()
                : settings.DataFolder;

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            return folder;
        }
    }
}