using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace PeekMemo
{
    public static class SettingsService
    {
        //private static readonly string SettingsFilePath = "settings.txt";
        //private static readonly string SettingsFilePath = "settings.json";
        private static string GetSettingsFilePath()
        {
            return Path.Combine(
                DataFolderManager.GetDataFolder(),
                "settings.json");
        }

        public static AppSettings Load()
        {
            string settingsFilePath = GetSettingsFilePath();

            if (!File.Exists(settingsFilePath))
            {
                return CreateDefaultSettings();
            }

            string json = File.ReadAllText(settingsFilePath);

            AppSettings settings =
                JsonConvert.DeserializeObject<AppSettings>(json);

            return settings ?? CreateDefaultSettings();
        }

        public static void Save(AppSettings settings)
        {
            string settingsFilePath = GetSettingsFilePath();

            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);

            File.WriteAllText(settingsFilePath, json);
        }

        private static AppSettings CreateDefaultSettings()
        {
            return new AppSettings
            {
                OpenMode = "Hover",
                Monitor = "Primary",
                Edge = "Right",
                Alignment = "Center",
                IndexLength = "Medium",
                VisibleIndexCount = 2,
                Indexes = new List<MemoIndexSettings>
                {
                    new MemoIndexSettings
                    {
                        Title = "업무",
                        Color = "#FFFFD54F",
                        HotKey = "",
                        MemoFileName = "memo1.txt"
                    },
                    new MemoIndexSettings
                    {
                        Title = "개인",
                        Color = "#FFF8BBD0",
                        HotKey = "",
                        MemoFileName = "memo2.txt"
                    },
                    new MemoIndexSettings
                    {
                        Title = "공부",
                        Color = "#FFBBDEFB",
                        HotKey = "",
                        MemoFileName = "memo3.txt"
                    }
                }
            };
        }

    }
}