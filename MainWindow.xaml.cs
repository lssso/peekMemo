using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Media;

namespace PeekMemo
{
    public partial class MainWindow : Window
    {
        private int currentIndex = 0;
        private bool isPinned = false;
        private AppSettings appSettings;
        
        public MainWindow()
        {
            InitializeComponent();

            saveTimer = new DispatcherTimer();
            saveTimer.Interval = TimeSpan.FromSeconds(1);
            saveTimer.Tick += SaveTimer_Tick;

            appSettings = SettingsService.Load();

            ApplySettings();

            LoadMemo();
        }

        private DispatcherTimer saveTimer;

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                isPinned = false;
                HideMemo();
            }
        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isPinned = true;
            ShowMemo();

            if (e.ClickCount == 2)
            {
                this.DragMove();
            }
        }
        private void MemoTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            SaveStatusText.Text = "입력 중...";

            saveTimer.Stop();
            saveTimer.Start();
        }
        private void SaveTimer_Tick(object sender, EventArgs e)
        {
            saveTimer.Stop();

            SaveMemo();

            SaveStatusText.Text =
                DateTime.Now.ToString("yy.MM.dd HH:mm") + " 저장됨";
        }

        private void LoadMemo()
        {
            string memoFileName = GetCurrentMemoFileName();

            if (File.Exists(memoFileName))
            {
                MemoTextBox.Text = File.ReadAllText(memoFileName);
                SaveStatusText.Text = "저장된 메모 불러옴";
            }
            else
            {
                MemoTextBox.Text = "";
                SaveStatusText.Text = "새 메모";
            }
        }

        private void SetWindowPosition()
        {
            //var workArea = SystemParameters.WorkArea;

            //this.Left = workArea.Right - 40;
            //this.Top = workArea.Top + (workArea.Height - this.Height) / 2;
            HideMemo();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetWindowPosition();
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            if (appSettings.OpenMode == "Hover")
            {
                ShowMemo();
            }
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            if (appSettings.OpenMode == "Hover" && !isPinned)
            {
                HideMemo();
            }
        }

        private void ShowMemo()
        {
            var workArea = SystemParameters.WorkArea;

            // 전체 창이 보이게
            AnimateWindow(workArea.Right - this.Width);
            this.Top = workArea.Top + (workArea.Height - this.Height) / 2;
        }

        private void HideMemo()
        {
            var workArea = SystemParameters.WorkArea;

            // 탭 40px만 보이게
            AnimateWindow(workArea.Right - 32);
            this.Top = workArea.Top + (workArea.Height - this.Height) / 2;
        }
        private void AnimateWindow(double targetLeft)
        {
            DoubleAnimation animation = new DoubleAnimation();

            animation.To = targetLeft;
            animation.Duration = TimeSpan.FromMilliseconds(300);

            this.BeginAnimation(Window.LeftProperty, animation);
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (appSettings.OpenMode == "Click")
            {
                isPinned = true;
                ShowMemo();
            }
            else
            {
                isPinned = true;
            }
        }
        private void MemoTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isPinned = true;
            ShowMemo();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(appSettings, currentIndex);

            settingsWindow.Owner = this;
            settingsWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            settingsWindow.Left = this.Left - settingsWindow.Width - 10;
            settingsWindow.Top = this.Top;

            settingsWindow.SettingsPreviewChanged += (previewSettings) =>
            {
                appSettings = previewSettings;
                ApplySettings();
            };

            settingsWindow.SettingsSaved += (savedSettings) =>
            {
                appSettings = savedSettings;
                ApplySettings();
            };

            settingsWindow.Show();
        }

        private void ApplySettings()
        {
            MemoIndexSettings index1 = appSettings.Indexes[0];
            MemoIndexSettings index2 = appSettings.Indexes[1];
            MemoIndexSettings index3 = appSettings.Indexes[2];

            MemoTabText1.Text = index1.Title;
            MemoTabText2.Text = index2.Title;
            MemoTabText3.Text = index3.Title;

            Brush colorBrush1 =
                (Brush)new BrushConverter().ConvertFromString(index1.Color);

            Brush colorBrush2 =
                (Brush)new BrushConverter().ConvertFromString(index2.Color);

            Brush colorBrush3 =
                (Brush)new BrushConverter().ConvertFromString(index3.Color);

            MemoTabBorder1.Background = colorBrush1;
            MemoTabBorder2.Background = colorBrush2;
            MemoTabBorder3.Background = colorBrush3;

            MemoIndexSettings currentIndexSetting = appSettings.Indexes[currentIndex];

            Brush currentColorBrush =
                (Brush)new BrushConverter().ConvertFromString(currentIndexSetting.Color);

            MemoBodyBorder.Background = currentColorBrush;

            ApplyIndexLength();

            MemoTabBorder3.Visibility =
            appSettings.VisibleIndexCount >= 3
            ? Visibility.Visible
            : Visibility.Collapsed;

        }

        private string GetCurrentMemoFileName()
        {
            return appSettings.Indexes[currentIndex].MemoFileName;
        }

        private void MemoTab1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SwitchMemo(0);
        }

        private void MemoTab2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SwitchMemo(1);
        }

        private void MemoTab3_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SwitchMemo(2);
        }

        private void SwitchMemo(int index)
        {
            SaveMemo();

            currentIndex = index;

            LoadMemo();

            ApplySettings();
        }

        private void SaveMemo()
        {
            File.WriteAllText(GetCurrentMemoFileName(), MemoTextBox.Text);
        }

        private void ApplyIndexLength()
        {
            double tabHeight;
            double fontSize;

            if (appSettings.IndexLength == "Short")
            {
                tabHeight = 90;
                fontSize = 10;
            }
            else if (appSettings.IndexLength == "Long")
            {
                tabHeight = 150;
                fontSize = 12;
            }
            else
            {
                tabHeight = 115;
                fontSize = 11;
            }

            CornerRadius cornerRadius = new CornerRadius(18, 0, 0, 18);

            MemoTabBorder1.Height = tabHeight;
            MemoTabBorder2.Height = tabHeight;
            MemoTabBorder3.Height = tabHeight;

            MemoTabText1.FontSize = fontSize;
            MemoTabText2.FontSize = fontSize;
            MemoTabText3.FontSize = fontSize;

            MemoTabBorder1.CornerRadius = cornerRadius;
            MemoTabBorder2.CornerRadius = cornerRadius;
            MemoTabBorder3.CornerRadius = cornerRadius;
        }
    }

}