using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grpc.Net.Client;
using MiniChat.Model;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace MiniChat.ViewModel
{
    public partial class RegistrationViewModel : ObservableObject
    {
        private ClientState state = ClientState.GetState();

        [ObservableProperty]
        private string _username;

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private string _confirmPassword;

        [ObservableProperty]
        private string _responseText = String.Empty;

        public bool IsRegistrationButtonEnabled => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(ConfirmPassword);

        public ICommand NavigateToLogin { get; }

        public RegistrationViewModel()
        {
            NavigateToLogin = new AsyncRelayCommand(OnNavigateToLoginAsync);
            Username = Email = Password = ConfirmPassword = "";
        }

        partial void OnUsernameChanged(string value)
        {
            OnPropertyChanged(nameof(IsRegistrationButtonEnabled));
        }

        partial void OnPasswordChanged(string value)
        {
            OnPropertyChanged(nameof(IsRegistrationButtonEnabled));
        }

        partial void OnEmailChanged(string value)
        {
            OnPropertyChanged(nameof(IsRegistrationButtonEnabled));
        }

        partial void OnConfirmPasswordChanged(string value)
        {
            OnPropertyChanged(nameof(IsRegistrationButtonEnabled));
        }

        private bool VerifyPasswords()
        {
            return Password == ConfirmPassword;
        }

        [RelayCommand]
        async Task Registration()
        {
            try
            {
                if (VerifyPasswords())
                {
                    var response = state.Client.Register(new MiniProtoImpl.RegisterRequest
                    {
                        Credentials = new MiniProtoImpl.Credentials
                        {
                            Name = Username,
                            Password = Password
                        },
                        Email = Email,
                        Device = state.UserDevice
                    });

                    if (response.IsSucceed)
                    {
                        // TODO make this a function to be reused when logging in!
                        state.LogUserIn(response.Token, response.RefreshToken);
                        //state.ConnectionObject = state.Client.InitiateAsyncChannel();
                        await Shell.Current.GoToAsync(nameof(ConversationSelectionPage));
                    }
                    else
                    {
                        Trace.WriteLine(response.ToString);
                        ResponseText = "Registration failed. Please try again.";
                    }
                }
                else
                {
                    ResponseText = "Passwords do not match.";
                }
            }
            catch (Exception ex)
            {
                ResponseText = "Registration failed. Please try again.";
            }

            Password = string.Empty;
            ConfirmPassword = string.Empty;
        }

        private async Task OnNavigateToLoginAsync()
        {
            await Shell.Current.GoToAsync(nameof(LoginPage));
        }
    }
}