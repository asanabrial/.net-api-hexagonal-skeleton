using HexagonalSkeleton.Application.Dto;
using MediatR;

namespace HexagonalSkeleton.Application.Query
{
    /// <summary>
    /// Query to validate user credentials without recording login
    /// Pure query operation following CQRS
    /// </summary>
    public class ValidateUserCredentialsQuery : IRequest<ResultDto>
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
