using Library.Shared.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Library.Common.RabbitMqMessages.UserTypeMessages
{
    public class UpdateUserTypeMessage
    {
        [Required]
        [Positive]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Role { get; set; } = null!;

    }
}