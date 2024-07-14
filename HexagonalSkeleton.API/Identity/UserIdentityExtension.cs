using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HexagonalSkeleton.CommonCore.Constants;
using Microsoft.AspNetCore.Routing.Constraints;

namespace HexagonalSkeleton.API.Identity
{
    public static class UserIdentityExtension
    {
        // Get the user id from the claims
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var nameId = user.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;

            if (!int.TryParse(nameId, out var nameIdValue)) throw new Exception(AppErrorMessage.UserIdNotFound);

            return nameIdValue;
        }

    }
}
