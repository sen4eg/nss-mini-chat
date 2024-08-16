using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniChat.Model;
using MiniProtoImpl;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MiniChat.ViewModel
{   
    /// <summary>
    /// Page that displays a selection of conversations.
    /// Clicking a conversation redirects to that conversation's individual page
    /// </summary>
    public partial class ConversationSelectionViewModel : ObservableObject
    {   

        private ClientState state;
        
        [ObservableProperty]
        ObservableCollection<MiniChat.Model.Conversation> conversations = [];

        public ConversationSelectionViewModel()
        {
            state = ClientState.GetState();
            conversations = state.Conversations;
            RequestMessages();
        }

        /// <summary>
        /// Navigate to the selected conversation
        /// </summary>
        /// <param name="conversation"> The particular conversation to navigate to </param>
        /// <returns></returns>
        [RelayCommand]
        async Task TapConversation(Conversation conversation)
        {
            Trace.WriteLine(conversation.ToString());
            await Shell.Current.GoToAsync(nameof(ConversationPage), new Dictionary<String, Object> { { "ConversationObject", conversation } });
        }

        private void RequestMessages()
        {
            CommunicationRequest communicationRequest = new()
            {
                Token = state.SessionToken,
                RequestUpdate = new RequestUpdate
                {
                    LastMessageId = 0,
                }
            };

            state.RequestStream?.WriteAsync(communicationRequest); // TODO what to do when request stream is null
        }
        
    }
}
