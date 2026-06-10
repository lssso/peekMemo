using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Forms = System.Windows.Forms;

namespace PeekMemo
{
    public static class SettingsService
    {
        //private static readonly string SettingsFilePath = "settings.txt";
        //private static readonly string SettingsFilePath = "settings.json";
        private static string GetSettingsFilePath()
        {
            return Path.Combine(
                DataFolderManager.GetDefaultDataFolder(),
                "settings.json");
        }

        public static AppSettings Load()
        {
            string settingsFilePath = GetSettingsFilePath();

            if (!File.Exists(settingsFilePath))
            {
                AppSettings defaultSettings = CreateDefaultSettings();

                Save(defaultSettings);

                return defaultSettings;
            }

            string json = File.ReadAllText(settingsFilePath);

            AppSettings settings =
                JsonConvert.DeserializeObject<AppSettings>(json);

            if (settings == null)
            {
                return CreateDefaultSettings();
            }

            if (string.IsNullOrWhiteSpace(settings.DataFolder))
            {
                settings.DataFolder = DataFolderManager.GetDefaultDataFolder();
            }

            return settings;
        }

        public static void Save(AppSettings settings)
        {
            string settingsFilePath = GetSettingsFilePath();

            string folder = Path.GetDirectoryName(settingsFilePath);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);

            File.WriteAllText(settingsFilePath, json);
        }

        private static AppSettings CreateDefaultSettings()
        {
            return new AppSettings
            {
                DataFolder = DataFolderManager.GetDefaultDataFolder(),
                OpenMode = "Hover",
                Monitor = Forms.Screen.PrimaryScreen.DeviceName,
                Edge = "Right",
                Alignment = "Center",
                IndexLength = "Medium",
                VisibleIndexCount = 2,
                Indexes = new List<MemoIndexSettings>
                {
                    new MemoIndexSettings
                    {
                        Title = "인덱스1",
                        Color = "#FFFFD54F",
                        HotKey = "",
                        MemoFileName = "memo1.txt"
                    },
                    new MemoIndexSettings
                    {
                        Title = "인덱스2",
                        Color = "#FFF8BBD0",
                        HotKey = "",
                        MemoFileName = "memo2.txt"
                    },
                    new MemoIndexSettings
                    {
                        Title = "인덱스3",
                        Color = "#FFBBDEFB",
                        HotKey = "",
                        MemoFileName = "memo3.txt"
                    }
                }
            };
        }

    }
}