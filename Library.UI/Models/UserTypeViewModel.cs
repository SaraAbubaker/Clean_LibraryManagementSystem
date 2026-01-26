using Library.Common.RabbitMqMessages.UserTypeMessages;

namespace Library.UI.Models
{
    public class UserTypeViewModel
    {
        public List<UserTypeListMessage> UserTypes { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }
}