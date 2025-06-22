using FluentValidation;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Application.Event;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Application.Extensions;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.Services;
using HexagonalSkeleton.Domain.Exceptions;
using MediatR;

namespace HexagonalSkeleton.Application.Command
{
    public class RegisterUserCommandHandler(
        IValidator<RegisterUserCommand> validator,
        IPublisher publisher,
        IUserWriteRepository userWriteRepository,
        IUserReadRepository userReadRepository,
        IAuthenticationService authenticationService)        : IRequestHandler<RegisterUserCommand, RegisterUserCommandResult>
    {        public async Task<RegisterUserCommandResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                throw new Exceptions.ValidationException(result.ToDictionary());            // 1. Validate password strength (domain business rule)
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
                throw new InvalidOperationException("Failed to retrieve created user");
            
            var jwtToken = await authenticationService.GenerateJwtTokenAsync(userId, cancellationToken);
            await publisher.Publish(new LoginEvent(userId), cancellationToken);
            
            return new RegisterUserCommandResult(jwtToken)
            {
                Id = createdUser.Id,
                FirstName = createdUser.FullName.FirstName,
                LastName = createdUser.FullName.LastName,
                Email = createdUser.Email.Value,
                PhoneNumber = createdUser.PhoneNumber?.Value,
                Birthdate = createdUser.Birthdate,
                Latitude = createdUser.Location?.Latitude,
                Longitude = createdUser.Location?.Longitude,
                AboutMe = createdUser.AboutMe,
                CreatedAt = createdUser.CreatedAt
            };
        }
    }
}
