using FluentValidation;
using HexagonalSkeleton.API.Config;
using HexagonalSkeleton.API.Data;
using HexagonalSkeleton.API.Features.User.Application.Event;
using HexagonalSkeleton.CommonCore.Auth;
using MediatR;
using Microsoft.Extensions.Options;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    public class RegisterUserCommandHandler(
        IValidator<RegisterUserCommand> validator,
        IPublisher publisher,
        IUnitOfWork unitOfWork,
        IOptions<AppSettings> appSettings)
        : IRequestHandler<RegisterUserCommand, IResult>
    {
        public async Task<IResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return Results.ValidationProblem(result.ToDictionary());

            var entity = request.ToDomainEntity();
            
            entity.PasswordSalt = PasswordHasher.GenerateSalt();
            entity.PasswordHash = PasswordHasher.ComputeHash(request.Password, entity.PasswordSalt, appSettings.Value.Pepper);

            await unitOfWork.Users.CreateUserAsync(entity, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var response = await GenerateJwtToken(entity.Id);

            await publisher.Publish(new LoginEvent(entity.Id), cancellationToken);
            return Results.Ok(response);
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
