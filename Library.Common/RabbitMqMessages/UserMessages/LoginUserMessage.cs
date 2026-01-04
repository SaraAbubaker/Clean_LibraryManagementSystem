using System.ComponentModel.DataAnnotations;

namespace Library.Common.RabbitMqMessages.UserMessages
{
    public class LoginUserMessage
    {
        [Required]
        public string UsernameOrEmail { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
