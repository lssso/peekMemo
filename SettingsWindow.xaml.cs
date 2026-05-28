using System;
using System;
using System.IO;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PeekMemo
{
    public partial class SettingsWindow : Window
    {
        private AppSettings originalSettings;
        private AppSettings tempSettings;
        private bool isLoadingSettings = false;
        private int selectedIndex = -1;

        public event System.Action<AppSettings> SettingsPreviewChanged;
        public event System.Action<AppSettings> SettingsSaved;

        public SettingsWindow(AppSettings settings, int startIndex)
        {
            InitializeComponent();

            originalSettings = settings;
            tempSettings = CloneSettings(settings);

            if (startIndex < 0)
            {
                SettingsTabButton_Click(null, null);
            }
            else
            {
                LoadIndexSettings(startIndex);
            }
            UpdateAddButton();
        }

        private AppSettings CloneSettings(AppSettings source)
        {
            AppSettings clone = new AppSettings
            {
                OpenMode = source.OpenMode,
                Monitor = source.Monitor,
                Edge = source.Edge,
                Alignment = source.Alignment,
                IndexLength = source.IndexLength,
                VisibleIndexCount = source.VisibleIndexCount
            };

            foreach (MemoIndexSettings index in source.Indexes)
            {
                clone.Indexes.Add(new MemoIndexSettings
                {
                    Title = index.Title,
                    Color = index.Color,
                    HotKey = index.HotKey,
                    MemoFileName = index.MemoFileName
                });
            }

            return clone;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyCurrentSettingsToObject();

            CopySettings(tempSettings, originalSettings);

            SettingsService.Save(originalSettings);

            SettingsSaved?.Invoke(originalSettings);

            MessageBox.Show("설정이 저장되었습니다.");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsPreviewChanged?.Invoke(originalSettings);
            this.Close();
        }

        private void SettingsTabButton_Click(object sender, RoutedEventArgs e)
        {
            selectedIndex = -1;

            AppSettingsPanel.Visibility = Visibility.Visible;
            IndexSettingsPanel.Visibility = Visibility.Collapsed;

            LoadAppSettings();

            UpdateSelectedTabStyle();
        }

        private void LoadIndexSettings(int index)
        {
            isLoadingSettings = true;

            selectedIndex = index;

            AppSettingsPanel.Visibility = Visibility.Collapsed;
            IndexSettingsPanel.Visibility = Visibility.Visible;

            TitleTextBox.Text = tempSettings.Indexes[index].Title;

            string currentColor = tempSettings.Indexes[index].Color;

            foreach (var child in ColorPanel.Children)
            {
                RadioButton radioButton = child as RadioButton;

                if (radioButton != null)
                {
                    radioButton.IsChecked =
                        radioButton.Tag.ToString() == currentColor;
                }
            }

            UpdateSelectedTabStyle();

            isLoadingSettings = false;
        }

        private void IndexTabButton1_Click(object sender, RoutedEventArgs e)
        {
            LoadIndexSettings(0);
        }

        private void IndexTabButton2_Click(object sender, RoutedEventArgs e)
        {
            LoadIndexSettings(1);
        }
        private void IndexTabButton3_Click(object sender, RoutedEventArgs e)
        {
            LoadIndexSettings(2);
        }

        private void UpdateSelectedTabStyle()
        {
            var selectedBrush = (System.Windows.Media.Brush)
                new System.Windows.Media.BrushConverter().ConvertFromString("#FFF5F5F5");

            var normalBrush = System.Windows.Media.Brushes.White;
            var borderBrush = (System.Windows.Media.Brush)
                new System.Windows.Media.BrushConverter().ConvertFromString("#FFE5E5E5");

            SettingsTabButton.Background = selectedIndex == -1 ? selectedBrush : normalBrush;
            SettingsTabButton.BorderBrush = borderBrush;
            SettingsTabButton.BorderThickness = selectedIndex == -1 ? new Thickness(1) : new Thickness(0);
            SettingsTabButton.FontWeight = selectedIndex == -1 ? FontWeights.Bold : FontWeights.Normal;

            IndexTabButton1.Background = selectedIndex == 0 ? selectedBrush : normalBrush;
            IndexTabButton1.BorderBrush = borderBrush;
            IndexTabButton1.BorderThickness = selectedIndex == 0 ? new Thickness(1) : new Thickness(0);
            IndexTabButton1.FontWeight = selectedIndex == 0 ? FontWeights.Bold : FontWeights.Normal;

            IndexTabButton2.Background = selectedIndex == 1 ? selectedBrush : normalBrush;
            IndexTabButton2.BorderBrush = borderBrush;
            IndexTabButton2.BorderThickness = selectedIndex == 1 ? new Thickness(1) : new Thickness(0);
            IndexTabButton2.FontWeight = selectedIndex == 1 ? FontWeights.Bold : FontWeights.Normal;

            AddIndexButton.Background = selectedIndex == 2 ? selectedBrush : normalBrush;
            AddIndexButton.BorderBrush = borderBrush;
            AddIndexButton.BorderThickness = selectedIndex == 2 ? new Thickness(1) : new Thickness(0);
            AddIndexButton.FontWeight = selectedIndex == 2 ? FontWeights.Bold : FontWeights.Normal;
        }
        private void LoadAppSettings()
        {
            OpenModeComboBox.SelectedIndex = tempSettings.OpenMode == "Click" ? 1 : 0;

            if (tempSettings.IndexLength == "Short")
            {
                IndexLengthComboBox.SelectedIndex = 0;
            }
            else if (tempSettings.IndexLength == "Long")
            {
                IndexLengthComboBox.SelectedIndex = 2;
            }
            else
            {
                IndexLengthComboBox.SelectedIndex = 1;
            }
            if (tempSettings.Edge == "Left")
            {
                EdgeComboBox.SelectedIndex = 0;
            }
            else if (tempSettings.Edge == "Top")
            {
                EdgeComboBox.SelectedIndex = 2;
            }
            else
            {
                EdgeComboBox.SelectedIndex = 1;
            }
        }
        private void SettingValue_Changed(object sender, RoutedEventArgs e)
        {
            if (isLoadingSettings)
            {
                return;
            }
            if (tempSettings == null)
            {
                return;
            }

            ApplyCurrentSettingsToObject();

            UpdateAddButton();

            SettingsPreviewChanged?.Invoke(tempSettings);
        }

        private void ApplyCurrentSettingsToObject()
        {
            if (selectedIndex == -1)
            {
                tempSettings.OpenMode =
                    OpenModeComboBox.SelectedIndex == 0 ? "Hover" : "Click";

                tempSettings.IndexLength =
                    IndexLengthComboBox.SelectedIndex == 0 ? "Short" :
                    IndexLengthComboBox.SelectedIndex == 1 ? "Medium" : "Long";

                tempSettings.Edge =
                    EdgeComboBox.SelectedIndex == 0 ? "Left" :
                    EdgeComboBox.SelectedIndex == 2 ? "Top" : "Right";
            }
            else
            {
                tempSettings.Indexes[selectedIndex].Title = TitleTextBox.Text;

                foreach (var child in ColorPanel.Children)
                {
                    RadioButton radioButton = child as RadioButton;

                    if (radioButton != null && radioButton.IsChecked == true)
                    {
                        tempSettings.Indexes[selectedIndex].Color = radioButton.Tag.ToString();
                        break;
                    }
                }
            }
        }
        private void CopySettings(AppSettings source, AppSettings target)
        {
            target.OpenMode = source.OpenMode;
            target.Monitor = source.Monitor;
            target.Edge = source.Edge;
            target.Alignment = source.Alignment;
            target.IndexLength = source.IndexLength;
            target.VisibleIndexCount = source.VisibleIndexCount;
            target.Indexes.Clear();

            foreach (MemoIndexSettings index in source.Indexes)
            {
                target.Indexes.Add(new MemoIndexSettings
                {
                    Title = index.Title,
                    Color = index.Color,
                    HotKey = index.HotKey,
                    MemoFileName = index.MemoFileName
                });
            }
        }
        private void UpdateAddButton()
        {
            IndexTabButton1.Content = tempSettings.Indexes[0].Title;
            IndexTabButton2.Content = tempSettings.Indexes[1].Title;

            if (tempSettings.VisibleIndexCount >= 3)
            {
                AddIndexButton.Content = tempSettings.Indexes[2].Title;

                AddIndexButton.Width = 70;
                AddIndexButton.FontSize = 14;
                AddIndexButton.FontWeight = FontWeights.Normal;
                AddIndexButton.Background = Brushes.White;
                AddIndexButton.BorderThickness = new Thickness(0);

                AddIndexButton.Click -= AddIndexButton_Click;
                AddIndexButton.Click -= IndexTabButton3_Click;
                AddIndexButton.Click += IndexTabButton3_Click;
            }
            else
            {
                AddIndexButton.Content = "+";

                AddIndexButton.Width = 34;
                AddIndexButton.FontSize = 18;
                AddIndexButton.FontWeight = FontWeights.Bold;

                AddIndexButton.Click -= IndexTabButton3_Click;
                AddIndexButton.Click -= AddIndexButton_Click;
                AddIndexButton.Click += AddIndexButton_Click;
            }

            UpdateSelectedTabStyle();
        }
        private void AddIndexButton_Click(object sender, RoutedEventArgs e)
        {
            if (tempSettings.VisibleIndexCount < 3)
            {
                tempSettings.VisibleIndexCount++;

                tempSettings.Indexes[2].Title = "인덱스명";
                tempSettings.Indexes[2].Color = "#FFFFD54F";

                UpdateAddButton();

                LoadIndexSettings(2);

                SettingsPreviewChanged?.Invoke(tempSettings);
            }
            else
            {
                MessageBox.Show("인덱스는 최대 3개까지 추가할 수 있습니다.");
            }
        }

        private void DeleteIndexButton_Click(object sender, RoutedEventArgs e)
        {
            if (tempSettings.VisibleIndexCount <= 1)
            {
                MessageBox.Show("인덱스는 최소 1개 이상 있어야 합니다.");
                return;
            }

            if (selectedIndex < 0)
            {
                return;
            }

            string memoFileName = tempSettings.Indexes[selectedIndex].MemoFileName;

            if (File.Exists(memoFileName))
            {
                string deletedFolder = "deleted";

                if (!Directory.Exists(deletedFolder))
                {
                    Directory.CreateDirectory(deletedFolder);
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                string deletedFileName =
                    Path.GetFileNameWithoutExtension(memoFileName)
                    + "_"
                    + timestamp
                    + ".txt";

                string deletedPath = Path.Combine(deletedFolder, deletedFileName);

                File.Move(memoFileName, deletedPath);
            }

            tempSettings.Indexes.RemoveAt(selectedIndex);
            tempSettings.VisibleIndexCount--;

            while (tempSettings.Indexes.Count < 3)
            {
                tempSettings.Indexes.Add(new MemoIndexSettings
                {
                    Title = "인덱스명",
                    Color = "#FFFFD54F",
                    HotKey = "",
                    MemoFileName = $"memo{tempSettings.Indexes.Count + 1}.txt"
                });
            }

            CopySettings(tempSettings, originalSettings);
            SettingsService.Save(originalSettings);

            SettingsSaved?.Invoke(originalSettings);

            int nextIndex = selectedIndex;

            if (nextIndex >= tempSettings.VisibleIndexCount)
            {
                nextIndex = tempSettings.VisibleIndexCount - 1;
            }

            UpdateAddButton();
            LoadIndexSettings(nextIndex);

            SettingsPreviewChanged?.Invoke(tempSettings);
            SettingsSaved?.Invoke(originalSettings);

            MessageBox.Show("인덱스가 삭제되었습니다.");

        }

    }
}