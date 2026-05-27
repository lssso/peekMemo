using System.Windows;
using System.Windows.Controls;

namespace PeekMemo
{
    public partial class SettingsWindow : Window
    {
        private AppSettings originalSettings;
        private AppSettings tempSettings;
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
            selectedIndex = index;

            AppSettingsPanel.Visibility = Visibility.Collapsed;
            IndexSettingsPanel.Visibility = Visibility.Visible;

            TitleTextBox.Text = tempSettings.Indexes[index].Title;

            UpdateSelectedTabStyle();
        }

        private void IndexTabButton1_Click(object sender, RoutedEventArgs e)
        {
            LoadIndexSettings(0);
        }

        private void IndexTabButton2_Click(object sender, RoutedEventArgs e)
        {
            LoadIndexSettings(1);
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
        }
        private void SettingValue_Changed(object sender, RoutedEventArgs e)
        {
            if (tempSettings == null)
            {
                return;
            }

            ApplyCurrentSettingsToObject();

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

        private void AddIndexButton_Click(object sender, RoutedEventArgs e)
        {
            if (tempSettings.VisibleIndexCount < 3)
            {
                tempSettings.VisibleIndexCount++;

                CopySettings(tempSettings, originalSettings);
                SettingsService.Save(originalSettings);

                SettingsPreviewChanged?.Invoke(tempSettings);
                CopySettings(tempSettings, originalSettings);
                SettingsService.Save(originalSettings);
                SettingsSaved?.Invoke(originalSettings);

                MessageBox.Show("인덱스가 추가되었습니다.");
            }
            else
            {
                MessageBox.Show("인덱스는 최대 3개까지 추가할 수 있습니다.");
            }
        }

    }
}