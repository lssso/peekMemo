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

            if (lines.Length >= 9)
            {
                settings.OpenMode = lines[0];
                settings.IndexLength = lines[1];

                int visibleIndexCount;
                if (int.TryParse(lines[2], out visibleIndexCount))
                {
                    settings.VisibleIndexCount = visibleIndexCount;
                }

                settings.Indexes[0].Title = lines[3];
                settings.Indexes[0].Color = lines[4];

                settings.Indexes[1].Title = lines[5];
                settings.Indexes[1].Color = lines[6];

                settings.Indexes[2].Title = lines[7];
                settings.Indexes[2].Color = lines[8];
            }

            return settings;
        }

        public static void Save(AppSettings settings)
        {
            List<string> lines = new List<string>
            {
                settings.OpenMode,
                settings.IndexLength,
                settings.VisibleIndexCount.ToString(),

                settings.Indexes[0].Title,
                settings.Indexes[0].Color,

                settings.Indexes[1].Title,
                settings.Indexes[1].Color,

                settings.Indexes[2].Title,
                settings.Indexes[2].Color
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