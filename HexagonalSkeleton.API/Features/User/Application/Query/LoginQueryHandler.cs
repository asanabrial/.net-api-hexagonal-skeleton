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

            var response = await GenerateJwtToken(request.Email, request.Password, cancellationToken);

            await publisher.Publish(new LoginEvent(request.ToDomainEntity()), cancellationToken);
            return Results.Ok(response);
        }

        public async Task<LoginQueryResult> GenerateJwtToken(string email, string password, CancellationToken cancellationToken)
        {
            var result = unitOfWork.Users.GetByEmail(email, cancellationToken).Result;
            string? accessToken = null;
            if (result == null) return await Task.FromResult(new LoginQueryResult(accessToken));

            if (result.PasswordHash == PasswordHasher.ComputeHash(password, result.PasswordSalt, appSettings.Value.Pepper))
            {
                accessToken = JwToken.GenerateJwtToken(
                    result.Id.ToString(),
                    appSettings.Value.Jwt.Secret,
                    appSettings.Value.Jwt.Issuer,
                    appSettings.Value.Jwt.Audience);
            }

            return await Task.FromResult(new LoginQueryResult(accessToken));
        }
    }
}
