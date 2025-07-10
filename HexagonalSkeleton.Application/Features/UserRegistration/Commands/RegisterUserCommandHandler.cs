using FluentValidation;
using HexagonalSkeleton.Application.IntegrationEvents;
using HexagonalSkeleton.Application.Features.UserRegistration.Dto;
using HexagonalSkeleton.Application.Features.UserAuthentication.Dto;
using HexagonalSkeleton.Application.Services;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain.Services;
using MediatR;

namespace HexagonalSkeleton.Application.Features.UserRegistration.Commands
{    public class RegisterUserCommandHandler(
        IValidator<RegisterUserCommand> validator,
        IIntegrationEventService integrationEventService,
        IUserWriteRepository userWriteRepository,
        IUserReadRepository userReadRepository,
        IAuthenticationService authenticationService)
        : IRequestHandler<RegisterUserCommand, RegisterDto>
    {        public async Task<RegisterDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                throw new Exceptions.ValidationException(validationResult.ToDictionary());// 1. Validate password strength (domain business rule)
            UserDomainService.ValidatePasswordStrength(request.Password);

            // 2. Check uniqueness (domain business rule)
            var emailExists = await userReadRepository.ExistsByEmailAsync(request.Email, cancellationToken);
            var phoneExists = await userReadRepository.ExistsByPhoneNumberAsync(request.PhoneNumber, cancellationToken);
            UserDomainService.ValidateUserUniqueness(emailExists, phoneExists, request.Email, request.PhoneNumber);

            // 3. Create user (domain logic)
            var passwordSalt = authenticationService.GenerateSalt();
            var passwordHash = authenticationService.HashPassword(request.Password, passwordSalt);

            var user = UserDomainService.CreateUser(
                request.Email,
                passwordSalt,
                passwordHash,
                request.FirstName,
                request.LastName,
                request.Birthdate,
                request.PhoneNumber,
                request.Latitude,
                request.Longitude,
                request.AboutMe);

            // 4. Persist and complete workflow
            var userId = await userWriteRepository.CreateAsync(user, cancellationToken);
            
            // Generate JWT token with expiration info
            var tokenInfo = await authenticationService.GenerateJwtTokenAsync(userId, cancellationToken);
            
            // Publish simple integration event for CQRS synchronization
            var integrationEvent = new UserCreatedIntegrationEvent(
                userId,
                user.Email.Value,
                user.FullName.FirstName,
                user.FullName.LastName,
                user.PhoneNumber.Value,
                user.CreatedAt
            );
            
            await integrationEventService.PublishAsync(integrationEvent, cancellationToken);
            
            // Also publish login event for activity tracking
            var loginEvent = new UserLoggedInIntegrationEvent(userId, DateTime.UtcNow);
            await integrationEventService.PublishAsync(loginEvent, cancellationToken);
              
            // Map user data to DTO using the correct userId from the repository
            var userDto = new RegisterUserInfoDto
            {
                Id = userId, // Use the ID returned by the repository
                FirstName = user.FullName.FirstName,
                LastName = user.FullName.LastName,
                FullName = user.FullName.GetFullName(),
                Email = user.Email.Value,
                PhoneNumber = user.PhoneNumber.Value,
                Birthdate = user.Birthdate,
                Latitude = user.Location.Latitude,
                Longitude = user.Location.Longitude,
                AboutMe = user.AboutMe,
                ProfileImageName = user.ProfileImageName,
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt
            };
            return new RegisterDto
            {
                AccessToken = tokenInfo.Token,
                ExpiresIn = tokenInfo.ExpiresIn,
                User = userDto
            };
        }
    }
}
