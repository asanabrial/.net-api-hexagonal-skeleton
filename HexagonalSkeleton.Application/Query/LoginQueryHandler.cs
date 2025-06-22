using FluentValidation;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Domain.Ports;
using MediatR;

namespace HexagonalSkeleton.Application.Query
{
    /// <summary>
    /// This class handles a LoginQuery.
    /// Now uses exceptions instead of IsValid pattern for error handling.
    /// </summary>
    public class LoginQueryHandler(
        IValidator<LoginQuery> validator,
        IUserReadRepository userReadRepository,
        IAuthenticationService authenticationService)
        : IRequestHandler<LoginQuery, LoginQueryResult>
    {
        public async Task<LoginQueryResult> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.ToDictionary();
                throw new HexagonalSkeleton.Application.Exceptions.ValidationException(errors);
            }

            var isValid = await authenticationService.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
            if (!isValid)
                throw new AuthenticationException("Invalid email or password");

            var user = await userReadRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
                throw new NotFoundException("User", request.Email);            // Pure query - just generate token without side effects
            var tokenInfo = await authenticationService.GenerateJwtTokenAsync(user.Id, cancellationToken);

            return new LoginQueryResult(tokenInfo.Token);
        }
    }
}
