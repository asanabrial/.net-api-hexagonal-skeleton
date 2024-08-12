using FluentValidation;
using HexagonalSkeleton.API.Data;
using MediatR;

namespace HexagonalSkeleton.API.Features.User.Application.Query
{
    public class GetUserQueryHandler(
        IValidator<GetUserQuery> validator,
        IUnitOfWork unitOfWork)
        : IRequestHandler<GetUserQuery, IResult>
    {
        public async Task<IResult> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return Results.ValidationProblem(result.ToDictionary());
            
            var userEntity = await unitOfWork.Users.GetProfileUserByIdAsync(
                id: request.Id,
                cancellationToken: cancellationToken);

            return userEntity is null ? Results.NotFound() : Results.Ok(new GetUserQueryResult(userEntity));
        }
    }
}
