using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace PeekMemo
{
    public static class SettingsService
    {
        //private static readonly string SettingsFilePath = "settings.txt";
        private static readonly string SettingsFilePath = "settings.json";

        public static AppSettings Load()
        {
            if (!File.Exists(SettingsFilePath))
            {
                return CreateDefaultSettings();
            }

            string json = File.ReadAllText(SettingsFilePath);

            AppSettings settings =
                JsonConvert.DeserializeObject<AppSettings>(json);

            return settings ?? CreateDefaultSettings();
        }

        public static void Save(AppSettings settings)
        {
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);

            File.WriteAllText(SettingsFilePath, json);
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