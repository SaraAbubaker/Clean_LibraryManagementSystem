using System.ComponentModel.DataAnnotations;

namespace Library.Common.DTOs.UserApiDtos.UserTypeDtos
{
    public class CreateUserTypeMessage
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Role { get; set; } = null!;

    }
}
