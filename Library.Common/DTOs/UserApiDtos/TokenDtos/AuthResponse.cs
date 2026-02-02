namespace Library.Common.DTOs.UserApiDtos.TokenDtos
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
