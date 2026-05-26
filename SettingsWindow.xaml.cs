using System.Windows;
using System.Windows.Controls;

namespace PeekMemo
{
    public partial class SettingsWindow : Window
    {
        private AppSettings appSettings;

        public SettingsWindow(AppSettings settings)
        {
            InitializeComponent();

            appSettings = settings;

            TitleTextBox.Text = appSettings.Indexes[0].Title;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            appSettings.Indexes[0].Title = TitleTextBox.Text;

            foreach (var child in ColorPanel.Children)
            {
                RadioButton radioButton = child as RadioButton;

                if (radioButton != null && radioButton.IsChecked == true)
                {
                    appSettings.Indexes[0].Color = radioButton.Tag.ToString();
                    break;
                }
            }

            SettingsService.Save(appSettings);

            this.DialogResult = true;
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}