using FluentValidation;
using HexagonalSkeleton.API.Config;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Application.Event;
using HexagonalSkeleton.CommonCore.Auth;
using HexagonalSkeleton.Domain;
using MediatR;
using Microsoft.Extensions.Options;

namespace HexagonalSkeleton.Application.Query
{
    /// <summary>
    /// This class handles a LoginCommand.
    /// </summary>
    public class LoginQueryHandler(
        IValidator<LoginQuery> validator,
        IPublisher publisher,
        IUserRepository unitOfWork,
        IOptions<AppSettings> appSettings)
        : IRequestHandler<LoginQuery, ResultDto>
    {
        public async Task<ResultDto> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return new ResultDto(result.ToDictionary());

            var entity = unitOfWork.GetByEmailAsync(request.Email, cancellationToken).Result;
            if (entity is null 
                || entity.PasswordHash != PasswordHasher.ComputeHash(request.Password, entity.PasswordSalt!, appSettings.Value.Pepper))
                throw new UnauthorizedAccessException();

            var response = await GenerateJwtToken(entity.Id.ToString(), cancellationToken);

            await publisher.Publish(new LoginEvent(entity.Id), cancellationToken);
            return new ResultDto(response);
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
