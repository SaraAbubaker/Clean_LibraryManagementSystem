using Library.Common.RabbitMqMessages.UserMessages;

namespace Library.User.Services.Interfaces
{
    public interface IAuthService
    {
        string GenerateJwtToken(LoginUserResponseMessage loginResponse);
        string GenerateRefreshToken();
        int GetRefreshTokenLifetimeDays();
        string HashToken(string refreshToken);
    }
}