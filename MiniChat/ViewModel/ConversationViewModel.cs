using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniChat.Model;
using MiniProtoImpl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace MiniChat.ViewModel
{
    /// <summary>
    /// Page for displaying a single conversation. This conversation is supplied as a Query attribute.
    /// Without the query attribute, an Exception will occur
    /// </summary>
    public partial class ConversationViewModel : ObservableObject, IQueryAttributable
    {

        [ObservableProperty]
        private Conversation? conversationObject = null;

        private ClientState state = ClientState.GetState();

        //Entry textbox contents
        [ObservableProperty]
        String messageText = string.Empty;
        public ConversationViewModel()
        {

        }

        /// <summary>
        /// For editing messages: This is the message that is currently being edited.
        /// Is null if a new message is being written
        /// </summary>
        private Model.Message? editedMessage = null;

        [RelayCommand]
        void Submit()
        {
            if (string.IsNullOrWhiteSpace(MessageText))
            {
                return;
            }

            // ConversationObject should not be null thanks to the query attribute. This is just to shut up IntelliSense and prevent runtime errors. 
            if (ConversationObject == null)
            {
                Trace.TraceError("Error: Conversation is null!");
                return;
            }

            MiniProtoImpl.CommunicationRequest communicationRequest = new()
            {
                Token = state.SessionToken
            };

            // If no message is being edited, send a new message
            if (editedMessage == null)
            {
                communicationRequest.Message = new MiniProtoImpl.Message
                {
                    ReceiverId = ConversationObject.ContactID,
                    Content = MessageText,
                };
            }
            else
            {
                communicationRequest.EditMessage = new MiniProtoImpl.EditMessageRequest
                {
                    Id = editedMessage.Id,
                    Message = MessageText,
                };
                ClearEdit();
            }  
            state.RequestStream?.WriteAsync(communicationRequest);
            MessageText = string.Empty;
        }

        [RelayCommand]
        void MarkForEdit(Model.Message message)
        {
            editedMessage = message;
            MessageText = message.Contents;
        }

        [RelayCommand]
        void ClearEdit()
        {
            editedMessage = null;
        }

        private void RequestMessages() {
            Trace.WriteLine("RequestMessage was called");
            MiniProtoImpl.CommunicationRequest request = new()
            {
                Token = state.SessionToken,
                RequestDialog = new MiniProtoImpl.RequestDialog
                {
                    Count = 10,
                    DialogId = ConversationObject.ContactID,
                    LastMessageId = ConversationObject.LastMessage,
                    Offset = ConversationObject.Messages.Count
                }
            };

            state.RequestStream?.WriteAsync(request);
        }

        /// <summary>
        /// Called when this page is queried from the conversation selection page (by tapping the conversation)
        /// </summary>
        /// <param name="query">The pairs of query names and parameter objects to be passed to this viewmodel</param>
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            ConversationObject = (Conversation)query["ConversationObject"];
            OnPropertyChanged(nameof(ConversationObject));
            RequestMessages();
        }
    }
}
