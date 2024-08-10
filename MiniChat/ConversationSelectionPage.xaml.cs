using MiniChat.ViewModel;

namespace MiniChat
{
    public partial class ConversationSelectionPage : ContentPage
    {
        public ConversationSelectionPage(ConversationSelectionViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}