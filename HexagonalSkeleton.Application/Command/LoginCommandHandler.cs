using FluentValidation;
using HexagonalSkeleton.Application.Events;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain.Ports;
using MediatR;
using AutoMapper;

namespace HexagonalSkeleton.Application.Command
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

        public LoginCommandHandler(
            IValidator<LoginCommand> validator,
            IPublisher publisher,
            IUserReadRepository userReadRepository,
            IUserWriteRepository userWriteRepository,
            IAuthenticationService authenticationService,
            IMapper mapper)
        {
            _validator = validator;
            _publisher = publisher;
            _userReadRepository = userReadRepository;
            _userWriteRepository = userWriteRepository;
            _authenticationService = authenticationService;
            _mapper = mapper;
        }        public async Task<AuthenticationDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // Validate the request - throw if invalid
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                throw new Exceptions.ValidationException(validationResult.ToDictionary());

            // Validate credentials - throw if invalid
            var isValid = await _authenticationService.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
            if (!isValid)
                throw new AuthenticationException("Invalid email or password");

            // Get user - throw if not found
            var user = await _userReadRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
                throw new NotFoundException("User", request.Email);

            // Record login using the dedicated method - this will raise UserLoggedInEvent (domain event)
            await _userWriteRepository.SetLastLoginAsync(user.Id, cancellationToken);
            // Note: UserLoggedInEvent is automatically published by the repository after save            // Generate token with expiration info
            var tokenInfo = await _authenticationService.GenerateJwtTokenAsync(user.Id, cancellationToken);

            // Publish application event for immediate coordination (e.g., update session, cache)
            // This is separate from the domain event and handles application-level concerns
            await _publisher.Publish(new LoginEvent(user.Id), cancellationToken);
            
            // Map user data to DTO and create authentication response
            var userDto = _mapper.Map<UserDto>(user);
            return new AuthenticationDto
            {
                AccessToken = tokenInfo.Token,
                ExpiresIn = tokenInfo.ExpiresIn,
                User = userDto
            };
        }
    }
}
