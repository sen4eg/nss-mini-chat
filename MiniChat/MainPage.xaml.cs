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
            // init
            try {
                ClientState state = ClientState.GetState();

                state.Channel = GrpcChannel.ForAddress("http://localhost:5244"); // make sure to use right port, maybe add posibility to set it along with ip for development stage
                state.Client = new MiniProtoImpl.Chat.ChatClient(state.Channel);

                // Get device information

                state.UserDevice ??= new MiniProtoImpl.Device // if device is null, then:...
                {
                    Ip = "localhost", // TODO get device IP address
                    Name = DeviceInfo.Name,
                    Os = DeviceInfo.Platform.ToString()
                };

                var response = state.Client.Connect(new MiniProtoImpl.ConnectRequest
                {
                    Credentials = new MiniProtoImpl.Credentials
                    {
                        Name = "Martin",
                        Password = "password",
                    },
                    Device = state.UserDevice
                });
                //channel.Dispose();

                response_field.Text = response.ToString();

                state.SessionToken = response.Token;
                state.RefreshToken = response.RefreshToken;

                state.ConnectionObject = state.Client.InitiateAsyncChannel();

                //connectionObject.RequestStream

                if (response.IsSucceed)
                {
                    LoginBtn.IsEnabled = true;
                }
            }catch (Exception ex) {
                response_field.Text = ex.Message;
                SemanticScreenReader.Announce(response_field.Text);
            }
            
        }

        private async void OnLoginClickedAsync(object sender, EventArgs e)
        {
            ClientState state = ClientState.GetState();
            if (String.IsNullOrEmpty(state.SessionToken)) return;
            await Shell.Current.GoToAsync(nameof(LoginPage));
        }
    }

}
