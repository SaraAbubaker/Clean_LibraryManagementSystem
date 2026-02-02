using System.Text.Json.Serialization;

namespace Library.Common.DTOs.UserApiDtos.UserDtos
{
    public class LoginUserResponseMessage
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;

        public DateTime LoggedInAt { get; set; } = DateTime.UtcNow;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;

        [JsonIgnore]
        public List<string> Permissions { get; set; } = new();
    }
}