using System.ComponentModel.DataAnnotations;

namespace Library.Common.RabbitMqMessages.UserMessages
{
    public class LoginUserMessage
    {
        [Required]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
