using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniServer.Data.Model;

public class Permission
{
    public Permission(string permissionName, string description, GroupRole groupRole) {
        PermissionName = permissionName;
        Description = description;
        GroupRole = groupRole;
    }

    [Key]
    public int PermissionId { get; set; } // Primary key
    public string PermissionName { get; set; } // Name of the permission
    public string Description { get; set; } // Description of the permission

    [ForeignKey("GroupRoleId")]
    public int GroupRoleId { get; set; }
    public GroupRole GroupRole { get; set; } // Navigation property for GroupRole
}