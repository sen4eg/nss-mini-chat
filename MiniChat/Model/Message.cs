using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MiniChat.Model
{
    public class Message(long id, String sender, String contents)
    {

        /// <summary>
        /// Constructor using the server definition for a message
        /// </summary>
        /// <param name="message">Message to convert</param>
        public Message(MiniProtoImpl.Message message) : this(message.Id, message.AuthorId.ToString(), message.Message_) { }

        public long Id {get; private set;} = id;
        public String Sender { get; set; } = sender;
        public String Contents { get; set; } = contents;
        // TODO add timestamp
    }
}
