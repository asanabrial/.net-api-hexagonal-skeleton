using FluentValidation;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Application.Event;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.Services;
using MediatR;

namespace HexagonalSkeleton.Application.Command
{
    public class RegisterUserCommandHandler(
        IValidator<RegisterUserCommand> validator,
        IPublisher publisher,
        IUserWriteRepository userWriteRepository,
        IUserReadRepository userReadRepository,
        IAuthenticationService authenticationService)
        : IRequestHandler<RegisterUserCommand, ResultDto>
    {
        public async Task<ResultDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return new ResultDto(result.ToDictionary());

            // Check uniqueness using domain service
            var isUnique = await UserDomainService.IsUserRegistrationDataUniqueAsync(
                request.Email, 
                request.PhoneNumber, 
                userReadRepository, 
                cancellationToken);
            
            if (!isUnique)
                return new ResultDto("Email or phone number already exists");

            // Validate password strength
            if (!UserDomainService.IsPasswordStrong(request.Password))
                return new ResultDto("Password does not meet strength requirements");

            var passwordSalt = authenticationService.GenerateSalt();
            var passwordHash = authenticationService.HashPassword(request.Password, passwordSalt);

            var user = UserDomainService.CreateUser(
                request.Email,
                passwordSalt,
                passwordHash,
                request.Name,
                request.Surname,
                request.Birthdate,
                request.PhoneNumber,
                request.Latitude,
                request.Longitude,
                request.AboutMe);

            var userId = await userWriteRepository.CreateAsync(user, cancellationToken);
            
            var jwtToken = await authenticationService.GenerateJwtTokenAsync(userId, cancellationToken);
            var response = new RegisterUserCommandResult(jwtToken);

            await publisher.Publish(new LoginEvent(userId), cancellationToken);
            return new ResultDto(response);
        }
    }
}
