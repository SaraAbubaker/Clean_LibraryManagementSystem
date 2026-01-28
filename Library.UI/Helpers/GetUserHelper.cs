using System.Security.Claims;

namespace Library.UI.Helpers
{
    public static class GetUserHelper
    {
        public static int GetCurrentUserId(ClaimsPrincipal user)
        {
            var claim = user.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
            {
                throw new InvalidOperationException("Logged-in user ID not found in claims.");
            }
            return userId;
        }
    }
}