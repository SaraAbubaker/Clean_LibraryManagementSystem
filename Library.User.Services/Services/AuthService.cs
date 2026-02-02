using Library.Common.DTOs.UserApiDtos.UserDtos;
using Library.User.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Library.User.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;

        public AuthService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateJwtToken(LoginUserResponseMessage loginResponse)
        {
            // Read JWT settings from configuration
            var jwtKey = _config["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key is not configured.");
            var issuer = _config["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
            var audience = _config["Jwt:Audience"]
                ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
            var expiresInMinutes = _config.GetValue<int>("Jwt:ExpiresInMinutes", 60);

            // Create signing credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Base identity claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, loginResponse.Username ?? string.Empty),
                new Claim("userId", loginResponse.Id.ToString()),
                new Claim(ClaimTypes.Role, loginResponse.UserRole ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Email, loginResponse.Email ?? string.Empty),
                new Claim("loggedInAt", loginResponse.LoggedInAt.ToString("O")) // ISO 8601 format
            };

            if (loginResponse.Permissions != null)
            {
                foreach (var permission in loginResponse.Permissions)
                {
                    claims.Add(new Claim("Permission", permission));
                }
            }

            // Build token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //Generate a secure refresh token
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public int GetRefreshTokenLifetimeDays()
        {
            return _config.GetValue<int>("Jwt:RefreshTokenDays", 7);
        }

        public string HashToken(string token)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }
    }
}