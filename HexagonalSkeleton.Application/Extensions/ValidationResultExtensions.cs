using FluentValidation.Results;

namespace HexagonalSkeleton.Application.Extensions
{
    /// <summary>
    /// Extension methods for FluentValidation ValidationResult
    /// </summary>
    public static class ValidationResultExtensions
    {
        /// <summary>
        /// Converts FluentValidation ValidationResult to a dictionary format
        /// suitable for ValidationProblemDetails
        /// </summary>
        /// <param name="validationResult">The FluentValidation ValidationResult</param>
        /// <returns>Dictionary with field names as keys and error arrays as values</returns>
        public static IDictionary<string, string[]> ToDictionary(this ValidationResult validationResult)
        {
            return validationResult.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray()
                );
        }
    }
}
