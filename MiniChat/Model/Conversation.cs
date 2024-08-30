using MiniProtoImpl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniChat.Model
{
    public class Conversation(long contactID, Message? lastMessage, long unreadCount)//NOTE: this is the Class definition AND primary constructor (thank you C#)
    {

        public Conversation(Dialog dialog) : this(dialog.ContactId, new Message(dialog.LastMessage), dialog.UnreadCount)
        {
        }

        public ObservableCollection<Message> Messages { get; set; } = [lastMessage];
        public long ContactID = contactID;
        public long UnreadCount = unreadCount;

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
        public String ContactHandle
        {
            get => ContactID.ToString();
        }

        /// <summary>
        /// Contents of the most recent message in the conversation.
        /// To be used for displaying to the screen
        /// </summary>
        public String TopMessage
        {
            get => Messages.Last().Contents;
        }
    }
}
