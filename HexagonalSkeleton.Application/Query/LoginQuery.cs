using HexagonalSkeleton.Application.Dto;
using MediatR;

namespace HexagonalSkeleton.Application.Query
{
    /// <summary>
    /// This class represents a command for logging in a user.
    /// </summary>
    public class LoginQuery(string email, string password) : IRequest<ResultDto>
    {
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string Email { get; set; } = email;

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; } = password;
    }
}
