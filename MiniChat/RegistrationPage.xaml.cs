using MiniChat.ViewModel;

namespace MiniChat;

public partial class RegistrationPage : ContentPage
{
    public RegistrationPage()
    {
        InitializeComponent();
        BindingContext = new RegistrationViewModel();
    }
}