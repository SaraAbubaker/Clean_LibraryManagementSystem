namespace Library.Common.RabbitMqMessages.UserMessages
{
    public class LoginUserResponseMessage
    {
        public UserListMessage User { get; set; } = null!;
        public DateTime LoggedInAt { get; set; } = DateTime.UtcNow;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}