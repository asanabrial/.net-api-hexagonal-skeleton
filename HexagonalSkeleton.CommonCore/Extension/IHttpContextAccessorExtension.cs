﻿using Microsoft.AspNetCore.Http;

namespace HexagonalSkeleton.CommonCore.Extension
{
    public static class HttpContextAccessorExtension
    {
        public static string? GetApiBaseUrl(this IHttpContextAccessor contextAccessor)
        {
            return contextAccessor?.HttpContext is null ? null : $"{contextAccessor.HttpContext.Request.Scheme}://{contextAccessor.HttpContext.Request.Host}{contextAccessor.HttpContext.Request.PathBase}";
        }
    }
}
