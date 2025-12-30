
using System.ComponentModel.DataAnnotations;
using Library.Common.Base;

namespace Library.UserAPI.Models
{
    public class UserType : AuditBase, IArchivable
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Role { get; set; } = null!; //"Admin", "Normal"
        public List<User> Users { get; set; } = new();
    }
}
