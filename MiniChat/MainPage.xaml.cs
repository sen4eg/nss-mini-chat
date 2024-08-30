using Grpc.Net.Client;
using MiniChat.Model;
using MiniProtoImpl;
using System.Diagnostics;

namespace MiniChat
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        ClientState state = ClientState.GetState();

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnConnectClicked(object sender, EventArgs e)
        {
            try
            {
                // Initiate connection to server
                state.Channel = GrpcChannel.ForAddress("http://localhost:5244"); // make sure to use right port, maybe add posibility to set it along with ip for development stage
                state.Client = new MiniProtoImpl.Chat.ChatClient(state.Channel);

                state.UserDevice ??= new MiniProtoImpl.Device // if device is null, then:...
                {
                    Ip = "localhost", // TODO get device IP address
                    Name = DeviceInfo.Name,
                    Os = DeviceInfo.Platform.ToString()
                };
                state.ConnectionObject = state.Client.InitiateAsyncChannel();
                LoginBtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }

        private async void OnLoginClickedAsync(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(LoginPage));
        }
    }

}

