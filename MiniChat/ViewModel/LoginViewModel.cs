using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MiniChat.ViewModel
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _username;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private bool _showPassword;

        public bool HidePassword
        {
            get { return !ShowPassword; }
        }

        public ICommand LoginCommand { get; }
        public ICommand ForgotPasswordCommand { get; }
        public ICommand CreateAccountCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new AsyncRelayCommand(OnLoginAsync);
            ForgotPasswordCommand = new RelayCommand(OnForgotPassword);
            CreateAccountCommand = new AsyncRelayCommand(OnCreateAccountAsync);
            Username = Password = "";
            ShowPassword = false;
        }

        private bool verifyPassword()
        {
            //TODO password verification with server
            return Username == "admin" && Password == "password";
        }

        private async Task OnLoginAsync()
        {
            if (verifyPassword())
            {
                // Navigate to the next page
                await App.Current.MainPage.Navigation.PushAsync(new MainPage());
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Login Failed", "Invalid Username or Password", "OK");

                // Clear password field
                Password = "";
            }
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
            //Update display password property
            OnPropertyChanged(nameof(HidePassword));
        }
    }
}
