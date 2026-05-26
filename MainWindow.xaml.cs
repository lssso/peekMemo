using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PeekMemo
{
    public partial class MainWindow : Window
    {
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
                this.Hide();
            }
        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
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
            var workArea = SystemParameters.WorkArea;

            this.Left = workArea.Right - this.Width - 20;
            this.Top = workArea.Top + (workArea.Height - this.Height) / 2;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetWindowPosition();
        }

    }

}