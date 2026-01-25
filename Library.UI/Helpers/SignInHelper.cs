using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Library.UI.Helpers
{
    public static class SignInHelper
    {
        public static async Task SignInWithJwtAsync(HttpContext httpContext, string jwtToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(jwtToken);

            // Copy all claims from JWT (includes role + permissions if embedded)
            var claims = jwt.Claims.ToList();

            // Add the raw token as a custom claim for later use
            claims.Add(new Claim("access_token", jwtToken));

            // Build cookie identity
            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            // Issue authentication cookie
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = jwt.ValidTo
                });
        }
    }
}