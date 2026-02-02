using System.ComponentModel.DataAnnotations;

namespace Library.Common.DTOs.UserApiDtos.UserTypeDtos
{
    public class UpdateUserTypeMessage
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Role { get; set; } = null!;
    }
}