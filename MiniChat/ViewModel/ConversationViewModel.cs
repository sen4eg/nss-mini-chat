using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniChat.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniChat.ViewModel
{ 
    public partial class ConversationViewModel : ObservableObject, IQueryAttributable
    {

        [ObservableProperty]
        private Conversation? conversationObject = null;

        private ClientState state = ClientState.GetState();

        [ObservableProperty]
        String messageText = string.Empty;
        public ConversationViewModel()
        {
            
        }

        [RelayCommand]
        void SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(MessageText))
            {
                MiniProtoImpl.CommunicationRequest communicationRequest = new()
                {
                    Token = state.SessionToken,
                    Message = new MiniProtoImpl.Message
                    {
                        ReceiverId = ConversationObject.ContactID,
                        Message_ = MessageText,
                    }
                };

                state.RequestStream?.WriteAsync(communicationRequest);
                MessageText = string.Empty;
            }
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

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            ConversationObject = (Conversation)query["ConversationObject"];
            OnPropertyChanged(nameof(ConversationObject));
            RequestMessages();
        }
    }
}
