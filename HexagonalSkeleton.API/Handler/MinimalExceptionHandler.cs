using HexagonalSkeleton.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HexagonalSkeleton.API.Handler
{    /// <summary>
    /// Minimal exception handler - only handles exceptions that ASP.NET Core doesn't map automatically
    /// Most standard exceptions (ArgumentException, KeyNotFoundException, etc.) are handled automatically
    /// </summary>
    public class MinimalExceptionHandler(ILogger<MinimalExceptionHandler> logger) : IExceptionHandler
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
            CancellationToken cancellationToken)
        {
            // Handle ValidationException specially to include detailed errors
            if (exception is ValidationException validationEx)
            {
                logger.LogWarning("Handling validation exception with {ErrorCount} errors", validationEx.Errors.Count);

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/problem+json";

                var response = new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    title = "Validation failed",
                    status = StatusCodes.Status400BadRequest,
                    instance = context.Request.Path.Value,
                    errors = validationEx.Errors
                };                var responseJson = JsonSerializer.Serialize(response, SerializerOptions);
                await context.Response.WriteAsync(responseJson, cancellationToken);
                return true;
            }

            // Only handle exceptions that need custom HTTP status codes
            // Let ASP.NET Core handle the rest automatically
            var problemDetails = exception switch
            {
                
                NotFoundException notFoundEx => new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resource not found",
                    Detail = notFoundEx.Message,
                    Instance = context.Request.Path
                },
                
                AuthenticationException authEx => new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Authentication failed",
                    Detail = authEx.Message,
                    Instance = context.Request.Path
                },
                
                AuthorizationException authzEx => new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Authorization failed",
                    Detail = authzEx.Message,
                    Instance = context.Request.Path
                },
                
                BusinessException businessEx => new ProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Title = "Business rule violation",
                    Detail = businessEx.Message,
                    Instance = context.Request.Path
                },
                
                ConflictException conflictEx => new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Resource conflict",
                    Detail = conflictEx.Message,
                    Instance = context.Request.Path
                },
                
                TooManyRequestsException rateLimitEx => new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Too many requests",
                    Detail = rateLimitEx.Message,
                    Instance = context.Request.Path
                },
                
                // Let ASP.NET Core handle everything else
                _ => null
            };

            if (problemDetails == null)
                return false; // Let ASP.NET Core handle it

            logger.LogWarning("Handling custom exception: {ExceptionType} - {Message}", 
                exception.GetType().Name, exception.Message);

            context.Response.StatusCode = problemDetails.Status ?? 500;
            context.Response.ContentType = "application/problem+json";
            
            var json = JsonSerializer.Serialize(problemDetails, SerializerOptions);
            await context.Response.WriteAsync(json, cancellationToken);
            
            return true;
        }
    }
}
