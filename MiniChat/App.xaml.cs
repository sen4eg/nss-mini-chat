using System.Diagnostics;

namespace MiniChat
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            // Force light theme
            MainPage = new AppShell();
            Current.UserAppTheme = AppTheme.Light;
        }
    }
}
