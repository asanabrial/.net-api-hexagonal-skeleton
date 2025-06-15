using FluentValidation;
using HexagonalSkeleton.API.Config;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Application.Event;
using HexagonalSkeleton.CommonCore.Auth;
using HexagonalSkeleton.Domain;
using MediatR;
using Microsoft.Extensions.Options;

namespace HexagonalSkeleton.Application.Command
{
    public class RegisterUserCommandHandler(
        IValidator<RegisterUserCommand> validator,
        IPublisher publisher,
        IUserRepository unitOfWork,
        IOptions<AppSettings> appSettings)
        : IRequestHandler<RegisterUserCommand, ResultDto>
    {
        public async Task<ResultDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return new ResultDto(result.ToDictionary());

            var entity = request.ToDomainEntity();
            
            entity.PasswordSalt = PasswordHasher.GenerateSalt();
            entity.PasswordHash = PasswordHasher.ComputeHash(request.Password, entity.PasswordSalt, appSettings.Value.Pepper);

            await unitOfWork.CreateUserAsync(entity, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var response = await GenerateJwtToken(entity.Id);

            await publisher.Publish(new LoginEvent(entity.Id), cancellationToken);
            return new ResultDto(response);
        }

        private async Task<RegisterUserCommandResult> GenerateJwtToken(int id)
        {
            return await Task.FromResult(new RegisterUserCommandResult(JwToken.GenerateJwtToken(
                id.ToString(),
                appSettings.Value.Jwt.Secret,
                appSettings.Value.Jwt.Issuer,
                appSettings.Value.Jwt.Audience)));
        }
    }
}
