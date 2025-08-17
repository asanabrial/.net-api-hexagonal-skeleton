using Microsoft.AspNetCore.Mvc;

namespace HexagonalSkeleton.API.Handler.ExceptionMapping
{
    /// <summary>
    /// Default exception mapper for unhandled exception types
    /// Follows Open/Closed Principle by providing safe fallback behavior
    /// </summary>
    public class DefaultExceptionMapper : IExceptionMapper
    {
        public bool CanHandle(Exception exception) => true;

        public ProblemDetails MapToProblemDetails(Exception exception, string requestPath)
        {
            return new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred. Please try again later.",
                Instance = requestPath
            };
        }
    }
}
