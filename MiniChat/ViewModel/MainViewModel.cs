using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grpc.Net.Client;
using MiniChat.Model;
using System.Diagnostics;

namespace MiniChat.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        private ClientState state = ClientState.GetState();

        [ObservableProperty]
        public bool enableLogin = false;

        [ObservableProperty]
        public string serverIP = "http://localhost";

        [ObservableProperty]
        public int serverPort = 5244;

        /// <summary>
        /// Establish connection to the server
        /// </summary>
        [RelayCommand]
        void Connect()
        {
            try
            {
                // Initiate connection to server
                state.Channel = GrpcChannel.ForAddress(String.Concat(ServerIP, ":", ServerPort)); // make sure to use right port, maybe add posibility to set it along with ip for development stage
                state.Client = new MiniProtoImpl.Chat.ChatClient(state.Channel);

                state.UserDevice ??= new MiniProtoImpl.Device // if device is null, then:...
                {
                    Ip = "localhost", // TODO get device IP address
                    Name = DeviceInfo.Name,
                    Os = DeviceInfo.Platform.ToString()
                };
                state.ConnectionObject = state.Client.InitiateAsyncChannel();
                EnableLogin = true;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }

        [RelayCommand]
        async Task LoginButton()
        {
            await Shell.Current.GoToAsync(nameof(LoginPage));
        }

    }
}
