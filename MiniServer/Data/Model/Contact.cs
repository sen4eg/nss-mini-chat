using System.ComponentModel.DataAnnotations;

namespace MiniServer.Data.Model;

public class Contact
{
    [Key]
    public int ContactId { get; set; }

    // Other contact attributes can be added here

    // Foreign key for the user who owns this contact
    public int UserId { get; set; }

    // Navigation property for the user who owns this contact
    public User User { get; set; }

    // Foreign key for the type of this contact
    public int ContactTypeId { get; set; }

    // Navigation property for the type of this contact
    public ContactType ContactType { get; set; }
}