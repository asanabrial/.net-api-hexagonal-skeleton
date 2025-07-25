﻿using System.Security.Claims;
using HexagonalSkeleton.API.Constants;

namespace HexagonalSkeleton.API.Identity
{
    public static class UserIdentityExtension
    {
        // Get the user id from the claims
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var nameId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(nameId, out var nameIdValue)) throw new Exception(AppErrorMessage.UserIdNotFound);

            return nameIdValue;
        }

    }
}
