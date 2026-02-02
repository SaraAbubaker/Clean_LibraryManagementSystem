using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Library.Common.DTOs.UserApiDtos.UserDtos
{
    public class LoginUserMessage
    {
        [Required]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

    }
}
