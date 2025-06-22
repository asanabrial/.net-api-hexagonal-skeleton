using FluentValidation;
using HexagonalSkeleton.Application.Events;
using HexagonalSkeleton.Application.Exceptions;
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
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginCommandResult>
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
        }

        public async Task<LoginCommandResult> Handle(LoginCommand request, CancellationToken cancellationToken)
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

            // Record login through the aggregate - this will raise UserLoggedInEvent (domain event)
            user.RecordLogin();
            await _userWriteRepository.UpdateAsync(user, cancellationToken);
            // Note: UserLoggedInEvent is automatically published by the repository after save

            // Generate token
            var accessToken = await _authenticationService.GenerateJwtTokenAsync(user.Id, cancellationToken);

            // Publish application event for immediate coordination (e.g., update session, cache)
            // This is separate from the domain event and handles application-level concerns
            await _publisher.Publish(new LoginEvent(user.Id), cancellationToken);
            
            // Map user data to result using AutoMapper
            var result = _mapper.Map<LoginCommandResult>(user);
            result.AccessToken = accessToken; // Set the access token manually as it's not part of the user domain
            
            return result;
        }
    }
}
