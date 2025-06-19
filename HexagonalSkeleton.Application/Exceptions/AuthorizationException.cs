namespace HexagonalSkeleton.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when authorization fails
    /// ASP.NET Core automatically maps UnauthorizedAccessException to HTTP 403 Forbidden
    /// </summary>
    public class AuthorizationException : UnauthorizedAccessException
    {
        public AuthorizationException(string message) : base(message) { }
        public AuthorizationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
