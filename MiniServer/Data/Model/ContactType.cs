using System.ComponentModel.DataAnnotations;

namespace MiniServer.Data.Model;

public class ContactType
{
    [Key]
    public int ContactTypeId { get; set; }

    [Required]
    public string Type { get; set; }

    // Navigation property for contacts associated with this type
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
}