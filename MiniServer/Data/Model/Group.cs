using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniServer.Data.Model;

public class Group
{
    public Group(string name, string description, User creatorUser) {
        Name = name;
        Description = description;
        CreatorUser = creatorUser;
    }

    [Key]
    public int GroupId { get; set; }

    [Required]
    public string Name { get; set; }

    public string Description { get; set; }

    // Foreign key for the user who created the group
    public int CreatorUserId { get; set; }

    // Navigation property for the user who created the group
    public User CreatorUser { get; set; }

    // Navigation property for group settings
    public ICollection<GroupSetting> GroupSettings { get; set; } = new List<GroupSetting>();

    // Navigation property for group roles
    public ICollection<GroupRole> GroupRoles { get; set; } = new List<GroupRole>();
}