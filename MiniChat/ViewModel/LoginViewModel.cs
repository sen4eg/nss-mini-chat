using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grpc.Net.Client;
using MiniChat.Model;
using MiniProtoImpl;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace MiniChat.ViewModel
{
    public partial class LoginViewModel : ObservableObject
    {
        private ClientState state = ClientState.GetState();

        [ObservableProperty]
        private string _username;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private bool _showPassword;

        [ObservableProperty]
        private string _responseText;

        public bool HidePassword => !ShowPassword;
        public bool IsLoginButtonEnabled => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
            
        public ICommand LoginCommand { get; }
        public ICommand ForgotPasswordCommand { get; }
        public ICommand CreateAccountCommand { get; }

        public LoginViewModel()
        {
            ForgotPasswordCommand = new RelayCommand(OnForgotPassword);
            CreateAccountCommand = new AsyncRelayCommand(OnCreateAccountAsync);
            LoginCommand = new AsyncRelayCommand(Login);
            Username = Password = "";
            ShowPassword = false;
        }

        partial void OnUsernameChanged(string value)
        {
            OnPropertyChanged(nameof(IsLoginButtonEnabled));
        }

        partial void OnPasswordChanged(string value)
        {
            OnPropertyChanged(nameof(IsLoginButtonEnabled));
        }

        private async Task Login()
        {
            try
            {
                state.Channel = GrpcChannel.ForAddress("http://localhost:5244"); // make sure to use right port, maybe add posibility to set it along with ip for development stage
                state.Client = new MiniProtoImpl.Chat.ChatClient(state.Channel);

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
                        Name = Username,
                        Password = Password,
                    },
                    Device = state.UserDevice
                });

                if (response.IsSucceed)
                {
                    state.SessionToken = response.Token;
                    state.RefreshToken = response.RefreshToken;

                    state.ConnectionObject = state.Client.InitiateAsyncChannel();

                    if (string.IsNullOrEmpty(state.SessionToken)) return;
                    await Shell.Current.GoToAsync(nameof(ConversationSelectionPage));

                    ResponseText = "";
                }
                else
                {
                    ResponseText = "Sorry, but your password is incorrect.";
                }
            }
            catch (Exception ex)
            {
                ResponseText = "Sorry, but your password is incorrect.";
            }

            Password = string.Empty;
        }

        private void OnForgotPassword()
        {
            App.Current.MainPage.DisplayAlert("Feature not implemented", "This feature is not implemented", "Bruh");
        }

        private async Task OnCreateAccountAsync()
        {
            await App.Current.MainPage.Navigation.PushAsync(new RegistrationPage());
        }

        partial void OnShowPasswordChanged(bool value)
        {
            OnPropertyChanged(nameof(HidePassword));
        }
    }
}
