using HexagonalSkeleton.Application.Features.UserAuthentication.Dto;
using HexagonalSkeleton.Domain;
using MediatR;

namespace HexagonalSkeleton.Application.Features.UserAuthentication.Commands
{
    /// <summary>
    /// Command to authenticate a user and record login
    /// </summary>
    public class LoginCommand : IRequest<AuthenticationDto>
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
