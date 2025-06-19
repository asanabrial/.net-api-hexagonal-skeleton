namespace HexagonalSkeleton.API.Models.Common
{
    /// <summary>
    /// Base response for API operations
    /// Provides consistent structure across all endpoints
    /// </summary>
    public abstract class BaseApiResponse
    {
        /// <summary>
        /// Timestamp when the response was generated
        /// </summary>
        public DateTime Timestamp { get; } = DateTime.UtcNow;

        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Optional message for additional context
        /// </summary>
        public string? Message { get; set; }
    }
}
