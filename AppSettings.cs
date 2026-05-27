using System.Collections.Generic;

namespace PeekMemo
{
    public class AppSettings
    {
        public string OpenMode { get; set; } = "Hover";
        public string Monitor { get; set; } = "Primary";
        public string Edge { get; set; } = "Right";
        public string Alignment { get; set; } = "Center";
        public string IndexLength { get; set; } = "Medium";

        public int VisibleIndexCount { get; set; } = 2;

        public List<MemoIndexSettings> Indexes { get; set; } = new List<MemoIndexSettings>();
    }
}