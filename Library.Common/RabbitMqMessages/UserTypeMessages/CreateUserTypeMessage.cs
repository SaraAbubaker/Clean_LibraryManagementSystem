using System.ComponentModel.DataAnnotations;

namespace Library.Common.RabbitMqMessages.UserTypeMessages
{
    public class CreateUserTypeMessage
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Role { get; set; } = null!;

    }
}
