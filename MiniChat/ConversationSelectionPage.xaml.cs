namespace MiniChat;

using MiniChat.ViewModel;

public partial class ConversationSelectionPage : ContentPage
{
	public ConversationSelectionPage()
	{
		InitializeComponent();
		BindingContext = new ConversationSelectionViewModel();
	}
}