using Microsoft.AspNetCore.Mvc;
using AutoMapper;

namespace HexagonalSkeleton.API.Extensions
{
    /// <summary>
    /// Extension methods for handling API responses
    /// Now simplified - exceptions are handled by global exception handler
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Maps application result to API response and returns OK
        /// Exceptions are handled by the global exception handler
        /// </summary>
        public static IActionResult ToApiResult<TSource, TTarget>(this ControllerBase controller, TSource source, IMapper mapper)
        {
            var mapped = mapper.Map<TTarget>(source);
            return controller.Ok(mapped);
        }

        /// <summary>
        /// Maps application result to API response and returns OK
        /// Exceptions (including NotFound) are handled by the global exception handler
        /// </summary>
        public static IActionResult ToApiResultWithNotFound<TSource, TTarget>(this ControllerBase controller, TSource source, IMapper mapper)
        {
            var mapped = mapper.Map<TTarget>(source);
            return controller.Ok(mapped);
        }
    }
}
