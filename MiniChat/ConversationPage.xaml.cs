using MiniChat.ViewModel;

namespace MiniChat
{
    public partial class ConversationPage : ContentPage
    {
        public ConversationPage(ConversationViewModel vm)
        {
            BindingContext = vm;
            InitializeComponent();
        }
    }
}