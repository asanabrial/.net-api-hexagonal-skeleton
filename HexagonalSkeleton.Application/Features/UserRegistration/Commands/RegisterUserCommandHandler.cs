using AutoMapper;
using FluentValidation;
using HexagonalSkeleton.Application.Events;
using HexagonalSkeleton.Application.Features.UserRegistration.Dto;
using HexagonalSkeleton.Application.Features.UserAuthentication.Dto;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain.Services;
using MediatR;

namespace HexagonalSkeleton.Application.Features.UserRegistration.Commands
{    public class RegisterUserCommandHandler(
        IValidator<RegisterUserCommand> validator,
        IPublisher publisher,
        IUserWriteRepository userWriteRepository,
        IUserReadRepository userReadRepository,
        IAuthenticationService authenticationService,
        IMapper mapper)
        : IRequestHandler<RegisterUserCommand, RegisterUserDto>
    {        public async Task<RegisterUserDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
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
            var userId = await userWriteRepository.CreateAsync(user, cancellationToken);            // Get the complete user data with generated values (ID, timestamps, etc.)
            var createdUser = await userReadRepository.GetByIdAsync(userId, cancellationToken);
            if (createdUser == null)
                throw new InvalidOperationException("Failed to retrieve created user");            // Generate JWT token with expiration info
            var tokenInfo = await authenticationService.GenerateJwtTokenAsync(userId, cancellationToken);
            await publisher.Publish(new LoginEvent(userId), cancellationToken);
              
            // Map user data to DTO and create authentication response
            var userDto = mapper.Map<AuthenticatedUserDto>(createdUser);
            return new RegisterUserDto
            {
                AccessToken = tokenInfo.Token,
                ExpiresIn = tokenInfo.ExpiresIn,
                User = userDto
            };
        }
    }
}
