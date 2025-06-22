using HexagonalSkeleton.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HexagonalSkeleton.API.Handler.ExceptionMapping
{    /// <summary>
    /// Maps validation exceptions to detailed error responses
    /// </summary>
    public class ValidationExceptionMapper : IExceptionMapper
    {
        public bool CanHandle(Exception exception)
        {
            return exception is ValidationException;
        }

        public ProblemDetails MapToProblemDetails(Exception exception, string requestPath)
        {
            if (exception is not ValidationException validationEx)
                throw new ArgumentException($"Cannot handle exception of type {exception.GetType().Name}");

            // Create a ProblemDetails with validation errors in extensions
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Instance = requestPath
            };

            // Add validation errors to extensions
            problemDetails.Extensions["errors"] = validationEx.Errors;
            
            return problemDetails;
        }
    }
}
