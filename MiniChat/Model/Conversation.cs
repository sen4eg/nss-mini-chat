using MiniProtoImpl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniChat.Model
{
    public class Conversation(long contactID, string contactHandle, Message? lastMessage, long unreadCount)//NOTE: this is the Class definition AND primary constructor (thank you C#)
    {

        public Conversation(Dialog dialog) : this(dialog.ContactId, dialog.ContactName, new Message(dialog.LastMessage), dialog.UnreadCount)
        {
        }
        private ObservableCollection<Message> messages = lastMessage == null ? [] : [lastMessage];
        public ObservableCollection<Message> Messages { get => messages; private set => messages = value; }
        public void AddMessage (Message message)
        {

            int index = 0;
            for (int i = 0; i < Messages.Count; i++)
            {
                if (Messages[i].Timestamp >= message.Timestamp)
                {
                    index = i;
                    break;
                }
            }
            Messages.Insert(index, message);
            //Messages.Add(message);
            //SortMessages();
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
        }

        /*        private void SortMessages()
                {
                    var sorted = Messages.OrderByDescending(message => message.Timestamp).ToList(); // Why descending bruh
                    Messages.Clear();
                    foreach (var message in sorted)
                    {
                        Messages.Add(message);
                    }
                }
        */
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
