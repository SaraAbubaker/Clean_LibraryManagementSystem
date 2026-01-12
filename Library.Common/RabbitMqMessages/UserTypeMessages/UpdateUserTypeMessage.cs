using System.ComponentModel.DataAnnotations;

namespace Library.Common.RabbitMqMessages.UserTypeMessages
{
    public class UpdateUserTypeMessage
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Role { get; set; } = null!;
    }
}