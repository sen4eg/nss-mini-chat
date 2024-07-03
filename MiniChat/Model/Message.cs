using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MiniChat.Model
{
    public class Message(String sender, String message)
    {
        public String Sender { get; set; } = sender;
        public String Contents { get; set; } = message;
    }
}
