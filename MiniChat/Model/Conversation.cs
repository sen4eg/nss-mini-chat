using CommunityToolkit.Mvvm.ComponentModel;
using MiniProtoImpl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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
            Messages.Add(message);
            SortMessages();
            //Trace.WriteLine(String.Format("New top message is {0}", TopMessage));
        }

        private void SortMessages()
        {
            var sorted = Messages.OrderByDescending(message => message.Timestamp).ToList(); // Why descending bruh
            Messages.Clear();
            foreach (var message in sorted)
            {
                Messages.Add(message);
            }
        }

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
        public String ContactHandle { get; set; } = contactHandle;

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
