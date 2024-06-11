using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniServer.Data.Model;


public class GroupRole
{
    public GroupRole(string role, User user, Group group, List<Permission> permissions) {
        Role = role;
        User = user;
        Group = group;
        Permissions = permissions;
    }

    [Key]
    public int GroupRoleId { get; set; }

    [ForeignKey("UserId")]
    public int UserId { get; set; }

    [ForeignKey("GroupId")]
    public int GroupId { get; set; }

    [Required]
    public string Role { get; set; }

    // Navigation properties
    public virtual User User { get; set; }
    public virtual Group Group { get; set; }
    
    public ICollection<Permission> Permissions { get; set; }
}