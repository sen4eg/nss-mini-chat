using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniServer.Data.Model;


public class GroupRole
{
    [Key]
    public int GroupRoleId { get; set; }

    [ForeignKey("UserId")]
    public long UserId { get; set; }

    [ForeignKey("GroupId")]
    public long GroupId { get; set; }

    [Required]
    public string Role { get; set; }

    // Navigation properties
    public virtual User User { get; set; }
    public virtual Group Group { get; set; }
    
    public ICollection<Permission> Permissions { get; set; }
}