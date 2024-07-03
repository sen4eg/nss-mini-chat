namespace MiniChat;

using MiniChat.ViewModel;

public partial class ConversationSelectionPage : ContentPage
{
	public ConversationSelectionPage(ConversationSelectionViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}