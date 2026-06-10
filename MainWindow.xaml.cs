using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

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
        private Forms.NotifyIcon trayIcon;
        private bool isReallyClosing = false;
        private List<int> searchResults = new List<int>();
        private int currentSearchResultIndex = -1;
        private string lastSearchKeyword = "";

        private const int HOTKEY_ID_MEMO_1 = 9001;
        private const int HOTKEY_ID_MEMO_2 = 9002;
        private const int HOTKEY_ID_MEMO_3 = 9003;

        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;

        private const uint VK_1 = 0x31;
        private const uint VK_2 = 0x32;
        private const uint VK_3 = 0x33;

        private const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(
            IntPtr hWnd,
            int id,
            uint fsModifiers,
            uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(
            IntPtr hWnd,
            int id);

      
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

            Closing += MainWindow_Closing;

            this.MinHeight = 300;
            this.MaxWidth = this.Width;
            this.MinWidth = this.Width;
        }

        private DispatcherTimer saveTimer;

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
           
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                SearchPanel.Visibility = Visibility.Visible;

                SearchTextBox.Focus();
                Keyboard.Focus(SearchTextBox);
                SearchTextBox.SelectAll();

                e.Handled = true;
                return;
            }
            if (e.Key == Key.Escape && SearchPanel.Visibility == Visibility.Visible)
            {
                CloseSearchPanel();

                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape && !isPinnedMode)
            {
                isPinned = false;
                HideMemo();
            }
            
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!isReallyClosing)
            {
                e.Cancel = true;
                Hide();
                return;
            }

            UnregisterGlobalHotKeys();

            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }
        }

        private void UnregisterGlobalHotKeys()
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;

            UnregisterHotKey(handle, HOTKEY_ID_MEMO_1);
            UnregisterHotKey(handle, HOTKEY_ID_MEMO_2);
            UnregisterHotKey(handle, HOTKEY_ID_MEMO_3);
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
            string memoFilePath = GetCurrentMemoFilePath();

            if (File.Exists(memoFilePath))
            {
                MemoTextBox.Text = File.ReadAllText(memoFilePath);
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
            HideMemo();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetWindowPosition();

            ApplySettingsWithoutMoving();

            trayIcon = new Forms.NotifyIcon();

            string iconPath =
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Assets",
                    "PeekMemo.ico");

            trayIcon.Icon = new Drawing.Icon(iconPath);

            trayIcon.Text = "PeekMemo";

            trayIcon.Visible = true;

            trayIcon.DoubleClick += TrayIcon_DoubleClick;

            Forms.ContextMenuStrip trayMenu = new Forms.ContextMenuStrip();

            trayMenu.Items.Add("열기", null, (s, args) =>
            {
                Show();
                WindowState = WindowState.Normal;
                SetWindowPositionInstant();
                Activate();
            });

            trayMenu.Items.Add("종료", null, (s, args) =>
            {
                isReallyClosing = true;

                if (trayIcon != null)
                {
                    trayIcon.Visible = false;
                    trayIcon.Dispose();
                }

                Close();
            });

            trayIcon.ContextMenuStrip = trayMenu;

            RegisterGlobalHotKeys();
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
        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();

            WindowState = WindowState.Normal;

            SetWindowPositionInstant();

            Activate();
        }

        private void ShowMemo()
        {
            var workArea = GetTargetWorkArea();

            if (appSettings.Edge == "Left")
            {
                AnimateWindow(workArea.Left);
                this.Top = GetTopByAlignment();
            }
            else
            {
                AnimateWindow(workArea.Right - this.Width);
                this.Top = GetTopByAlignment();
            }
        }

        private void HideMemo()
        {
            var workArea = GetTargetWorkArea();

            if (appSettings.Edge == "Left")
            {
                AnimateWindow(workArea.Left - this.Width + 32);
                this.Top = GetTopByAlignment();
            }
            else
            {
                AnimateWindow(workArea.Right - 32);
                this.Top = GetTopByAlignment();
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

                if (currentIndex >= appSettings.VisibleIndexCount)
                {
                    currentIndex = appSettings.VisibleIndexCount - 1;
                }

                if (currentIndex < 0)
                {
                    currentIndex = 0;
                }

                LoadMemo();
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
            if (currentIndex >= appSettings.VisibleIndexCount)
            {
                currentIndex = appSettings.VisibleIndexCount - 1;
            }

            if (currentIndex < 0)
            {
                currentIndex = 0;
            }

            MemoIndexSettings index1 = appSettings.Indexes[0];
            MemoIndexSettings index2 = appSettings.Indexes[1];
            MemoIndexSettings index3 = appSettings.Indexes[2];

            MemoTabText1.Text = index1.Title;
            MemoTabText2.Text = index2.Title;
            MemoTabText3.Text = index3.Title;

            Brush colorBrush1 = GetSafeBrush(index1.Color);
            Brush colorBrush2 = GetSafeBrush(index2.Color);
            Brush colorBrush3 = GetSafeBrush(index3.Color);

            MemoTabBorder1.Background = colorBrush1;
            MemoTabBorder2.Background = colorBrush2;
            MemoTabBorder3.Background = colorBrush3;

            MemoIndexSettings currentIndexSetting = appSettings.Indexes[currentIndex];

            Brush currentColorBrush = GetSafeBrush(currentIndexSetting.Color);

            MemoBodyBorder.Background = currentColorBrush;


            MemoTabBorder2.Visibility =
                appSettings.VisibleIndexCount >= 2
                    ? Visibility.Visible
                    : Visibility.Collapsed;

         
            MemoTabBorder3.Visibility =
            appSettings.VisibleIndexCount >= 3
            ? Visibility.Visible
            : Visibility.Collapsed;


            ApplyIndexLength();
            ApplyEdgeLayout();

            bool wasPinnedMode = isPinnedMode;

            isPinned = false;

            SetWindowPositionInstant();

            if (wasPinnedMode)
            {
                ShowMemo();
            }
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

            Brush colorBrush1 = GetSafeBrush(index1.Color);
            Brush colorBrush2 = GetSafeBrush(index2.Color);
            Brush colorBrush3 = GetSafeBrush(index3.Color);

            MemoTabBorder1.Background = colorBrush1;
            MemoTabBorder2.Background = colorBrush2;
            MemoTabBorder3.Background = colorBrush3;

            MemoIndexSettings currentIndexSetting = appSettings.Indexes[currentIndex];
            Brush currentColorBrush = GetSafeBrush(currentIndexSetting.Color);

            MemoBodyBorder.Background = currentColorBrush;
        }

        private void SaveMemo()
        {
            File.WriteAllText(GetCurrentMemoFilePath(), MemoTextBox.Text);
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

        private void SetWindowPositionInstant()
        {
            var workArea = GetTargetWorkArea();

            this.BeginAnimation(Window.LeftProperty, null);
            this.BeginAnimation(Window.TopProperty, null);

            if (appSettings.Edge == "Left")
            {
                this.Left = workArea.Left - this.Width + 32;
                this.Top = GetTopByAlignment();
            }
            else
            {
                this.Left = workArea.Right - 32;
                this.Top = GetTopByAlignment();
            }
        }

        private void ApplyEdgeLayout()
        {
            if (appSettings.Edge == "Left")
            {
              
                // 좌측: [메모][탭]
                Grid.SetColumn(MemoBodyContainer, 0);
                Grid.SetColumn(MemoTabsPanel, 1);

                LeftColumn.Width = new GridLength(1, GridUnitType.Star);
                RightColumn.Width = new GridLength(32);

                MemoBodyBorder.CornerRadius = new CornerRadius(20, 0, 0, 20);
                SetTabCornerRadius(new CornerRadius(0, 20, 20, 0));

                MemoTabsPanel.LayoutTransform = null;
                MemoTabsPanel.VerticalAlignment = VerticalAlignment.Center;
            } else
            {
                // 우측: [탭][메모]
                Grid.SetColumn(MemoTabsPanel, 0);
                Grid.SetColumn(MemoBodyContainer, 1);

                LeftColumn.Width = new GridLength(32);
                RightColumn.Width = new GridLength(1, GridUnitType.Star);

                MemoBodyBorder.CornerRadius = new CornerRadius(0, 20, 20, 0);
                SetTabCornerRadius(new CornerRadius(20, 0, 0, 20));

                MemoTabsPanel.LayoutTransform = null;
                MemoTabsPanel.VerticalAlignment = VerticalAlignment.Center;
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

        private double GetTopByAlignment()
        {
            var workArea = GetTargetWorkArea();

            if (appSettings.Alignment == "Top")
            {
                return workArea.Top + 40;
            }

            if (appSettings.Alignment == "Bottom")
            {
                return workArea.Bottom - this.Height - 40;
            }

            return workArea.Top + (workArea.Height - this.Height) / 2;
        }

        private string GetCurrentMemoFilePath()
        {
            return Path.Combine(
                DataFolderManager.GetDataFolder(),
                GetCurrentMemoFileName());
        }

        private void HideToTrayButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
        private void RegisterGlobalHotKeys()
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;

            HwndSource source = HwndSource.FromHwnd(handle);
            source.AddHook(HwndHook);

            RegisterHotKey(handle, HOTKEY_ID_MEMO_1, MOD_CONTROL | MOD_SHIFT, VK_1);
            RegisterHotKey(handle, HOTKEY_ID_MEMO_2, MOD_CONTROL | MOD_SHIFT, VK_2);
            RegisterHotKey(handle, HOTKEY_ID_MEMO_3, MOD_CONTROL | MOD_SHIFT, VK_3);
        }

        private IntPtr HwndHook( IntPtr hwnd, int msg,IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int hotKeyId = wParam.ToInt32();

                if (hotKeyId == HOTKEY_ID_MEMO_1)
                {
                    OpenMemoByHotKey(0);
                    handled = true;
                }
                else if (hotKeyId == HOTKEY_ID_MEMO_2)
                {
                    OpenMemoByHotKey(1);
                    handled = true;
                }
                else if (hotKeyId == HOTKEY_ID_MEMO_3)
                {
                    OpenMemoByHotKey(2);
                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        private void OpenMemoByHotKey(int index)
        {
            if (index >= appSettings.VisibleIndexCount)
            {
                return;
            }

            Show();

            WindowState = WindowState.Normal;

            SwitchMemo(index);

            ShowMemo();

            Activate();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            BuildSearchResults();
        }

        private void BuildSearchResults()
        {
            searchResults.Clear();
            currentSearchResultIndex = -1;

            string keyword = SearchTextBox.Text;
            lastSearchKeyword = keyword;

            if (string.IsNullOrWhiteSpace(keyword))
            {
                return;
            }

            string text = MemoTextBox.Text;
            int startIndex = 0;

            while (startIndex < text.Length)
            {
                int foundIndex = text.IndexOf(
                    keyword,
                    startIndex,
                    StringComparison.OrdinalIgnoreCase);

                if (foundIndex < 0)
                {
                    break;
                }

                searchResults.Add(foundIndex);
                startIndex = foundIndex + keyword.Length;
            }

            if (searchResults.Count > 0)
            {
                currentSearchResultIndex = 0;

                SelectSearchResult();
            }
            else
            {
                SearchResultText.Text = "0 / 0";
            }
        }

        private void MoveToNextSearchResult()
        {
            if (searchResults.Count == 0)
            {
                return;
            }

            if (currentSearchResultIndex < 0)
            {
                currentSearchResultIndex = 0;
            }
            else
            {
                currentSearchResultIndex++;

                if (currentSearchResultIndex >= searchResults.Count)
                {
                    currentSearchResultIndex = 0;
                }
            }

            SelectSearchResult();
        }

        private void MoveToPreviousSearchResult()
        {
            if (searchResults.Count == 0)
            {
                return;
            }

            if (currentSearchResultIndex < 0)
            {
                currentSearchResultIndex = searchResults.Count - 1;
            }
            else
            {
                currentSearchResultIndex--;

                if (currentSearchResultIndex < 0)
                {
                    currentSearchResultIndex = searchResults.Count - 1;
                }
            }

            SelectSearchResult();
        }

        private void SelectSearchResult()
        {
            string keyword = SearchTextBox.Text;

            if (string.IsNullOrWhiteSpace(keyword))
            {
                return;
            }

            int index = searchResults[currentSearchResultIndex];

            MemoTextBox.Focus();

            MemoTextBox.Select(index, keyword.Length);

            MemoTextBox.ScrollToLine(
                MemoTextBox.GetLineIndexFromCharacterIndex(index));

            SearchResultText.Text =
                $"{currentSearchResultIndex + 1} / {searchResults.Count}";

            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    SearchTextBox.Focus();
                    SearchTextBox.CaretIndex = SearchTextBox.Text.Length;
                }),
                DispatcherPriority.Background);
        }

        private void CloseSearchButton_Click(object sender, RoutedEventArgs e)
        {
            CloseSearchPanel();
        }

        private void CloseSearchPanel()
        {
            SearchPanel.Visibility = Visibility.Collapsed;
            SearchTextBox.Text = "";
            MemoTextBox.Focus();
        }

        private void NextSearchButton_Click(object sender, RoutedEventArgs e)
        {
            MoveSearchResult(1);
        }

        private void PreviousSearchButton_Click(object sender, RoutedEventArgs e)
        {
            MoveSearchResult(-1);
        }

        private void MoveSearchResult(int direction)
        {
            string keyword = SearchTextBox.Text;

            if (string.IsNullOrWhiteSpace(keyword))
            {
                SearchResultText.Text = "0 / 0";
                return;
            }

            if (keyword != lastSearchKeyword)
            {
                BuildSearchResults();
            }

            if (searchResults.Count == 0)
            {
                SearchResultText.Text = "0 / 0";
                return;
            }

            if (currentSearchResultIndex < 0)
            {
                currentSearchResultIndex =
                    direction > 0 ? 0 : searchResults.Count - 1;
            }
            else
            {
                currentSearchResultIndex += direction;

                if (currentSearchResultIndex >= searchResults.Count)
                {
                    currentSearchResultIndex = 0;
                }

                if (currentSearchResultIndex < 0)
                {
                    currentSearchResultIndex = searchResults.Count - 1;
                }
            }

            SelectSearchResult();
        }

        private Rect GetTargetWorkArea()
        {
            Forms.Screen[] screens = Forms.Screen.AllScreens;

            Forms.Screen targetScreen = Forms.Screen.PrimaryScreen;

            foreach (Forms.Screen screen in screens)
            {
                if (appSettings.Monitor == screen.DeviceName)
                {
                    targetScreen = screen;
                    break;
                }
            }

            return new Rect(
                targetScreen.WorkingArea.Left,
                targetScreen.WorkingArea.Top,
                targetScreen.WorkingArea.Width,
                targetScreen.WorkingArea.Height);
        }

        private Brush GetSafeBrush(string colorCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(colorCode))
                {
                    return Brushes.LightYellow;
                }

                Brush brush =
                    (Brush)new BrushConverter().ConvertFromString(colorCode);

                return brush ?? Brushes.LightYellow;
            }
            catch
            {
                return Brushes.LightYellow;
            }
        }



    }

}