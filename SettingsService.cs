using System.Collections.Generic;
using System.IO;

namespace PeekMemo
{
    public static class SettingsService
    {
        private static readonly string SettingsFilePath = "settings.txt";

        public static AppSettings Load()
        {
            AppSettings settings = CreateDefaultSettings();

            if (!File.Exists(SettingsFilePath))
            {
                return settings;
            }

            string[] lines = File.ReadAllLines(SettingsFilePath);

            if (lines.Length >= 2)
            {
                settings.Indexes[0].Title = lines[0];
                settings.Indexes[0].Color = lines[1];
            }

            return settings;
        }

        public static void Save(AppSettings settings)
        {
            List<string> lines = new List<string>
            {
                settings.Indexes[0].Title,
                settings.Indexes[0].Color
            };

            File.WriteAllLines(SettingsFilePath, lines);
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
                    }
                }
            };
        }
    }
}