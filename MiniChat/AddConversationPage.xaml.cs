using MiniChat.ViewModel;

namespace MiniChat;

public partial class AddConversationPage: ContentPage
{
	public AddConversationPage(AddConversationViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}