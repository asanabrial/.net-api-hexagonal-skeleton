using FluentValidation;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain;
using MediatR;

namespace HexagonalSkeleton.Application.Query
{
    public class GetUserQueryHandler(
        IValidator<GetUserQuery> validator,
        IUserRepository unitOfWork)
        : IRequestHandler<GetUserQuery, ResultDto>
    {
        public async Task<ResultDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return new ResultDto(result.ToDictionary());
            
            var userEntity = await unitOfWork.GetUserByIdAsync(
                id: request.Id,
                cancellationToken: cancellationToken);

            return new ResultDto(new GetUserQueryResult(userEntity));
        }
    }
}
