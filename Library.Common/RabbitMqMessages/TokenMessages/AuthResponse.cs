
namespace Library.Common.RabbitMqMessages.TokenMessages
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
