using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniChat.Model;
using MiniChat.ViewModel;
using System.Windows.Input;

namespace MiniChat
{

    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            BindingContext = new LoginViewModel();
        }

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            ClientState.GetState().LogUserOut();
            base.OnNavigatedTo(args);
        }
    }
}
