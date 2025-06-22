using HexagonalSkeleton.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace HexagonalSkeleton.API.Handler.ExceptionMapping
{
    /// <summary>
    /// Maps domain exceptions to appropriate HTTP responses
    /// </summary>
    public class DomainExceptionMapper : IExceptionMapper
    {
        public bool CanHandle(Exception exception)
        {
            return exception is HexagonalSkeleton.Domain.Exceptions.DomainException;
        }        public ProblemDetails MapToProblemDetails(Exception exception, string requestPath)
        {
            return exception switch
            {
                UserDataNotUniqueException uniqueEx => new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Data conflict",
                    Detail = uniqueEx.Message,
                    Instance = requestPath,
                    Extensions = { ["email"] = uniqueEx.Email }
                },
                
                WeakPasswordException => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid password",
                    Detail = "Password does not meet strength requirements",
                    Instance = requestPath
                },
                
                InsufficientPermissionException permissionEx => new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Insufficient permissions",
                    Detail = permissionEx.Message,
                    Instance = requestPath,
                    Extensions = { 
                        ["userId"] = permissionEx.UserId,
                        ["action"] = permissionEx.Action,
                        ["resource"] = permissionEx.Resource
                    }
                },
                
                HexagonalSkeleton.Domain.Exceptions.DomainException domainEx => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Business rule violation",
                    Detail = domainEx.Message,
                    Instance = requestPath
                },
                
                _ => throw new ArgumentException($"Cannot handle exception of type {exception.GetType().Name}")
            };
        }
    }
}
