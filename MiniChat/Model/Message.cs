using CommunityToolkit.Mvvm.ComponentModel;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MiniChat.Model
{
    public partial class Message(long id, long sender, long recipient, String contents, Timestamp timestamp) : ObservableObject
    {

        /// <summary>
        /// Constructor using the server definition for a message
        /// </summary>
        /// <param name="message">Message to convert</param>
        public Message(MiniProtoImpl.Message message) : this(message.Id, message.AuthorId, message.ReceiverId, message.Content, message.Timestamp) { }

        public long Id {get; private set;} = id;
        public long Sender { get; set; } = sender;

        public long Recipient = recipient;

        [ObservableProperty]
        private String contents = contents;
        public bool IsReceived { get => Recipient == ClientState.GetState().UserID; }
        public Timestamp Timestamp { get; internal set; } = timestamp;
    }
}


