using HexagonalSkeleton.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace HexagonalSkeleton.API.Handler.ExceptionMapping
{
    /// <summary>
    /// Maps application layer exceptions to appropriate HTTP responses
    /// </summary>
    public class ApplicationExceptionMapper : IExceptionMapper
    {
        public bool CanHandle(Exception exception)
        {
            return exception switch
            {
                NotFoundException => true,
                AuthenticationException => true,
                AuthorizationException => true,
                BusinessException => true,
                ConflictException => true,
                TooManyRequestsException => true,
                _ => false
            };
        }

        public ProblemDetails MapToProblemDetails(Exception exception, string requestPath)
        {
            return exception switch
            {
                NotFoundException notFoundEx => new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resource not found",
                    Detail = notFoundEx.Message,
                    Instance = requestPath
                },
                
                AuthenticationException authEx => new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Authentication failed",
                    Detail = authEx.Message,
                    Instance = requestPath
                },
                
                AuthorizationException authzEx => new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Authorization failed",
                    Detail = authzEx.Message,
                    Instance = requestPath
                },
                
                BusinessException businessEx => new ProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Title = "Business rule violation",
                    Detail = businessEx.Message,
                    Instance = requestPath
                },
                
                ConflictException conflictEx => new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Resource conflict",
                    Detail = conflictEx.Message,
                    Instance = requestPath
                },
                
                TooManyRequestsException rateLimitEx => new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Too many requests",
                    Detail = rateLimitEx.Message,
                    Instance = requestPath
                },
                
                _ => throw new ArgumentException($"Cannot handle exception of type {exception.GetType().Name}")
            };
        }
    }
}
