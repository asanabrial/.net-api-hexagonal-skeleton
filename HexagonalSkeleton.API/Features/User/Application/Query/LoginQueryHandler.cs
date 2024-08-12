using FluentValidation;
using HexagonalSkeleton.API.Config;
using HexagonalSkeleton.API.Data;
using HexagonalSkeleton.API.Features.User.Application.Event;
using HexagonalSkeleton.CommonCore.Auth;
using MediatR;
using Microsoft.Extensions.Options;

namespace HexagonalSkeleton.API.Features.User.Application.Query
{
    /// <summary>
    /// This class handles a LoginCommand.
    /// </summary>
    public class LoginQueryHandler(
        IValidator<LoginQuery> validator,
        IPublisher publisher,
        IUnitOfWork unitOfWork,
        IOptions<AppSettings> appSettings)
        : IRequestHandler<LoginQuery, IResult>
    {
        public async Task<IResult> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return Results.ValidationProblem(result.ToDictionary());

            var entity = unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken).Result;
            if (entity is null 
                || entity.PasswordHash != PasswordHasher.ComputeHash(request.Password, entity.PasswordSalt!, appSettings.Value.Pepper))
                return Results.Unauthorized();

            var response = await GenerateJwtToken(entity.Id.ToString(), cancellationToken);

            await publisher.Publish(new LoginEvent(entity.Id), cancellationToken);
            return Results.Ok(response);
        }

        public async Task<LoginQueryResult> GenerateJwtToken(string id, CancellationToken cancellationToken)
        {
            var accessToken = JwToken.GenerateJwtToken(
                id,
                appSettings.Value.Jwt.Secret,
                appSettings.Value.Jwt.Issuer,
                appSettings.Value.Jwt.Audience);

            return await Task.FromResult(new LoginQueryResult(accessToken));
        }
    }
}
