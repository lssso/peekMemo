using peekMemo;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace PeekMemo
{
    public partial class MainWindow : Window
    {
        private bool isPinned = false;
        public MainWindow()
        {
            InitializeComponent();

            saveTimer = new DispatcherTimer();
            saveTimer.Interval = TimeSpan.FromSeconds(1);
            saveTimer.Tick += SaveTimer_Tick;

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

            File.WriteAllText("memo.txt", MemoTextBox.Text);

            SaveStatusText.Text =
                DateTime.Now.ToString("yy.MM.dd HH:mm") + " 저장됨";
        }

        private void LoadMemo()
        {
            if (File.Exists("memo.txt"))
            {
                MemoTextBox.Text = File.ReadAllText("memo.txt");
                SaveStatusText.Text = "저장된 메모 불러옴";
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
            ShowMemo();
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!isPinned)
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
            AnimateWindow(workArea.Right - 40);
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
            isPinned = true;
            ShowMemo();
        }
        private void MemoTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isPinned = true;
            ShowMemo();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }

    }

}