using MiniChat.ViewModel;
using System.Diagnostics;

namespace MiniChat
{
    public partial class ConversationSelectionPage : ContentPage
    {
        ConversationSelectionViewModel viewModel;
        public ConversationSelectionPage(ConversationSelectionViewModel vm)
        {
            InitializeComponent();
            viewModel = vm;
            BindingContext = vm;
        }

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            Trace.WriteLine("OnNavigatedTo");
            viewModel.RequestMessages();
            base.OnNavigatedTo(args);
        }
        protected override void OnAppearing()
        {
            OnPropertyChanged(nameof(Model.Conversation));

            base.OnAppearing();
        }
    }
}