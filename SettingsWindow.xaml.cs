using System.Windows;

namespace PeekMemo
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("설정 저장 준비!");
        }
    }
}