using System.ComponentModel.DataAnnotations;

namespace Library.Common.DTOs.UserApiDtos.TokenDtos
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}