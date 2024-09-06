using CommunityToolkit.Mvvm.ComponentModel;
using MiniProtoImpl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MiniChat.Model
{
    public partial class Conversation(long contactID, string contactHandle, Message? lastMessage, long unreadCount) : ObservableObject //NOTE: this is the Class definition AND primary constructor (thank you C#)
    {

        public Conversation(Dialog dialog) : this(dialog.ContactId, dialog.ContactName, new Message(dialog.LastMessage), dialog.UnreadCount)
        {
        }

        [ObservableProperty]
        private ObservableCollection<Message> messages = lastMessage == null ? [] : [lastMessage];
        
        public void AddMessage (Message message)
        {
            int index = Messages.Count;

            while (index > 0)
            {
                if(Messages.ElementAt(index - 1).Timestamp > message.Timestamp)
                {
                    index--;
                }
                else
                {
                    break;
                }
            }
            Messages.Insert(index, message);
            UpdateTopMessage();
        }

        internal void MessageEdited(MiniProtoImpl.Message message)
        {
            for (int i = 0; i < Messages.Count; i++)
            {
                if (Messages[i].Id == message.TargetId)
                {
                    Messages[i].Contents = message.Content;
                    break;
                }
            }
            UpdateTopMessage();
        }

        internal void RemoveMessage(long targetId)
        {
            int index = -1;
            for (int i = 0; i < Messages.Count; i++)
            {
                if (Messages[i].Id == targetId)
                {
                    index = i;
                    break;
                }
            }
            if (index != -1)
            {
                Messages.RemoveAt(index);
            }

            
            UpdateTopMessage();
        }

        public long ContactID = contactID;
        public long UnreadCount = unreadCount;
        
        /// <summary>
        /// Last message object received, sorted by message timestamp.
        /// Used in making calls to the server. Not used to view
        /// For view Parameter, see TopMessage
        /// </summary>
        public Message? LastMessage
        {
            get => Messages.Count > 0 ? Messages.Last() : null;
        }

        /// <summary>
        /// ID of last message received
        /// </summary>
        public long LastMessageId
        {
            get => LastMessage != null ? LastMessage.Id : 0;
        }

        /// <summary>
        /// Screen name of the other participant or participants of the conversation
        /// </summary>
        public String ContactHandle { get; set; } = contactHandle;

        /// <summary>
        /// Contents of the most recent message in the conversation.
        /// To be used for displaying to the screen
        /// </summary>
        [ObservableProperty]
        public String topMessage = lastMessage == null ? String.Empty : lastMessage.Contents;

        /// <summary>
        /// Manually update top message for the view
        /// </summary>
        private void UpdateTopMessage()
        {
            TopMessage = LastMessage == null ? String.Empty : LastMessage.Contents;
        }
    }
}
