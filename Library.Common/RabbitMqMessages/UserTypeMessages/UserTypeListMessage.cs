namespace Library.Common.RabbitMqMessages.UserTypeMessages
{
    public class UserTypeListMessage
    {
        public int Id { get; set; }
        public string Role { get; set; } = null!;
    }
}
