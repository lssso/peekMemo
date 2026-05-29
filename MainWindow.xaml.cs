using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace PeekMemo
{
    public partial class MainWindow : Window
    {
        private int currentIndex = 0;
        private bool isPinned = false;
        private AppSettings appSettings;
        private bool isPinnedMode = false;
        private SettingsWindow openedSettingsWindow;
        private bool isResizingHeight = false;
        private Point resizeStartPoint;
        private double resizeStartHeight;

        public MainWindow()
        {
            InitializeComponent();

            PinButton.Content = "📌";
            PinButton.Opacity = 0.6;

            saveTimer = new DispatcherTimer();
            saveTimer.Interval = TimeSpan.FromSeconds(1);
            saveTimer.Tick += SaveTimer_Tick;

            appSettings = SettingsService.Load();

            ApplySettings();

            LoadMemo();

            this.MinHeight = 300;
            this.MaxWidth = this.Width;
            this.MinWidth = this.Width;
        }

        private DispatcherTimer saveTimer;

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && !isPinnedMode)
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
            if (appSettings.OpenMode == "Hover"
                && !isPinned
                && !isPinnedMode)
            {
                HideMemo();
            }
        }

        private void ShowMemo()
        {
            var workArea = SystemParameters.WorkArea;

            if (appSettings.Edge == "Left")
            {
                AnimateWindow(workArea.Left);
                this.Top = workArea.Top + (workArea.Height - this.Height) / 2;
            }
            else if (appSettings.Edge == "Top")
            {
                this.BeginAnimation(Window.LeftProperty, null);
                this.Left = workArea.Left + (workArea.Width - this.Width) / 2;
                AnimateTop(workArea.Top);
            }
            else
            {
                AnimateWindow(workArea.Right - this.Width);
                this.Top = workArea.Top + (workArea.Height - this.Height) / 2;
            }
        }

        private void HideMemo()
        {
            var workArea = SystemParameters.WorkArea;

            if (appSettings.Edge == "Left")
            {
                AnimateWindow(workArea.Left - this.Width + 32);
                this.Top = workArea.Top + (workArea.Height - this.Height) / 2;
            }
            else if (appSettings.Edge == "Top")
            {
                this.BeginAnimation(Window.LeftProperty, null);
                this.Left = workArea.Left + (workArea.Width - this.Width) / 2;
                AnimateTop(workArea.Top - this.Height + 32);
            }
            else
            {
                AnimateWindow(workArea.Right - 32);
                this.Top = workArea.Top + (workArea.Height - this.Height) / 2;
            }
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
            if (openedSettingsWindow != null)
            {
                openedSettingsWindow.Activate();
                return;
            }
            openedSettingsWindow = new SettingsWindow(appSettings, currentIndex);

            SettingsWindow settingsWindow = openedSettingsWindow;

            settingsWindow.Owner = this;
            settingsWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            if (appSettings.Edge == "Left")
            {
                settingsWindow.Left = this.Left + this.Width + 10;
            }
            else
            {
                settingsWindow.Left = this.Left - settingsWindow.Width - 10;
            }
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

            settingsWindow.Closed += (s, args) =>
            {
                openedSettingsWindow = null;
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

           
            MemoTabBorder3.Visibility =
            appSettings.VisibleIndexCount >= 3
            ? Visibility.Visible
            : Visibility.Collapsed;

            ApplyIndexLength();
            ApplyEdgeLayout();

            isPinned = false;
            SetWindowPositionInstant();
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

            ApplySettingsWithoutMoving();
        }

        private void ApplySettingsWithoutMoving()
        {
            ApplyIndexLength();
            ApplyEdgeLayout();

            MemoIndexSettings index1 = appSettings.Indexes[0];
            MemoIndexSettings index2 = appSettings.Indexes[1];
            MemoIndexSettings index3 = appSettings.Indexes[2];

            MemoTabText1.Text = index1.Title;
            MemoTabText2.Text = index2.Title;
            MemoTabText3.Text = index3.Title;

            Brush colorBrush1 = (Brush)new BrushConverter().ConvertFromString(index1.Color);
            Brush colorBrush2 = (Brush)new BrushConverter().ConvertFromString(index2.Color);
            Brush colorBrush3 = (Brush)new BrushConverter().ConvertFromString(index3.Color);

            MemoTabBorder1.Background = colorBrush1;
            MemoTabBorder2.Background = colorBrush2;
            MemoTabBorder3.Background = colorBrush3;

            MemoIndexSettings currentIndexSetting = appSettings.Indexes[currentIndex];
            Brush currentColorBrush = (Brush)new BrushConverter().ConvertFromString(currentIndexSetting.Color);

            MemoBodyBorder.Background = currentColorBrush;
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

        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            isPinnedMode = !isPinnedMode;

            if (isPinnedMode)
            {
                PinButton.Content = "📍";
                PinButton.Opacity = 1.0;
                PinButton.Background = Brushes.LightGoldenrodYellow;

                isPinned = true;
                ShowMemo();
            }
            else
            {
                PinButton.Content = "📌";
                PinButton.Opacity = 0.6;
                PinButton.Background = Brushes.White;

                isPinned = false;
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (!isPinnedMode)
            {
                isPinned = false;
                HideMemo();
            }
        }

        private void AnimateTop(double targetTop)
        {
            DoubleAnimation animation = new DoubleAnimation();

            animation.To = targetTop;
            animation.Duration = TimeSpan.FromMilliseconds(300);

            this.BeginAnimation(Window.TopProperty, animation);
        }
        private void SetWindowPositionInstant()
        {
            var workArea = SystemParameters.WorkArea;

            this.BeginAnimation(Window.LeftProperty, null);
            this.BeginAnimation(Window.TopProperty, null);

            if (appSettings.Edge == "Left")
            {
                // 좌측: 오른쪽 탭 32px만 보이게
                this.Left = workArea.Left - this.Width + 32;
                this.Top = workArea.Top + (workArea.Height - this.Height) / 2;
            }
            else if (appSettings.Edge == "Top")
            {
                // 상단: 아래쪽 32px만 보이게
                this.Left = workArea.Left + (workArea.Width - this.Width) / 2;
                this.Top = workArea.Top - this.Height + 32;
            }
            else
            {
                // 우측: 왼쪽 탭 32px만 보이게
                this.Left = workArea.Right - 32;
                this.Top = workArea.Top + (workArea.Height - this.Height) / 2;
            }
        }

        private void ApplyEdgeLayout()
        {
            if (appSettings.Edge == "Left")
            {
                // 좌측 배치: [메모][탭]
                Grid.SetColumn(MemoBodyContainer, 0);
                Grid.SetColumn(MemoTabsPanel, 1);

                TabColumn.Width = new GridLength(1, GridUnitType.Star);
                MemoColumn.Width = new GridLength(32);

                MemoBodyBorder.CornerRadius = new CornerRadius(20, 0, 0, 20);
                SetTabCornerRadius(new CornerRadius(0, 20, 20, 0));
            }
            else
            {
                // 우측 배치: [탭][메모]
                Grid.SetColumn(MemoTabsPanel, 0);
                Grid.SetColumn(MemoBodyContainer, 1);

                TabColumn.Width = new GridLength(32);
                MemoColumn.Width = new GridLength(1, GridUnitType.Star);

                MemoBodyBorder.CornerRadius = new CornerRadius(0, 20, 20, 0);
                SetTabCornerRadius(new CornerRadius(20, 0, 0, 20));
            }
        }

        private void SetTabCornerRadius(CornerRadius cornerRadius)
        {
            MemoTabBorder1.CornerRadius = cornerRadius;
            MemoTabBorder2.CornerRadius = cornerRadius;
            MemoTabBorder3.CornerRadius = cornerRadius;
        }

        private void ResizeBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isResizingHeight = true;
            resizeStartPoint = e.GetPosition(null);
            resizeStartHeight = this.Height;

            Mouse.Capture((IInputElement)sender);
        }

        private void ResizeBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isResizingHeight)
            {
                return;
            }

            Point currentPoint = e.GetPosition(null);
            double deltaY = currentPoint.Y - resizeStartPoint.Y;

            double newHeight = resizeStartHeight + deltaY;

            if (newHeight < 300)
            {
                newHeight = 300;
            }

            this.Height = newHeight;
        }

        private void ResizeBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isResizingHeight = false;
            Mouse.Capture(null);
        }
    }

}