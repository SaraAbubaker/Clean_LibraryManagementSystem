
using System.ComponentModel.DataAnnotations;

namespace Library.Common.RabbitMqMessages.TokenMessages
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}