using Grpc.Net.Client;
using MiniChat.Model;
using MiniProtoImpl;

namespace MiniChat
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnConnectClicked(object sender, EventArgs e)
        {
            LoginBtn.IsEnabled = true;
        }

        private async void OnLoginClickedAsync(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(LoginPage));
        }
    }

}

