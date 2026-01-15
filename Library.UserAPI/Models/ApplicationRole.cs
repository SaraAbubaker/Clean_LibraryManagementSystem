
using Library.Common.Base;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Library.UserAPI.Models
{
    /*
      ApplicationRole extends IdentityRole<int>
      IdentityRole<int> already provides:
        - Id (primary key, int in this case)
        - Name (role name, e.g. "Admin", "Member")
        - NormalizedName
        - ConcurrencyStamp
    */

    public class ApplicationRole : IdentityRole<int>, IArchivable
    {
        [Required] 
        public override string? Name { get; set; } = string.Empty;

        public int? CreatedByUserId { get; set; }
        [Required]
        public DateOnly CreatedDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        public int? LastModifiedByUserId { get; set; }
        public DateOnly? LastModifiedDate { get; set; }

        public bool IsArchived { get; set; } = false;
        public int? ArchivedByUserId { get; set; }
        public DateOnly? ArchivedDate { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    }
}