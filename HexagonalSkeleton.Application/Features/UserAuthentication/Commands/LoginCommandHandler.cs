using FluentValidation;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Application.Common.Messaging;
using HexagonalSkeleton.Application.Features.UserAuthentication.Dto;
using HexagonalSkeleton.Application.Mapping;
using Microsoft.Extensions.Logging;

namespace HexagonalSkeleton.Application.Features.UserAuthentication.Commands
{
    /// <summary>
    /// Handles user login authentication and records login event
    /// Follows CQRS by handling the command side of login
    /// Now uses exceptions instead of IsValid pattern
    /// </summary>
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthenticationDto>
    {
        private readonly IValidator<LoginCommand> _validator;
        private readonly IUserWriteRepository _userWriteRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            IValidator<LoginCommand> validator,
            IUserWriteRepository userWriteRepository,
            IAuthenticationService authenticationService,
            ILogger<LoginCommandHandler> logger)
        {
            _validator = validator;
            _userWriteRepository = userWriteRepository;
            _authenticationService = authenticationService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }        public async Task<AuthenticationDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // Validate the request - throw if invalid
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                throw new Exceptions.ValidationException(validationResult.ToDictionary());

            // Get user first from write repository to ensure we have all fields including password hash and salt
            _logger.LogInformation("Fetching user with email: {Email}", request.Email);
            var user = await _userWriteRepository.GetUserByEmailAsync(request.Email, cancellationToken);
            
            if (user == null)
            {
                // Use the same generic failure as a wrong password to avoid account enumeration:
                // returning 404 for unknown emails would reveal which addresses are registered.
                _logger.LogWarning("User not found with email: {Email} in write repository", request.Email);
                throw new AuthenticationException("Invalid email or password");
            }
            
            _logger.LogInformation("User found in write repository: ID={UserId}, Email={Email}, HasSalt={HasSalt}, HasHash={HasHash}", 
                user.Id, user.Email?.Value, 
                !string.IsNullOrEmpty(user.PasswordSalt),
                !string.IsNullOrEmpty(user.PasswordHash));

            // Validate credentials directly - throw if invalid
            _logger.LogInformation("Validating credentials for user: {Email}", request.Email);
            var hashedPassword = _authenticationService.HashPassword(request.Password, user.PasswordSalt);
            var isValid = hashedPassword == user.PasswordHash;
            
            _logger.LogInformation("Password validation: HashedInputLength={InputLength}, StoredHashLength={StoredLength}, Match={IsMatch}", 
                hashedPassword?.Length ?? 0, user.PasswordHash?.Length ?? 0, isValid);
            
            if (!isValid)
            {
                _logger.LogWarning("Invalid credentials for user: {Email}", request.Email);
                throw new AuthenticationException("Invalid email or password");
            }
            
            _logger.LogInformation("Credentials validated successfully for user: {Email}", request.Email);

            // Record login using the dedicated method - this will raise UserLoggedInEvent (domain event)
            await _userWriteRepository.SetLastLoginAsync(user.Id, cancellationToken);
            // Note: UserLoggedInEvent is automatically published by the repository after save            
            
            // Generate token with expiration info (user should exist in read database for login)
            var tokenInfo = await _authenticationService.GenerateJwtTokenAsync(user.Id, cancellationToken);
            
            // Map user data to DTO and create authentication response
            var userDto = user.ToAuthenticatedUserDto();
            return new AuthenticationDto
            {
                AccessToken = tokenInfo.Token,
                ExpiresIn = tokenInfo.ExpiresIn,
                User = userDto
            };
        }
    }
}
