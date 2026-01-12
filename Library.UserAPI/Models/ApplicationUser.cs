using Library.Common.Base;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Library.UserAPI.Models
{
    public class ApplicationUser : IdentityUser<int>, IArchivable
    {
        public int? CreatedByUserId { get; set; }
        [Required]
        public DateOnly CreatedDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        public int? LastModifiedByUserId { get; set; }
        public DateOnly? LastModifiedDate { get; set; }

        public bool IsArchived { get; set; } = false;
        public int? ArchivedByUserId { get; set; }
        public DateOnly? ArchivedDate { get; set; }

        public int? DeactivatedByUserId { get; set; }
        public DateTimeOffset? DeactivatedDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status =>
            IsArchived
                ? "Archived"
                : (LockoutEnd.HasValue && LockoutEnd > DateTimeOffset.UtcNow
                    ? "Deactivated"
                    : "Active");

        //navigation property for refresh tokens
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}