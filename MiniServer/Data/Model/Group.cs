using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniServer.Data.Model;

public class Group
{
    [Key]
    public long GroupId { get; set; }

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