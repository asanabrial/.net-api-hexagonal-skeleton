using Microsoft.AspNetCore.Mvc;

namespace HexagonalSkeleton.API.Handler.ExceptionMapping
{
    /// <summary>
    /// Interface for mapping exceptions to ProblemDetails
    /// </summary>
    public interface IExceptionMapper
    {
        /// <summary>
        /// Determines if this mapper can handle the given exception
        /// </summary>
        bool CanHandle(Exception exception);

        /// <summary>
        /// Maps the exception to ProblemDetails
        /// </summary>
        ProblemDetails MapToProblemDetails(Exception exception, string requestPath);
    }
}
