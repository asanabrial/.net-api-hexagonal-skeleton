using FluentValidation;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Application.Event;
using HexagonalSkeleton.Domain.Ports;
using MediatR;

namespace HexagonalSkeleton.Application.Query
{
    /// <summary>
    /// This class handles a LoginQuery.
    /// </summary>
    public class LoginQueryHandler(
        IValidator<LoginQuery> validator,
        IPublisher publisher,
        IUserReadRepository userReadRepository,
        IUserWriteRepository userWriteRepository,
        IAuthenticationService authenticationService)
        : IRequestHandler<LoginQuery, ResultDto>
    {
        public async Task<ResultDto> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return new ResultDto(result.ToDictionary());

            var isValid = await authenticationService.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
            if (!isValid)
                return new ResultDto("Invalid email or password");

            var user = await userReadRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
                return new ResultDto("User not found");

            await userWriteRepository.SetLastLoginAsync(user.Id, cancellationToken);

            var accessToken = await authenticationService.GenerateJwtTokenAsync(user.Id, cancellationToken);
            var response = new LoginQueryResult(accessToken);

            await publisher.Publish(new LoginEvent(user.Id), cancellationToken);
            return new ResultDto(response);
        }
    }
}
