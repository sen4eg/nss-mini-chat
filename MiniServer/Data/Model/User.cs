using System.ComponentModel.DataAnnotations;

namespace MiniServer.Data.Model;

public class User {

    [Key] public int UserId { get; set; }

    [Required] public string Username { get; set; }

    [Required] public string Password { get; set; }

    [Required] public string Email { get; set; }

    // Navigation property for messages sent by this user
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();

    // Navigation property for groups this user belongs to
    public ICollection<GroupRole> GroupRoles { get; set; } = new List<GroupRole>();

    // Navigation property for contacts related to this user
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
}