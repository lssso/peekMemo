using System.Windows;

namespace PeekMemo
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (!SingleInstanceManager.IsFirstInstance())
            {
                MessageBox.Show("PeekMemo가 이미 실행 중입니다.");

                Shutdown();
                return;
            }

            base.OnStartup(e);
        }
    }
}