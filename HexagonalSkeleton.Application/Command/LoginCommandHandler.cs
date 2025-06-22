using FluentValidation;
using HexagonalSkeleton.Application.Event;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Application.Extensions;
using HexagonalSkeleton.Domain.Ports;
using MediatR;

namespace HexagonalSkeleton.Application.Command
{
    /// <summary>
    /// Handles user login authentication and records login event
    /// Follows CQRS by handling the command side of login
    /// Now uses exceptions instead of IsValid pattern
    /// </summary>
    public class LoginCommandHandler(
        IValidator<LoginCommand> validator,
        IPublisher publisher,
        IUserReadRepository userReadRepository,
        IUserWriteRepository userWriteRepository,
        IAuthenticationService authenticationService)
        : IRequestHandler<LoginCommand, LoginCommandResult>
    {
        public async Task<LoginCommandResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {            // Validate the request - throw if invalid
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                throw new Exceptions.ValidationException(validationResult.ToDictionary());

            // Validate credentials - throw if invalid
            var isValid = await authenticationService.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
            if (!isValid)
                throw new AuthenticationException("Invalid email or password");

            // Get user - throw if not found
            var user = await userReadRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
                throw new NotFoundException("User", request.Email);

            // Record login through the aggregate - this will raise UserLoggedInEvent (domain event)
            user.RecordLogin();
            await userWriteRepository.UpdateAsync(user, cancellationToken);
            // Note: UserLoggedInEvent is automatically published by the repository after save

            // Generate token
            var accessToken = await authenticationService.GenerateJwtTokenAsync(user.Id, cancellationToken);

            // Publish application event for immediate coordination (e.g., update session, cache)
            // This is separate from the domain event and handles application-level concerns
            await publisher.Publish(new LoginEvent(user.Id), cancellationToken);
            
            // Return success result
            return new LoginCommandResult(accessToken);
        }
    }
}
