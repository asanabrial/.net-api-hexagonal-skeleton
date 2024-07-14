using FluentValidation;
using HexagonalSkeleton.API.Data;
using MediatR;

namespace HexagonalSkeleton.API.Features.User.Application.Query
{
    /// <summary>
    /// This class handles a GetUserCommand.
    /// </summary>
    public class GetUserQueryHandler(
        IValidator<GetUserQuery> validator,
        ILogger<GetUserQueryHandler> logger,
        IUnitOfWork unitOfWork)
        : IRequestHandler<GetUserQuery, IResult>
    {
        public async Task<IResult> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return Results.ValidationProblem(result.ToDictionary());

            var userEntity = await unitOfWork.Users.FindOneAsync(request.Id, cancellationToken);

            return userEntity is null ? Results.NotFound() : Results.Ok(new GetUserQueryResult(userEntity));
        }
    }
}
