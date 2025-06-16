using FluentValidation;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Application.Event;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Application.Query;
using MediatR;

namespace HexagonalSkeleton.Application.Command
{
    /// <summary>
    /// Handles user login authentication and records login event
    /// Follows CQRS by handling the command side of login
    /// </summary>
    public class LoginCommandHandler(
        IValidator<LoginCommand> validator,
        IPublisher publisher,
        IUserReadRepository userReadRepository,
        IUserWriteRepository userWriteRepository,
        IAuthenticationService authenticationService)
        : IRequestHandler<LoginCommand, ResultDto>
    {
        public async Task<ResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
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

            // Record login through the aggregate
            user.RecordLogin();
            await userWriteRepository.UpdateAsync(user, cancellationToken);

            var accessToken = await authenticationService.GenerateJwtTokenAsync(user.Id, cancellationToken);
            var response = new LoginQueryResult(accessToken);

            await publisher.Publish(new LoginEvent(user.Id), cancellationToken);
            return new ResultDto(response);
        }
    }
}
