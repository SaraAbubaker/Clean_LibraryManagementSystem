namespace Library.Common.RabbitMqMessages.UserMessages
{
    public class LoginUserResponseMessage
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;

        public DateTime LoggedInAt { get; set; } = DateTime.UtcNow;
        public List<string> Permissions { get; set; } = new();
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}