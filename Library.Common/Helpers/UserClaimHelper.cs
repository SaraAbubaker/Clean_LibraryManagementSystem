using System.Security.Claims;

namespace Library.Common.Helpers
{
    public static class UserClaimHelper
    {
        private const string UserIdClaimType = "userId";

        public static int GetUserClaim(ClaimsPrincipal user)
        {
            if (user == null)
                throw new UnauthorizedAccessException("User context is missing.");

            var claim = user.FindFirst(UserIdClaimType);

            if (claim == null)
                throw new UnauthorizedAccessException("UserId claim is missing.");

            if (!int.TryParse(claim.Value, out var userId))
                throw new UnauthorizedAccessException("UserId claim is invalid.");

            return userId;
        }
    }
}
