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
    public partial class RegistrationViewModel : ObservableObject
    {
        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string confirmPassword;

        public ICommand RegistrationCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        public RegistrationViewModel()
        {
            RegistrationCommand = new RelayCommand(OnRegistration);
            NavigateToLoginCommand = new RelayCommand(OnNavigateToLogin);
        }

        private void OnRegistration()
        {
            // Registration
        }

        private async void OnNavigateToLogin()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new LoginPage());
        }
    }
}
