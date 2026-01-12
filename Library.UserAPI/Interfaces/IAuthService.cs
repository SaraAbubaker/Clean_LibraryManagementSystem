using Library.Common.RabbitMqMessages.UserMessages;

namespace Library.UserAPI.Interfaces
{
    public interface IAuthService
    {
        string GenerateJwtToken(UserListMessage user);
        string GenerateRefreshToken();
        int GetRefreshTokenLifetimeDays();
    }
}