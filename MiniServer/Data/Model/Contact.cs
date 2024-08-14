using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniServer.Data.Model;

public class Contact
{
    [Key]
    public long Id { get; set; }

    // Other contact attributes can be added here

    // Foreign key for the user who owns this contact
    
    public long ContactId { get; set; }
    
    public long UserId { get; set; }

    // Navigation property for the user who owns this contact
    public User User { get; set; }

    // Foreign key for the type of this contact
    public int ContactTypeId { get; set; }

    // Navigation property for the type of this contact
    public ContactType ContactType { get; set; }
    
    [NotMapped]
    public Message LastMessage { get; set; }
}