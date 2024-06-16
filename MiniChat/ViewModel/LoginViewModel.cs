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
        private bool _isPasswordVisible;

        public bool IsPasswordHidden => !IsPasswordVisible;

        public ICommand LoginCommand { get; }
        public ICommand ForgotPasswordCommand { get; }
        public ICommand CreateAccountCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new AsyncRelayCommand(OnLoginAsync);
            ForgotPasswordCommand = new RelayCommand(OnForgotPassword);
            CreateAccountCommand = new AsyncRelayCommand(OnCreateAccountAsync);
            IsPasswordVisible = false;
        }

        private async Task OnLoginAsync()
        {
            if (Username == "admin" && Password == "password")
            {
                // Navigate to the next page
                await App.Current.MainPage.Navigation.PushAsync(new MainPage());
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Login Failed", "Invalid Username or Password", "OK");
            }
        }

        private void OnForgotPassword()
        {
            App.Current.MainPage.DisplayAlert("Login Failed", "Invalid Username or Password", "OK");
        }

        private async Task OnCreateAccountAsync()
        {
            await App.Current.MainPage.Navigation.PushAsync(new RegistrationPage());
        }

        partial void OnIsPasswordVisibleChanged(bool value)
        {
            OnPropertyChanged(nameof(IsPasswordHidden));
        }
    }
}
