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
    public long CreatorUserId { get; set; }

    // Navigation property for the user who created the group
    public User? CreatorUser { get; set; }

    // Navigation property for group settings
    public ICollection<GroupSetting> GroupSettings { get; set; } = new List<GroupSetting>();

    // Navigation property for group roles
    public ICollection<GroupRole> GroupRoles { get; set; } = new List<GroupRole>();

    public Group(String name, String description, long creatorUserId) {
        Name = name;
        Description = description;
        CreatorUserId = creatorUserId;
    }

    public Group(String name, String description, long creatorUserId, ICollection<long> memberIds) : this(name, description, creatorUserId){
        foreach (var memberId in memberIds) {
            GroupRoles.Add(new GroupRole(memberId, this, GroupRoleTypes.Member));
        }
    }

}