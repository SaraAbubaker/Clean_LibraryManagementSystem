using System.ComponentModel.DataAnnotations;
using Library.Common.Base;

namespace Library.UserAPI.Models
{
    public class User : AuditBase
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Username must contain letters, numbers, underscore, dot and dash only.")]
        public string Username { get; set; } = null!;

        // Email validation
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = null!;

        // Store only the hashed password
        [Required]
        public string HashedPassword { get; set; } = null!;

        // Foreign Key
        public int UserTypeId { get; set; }

        // Navigation Property
        public UserType? UserType { get; set; }

        // Deactivation fields
        public bool IsActive { get; set; } = true;
        public DateOnly? DeactivatedDate { get; set; }
        public int? DeactivatedByUserId { get; set; }
    }
}
