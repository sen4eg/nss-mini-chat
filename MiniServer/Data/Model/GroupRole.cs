using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniServer.Data.Model;


public sealed class GroupRole
{
    [Key]
    public int GroupRoleId { get; set; }

    [ForeignKey("UserId")]
    public long UserId { get; set; }

    [ForeignKey("GroupId")]
    public long GroupId { get; set; }

    [Required]
    public int Role { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Group? Group { get; set; }

    public ICollection<Permission> Permissions { get; set; } = null!;
    public GroupRole() { }

    public GroupRole(long userId, Group group, GroupRoleTypes role) {
        UserId = userId;
        Group = group;
        Role = (int)role;
    }
}