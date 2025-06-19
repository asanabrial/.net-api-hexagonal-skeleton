namespace HexagonalSkeleton.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when request rate limit is exceeded
    /// Maps to HTTP 429 Too Many Requests
    /// </summary>
    public class TooManyRequestsException : DomainException
    {
        public TooManyRequestsException(string message = "Too many requests") : base(message) { }
        public TooManyRequestsException(string message, Exception innerException) : base(message, innerException) { }
    }
}
