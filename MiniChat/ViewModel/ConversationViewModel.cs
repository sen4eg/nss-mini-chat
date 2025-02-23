﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniChat.Model;
using MiniProtoImpl;
using System.Diagnostics;

namespace MiniChat.ViewModel
{
    /// <summary>
    /// Page for displaying a single conversation. This conversation is supplied as a Query attribute.
    /// Without the query attribute, an Exception will occur
    /// </summary>
    public partial class ConversationViewModel : ObservableObject, IQueryAttributable
    {
        [ObservableProperty]
        private double messageCollectionDynamicHeight;

        public double WindowHeight;

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

        [ObservableProperty]
        private bool isEditing = false;

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
                communicationRequest.Message = new MiniProtoImpl.Message
                {
                    TargetId = editedMessage.Id,
                    ReceiverId = ConversationObject.ContactID,
                    Content = MessageText,
                    MsgType = 1
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
            IsEditing = true;
        }

        [RelayCommand]
        void ClearEdit()
        {
            editedMessage = null;
            MessageText = string.Empty;
            IsEditing = false;
        }

        [RelayCommand]
        void DeleteMessage(Model.Message message)
        {
            CommunicationRequest communicationRequest = new()
            {
                Token = state.SessionToken,
                Message = new MiniProtoImpl.Message
                {
                    TargetId = message.Id,
                    MsgType = 2,
                    ReceiverId = ConversationObject.ContactID
                }
            };

            state.RequestStream?.WriteAsync(communicationRequest);
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
                    LastMessageId = ConversationObject.LastMessageId,
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
