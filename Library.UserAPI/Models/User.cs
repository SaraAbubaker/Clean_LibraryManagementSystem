
using System.ComponentModel.DataAnnotations;
using Library.Common.Base;

namespace Library.UserAPI.Models
{
    public class User : AuditBase, IArchivable
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9_.-]+$",
            ErrorMessage = "Username must contain letters, numbers, underscore, dot and dash only.")]
        public string Username { get; set; } = null!;

        //Email validation
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = null!;

        //Password Validation
        [Required]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$",
            ErrorMessage = "Password must be at least 8 characters, contain upper, lower case, and a number.")]
        public string Password { get; set; } = null!;

        //Foreign Key
        public int UserTypeId { get; set; }

        //Navigation Property
        public UserType? UserType { get; set; }
    }
}