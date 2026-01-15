using System.ComponentModel.DataAnnotations;

namespace Library.UserAPI.Models
{
    public class Permission
    {
        public int Id { get; set; }

        [Required]
        public string PermissionName { get; set; } = string.Empty;
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
