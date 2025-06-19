using System.Security.Authentication;

namespace HexagonalSkeleton.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when authentication fails
    /// ASP.NET Core automatically maps InvalidCredentialException to HTTP 401 Unauthorized
    /// </summary>
    public class AuthenticationException : InvalidCredentialException
    {
        public AuthenticationException(string message) : base(message) { }
        public AuthenticationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
