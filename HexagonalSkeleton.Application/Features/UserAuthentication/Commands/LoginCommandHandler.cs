using FluentValidation;
using HexagonalSkeleton.Application.Events;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Domain.Ports;
using MediatR;
using AutoMapper;
using HexagonalSkeleton.Application.Features.UserAuthentication.Dto;
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
        private readonly IPublisher _publisher;
        private readonly IUserReadRepository _userReadRepository;
        private readonly IUserWriteRepository _userWriteRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMapper _mapper;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            IValidator<LoginCommand> validator,
            IPublisher publisher,
            IUserReadRepository userReadRepository,
            IUserWriteRepository userWriteRepository,
            IAuthenticationService authenticationService,
            IMapper mapper,
            ILogger<LoginCommandHandler> logger)
        {
            _validator = validator;
            _publisher = publisher;
            _userReadRepository = userReadRepository;
            _userWriteRepository = userWriteRepository;
            _authenticationService = authenticationService;
            _mapper = mapper;
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
                _logger.LogWarning("User not found with email: {Email} in write repository", request.Email);
                throw new NotFoundException("User", request.Email);
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
            // Note: UserLoggedInEvent is automatically published by the repository after save            // Generate token with expiration info
            var tokenInfo = await _authenticationService.GenerateJwtTokenAsync(user.Id, cancellationToken);

            // Publish application event for immediate coordination (e.g., update session, cache)
            // This is separate from the domain event and handles application-level concerns
            await _publisher.Publish(new LoginEvent(user.Id), cancellationToken);
            
            // Map user data to DTO and create authentication response
            var userDto = _mapper.Map<AuthenticatedUserDto>(user);
            return new AuthenticationDto
            {
                AccessToken = tokenInfo.Token,
                ExpiresIn = tokenInfo.ExpiresIn,
                User = userDto
            };
        }
    }
}
