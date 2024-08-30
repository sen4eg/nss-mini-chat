using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MiniChat.Model
{
    public enum UserRelation
    {
        NOT_FRIENDS,
        FRIENDS,
        BLOCKED,
        UNKNOWN
    }
    public class User(long id, string username, UserRelation relation)
    {
        public User(long id) : this(id, string.Empty, UserRelation.UNKNOWN) { }
        public long Id { get; set; } = id;
        public string Username { get; set; } = username;
        public string Email { get; set; } = string.Empty;
        public UserRelation Relation { get; set; } = relation;
    }
}

