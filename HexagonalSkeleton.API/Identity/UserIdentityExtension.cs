using System.Security.Claims;
using HexagonalSkeleton.CommonCore.Constants;

namespace HexagonalSkeleton.API.Identity
{
    public static class UserIdentityExtension
    {
        // Get the user id from the claims
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var nameId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(nameId, out var nameIdValue)) throw new Exception(AppErrorMessage.UserIdNotFound);

            return nameIdValue;
        }

    }
}
