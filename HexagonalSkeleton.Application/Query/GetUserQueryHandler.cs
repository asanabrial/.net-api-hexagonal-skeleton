using FluentValidation;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain.Ports;
using MediatR;

namespace HexagonalSkeleton.Application.Query
{    public class GetUserQueryHandler(
        IValidator<GetUserQuery> validator,
        IUserReadRepository userReadRepository)
        : IRequestHandler<GetUserQuery, ResultDto>
    {
        public async Task<ResultDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return new ResultDto(result.ToDictionary());
              var user = await userReadRepository.GetByIdAsync(
                id: request.Id,
                cancellationToken: cancellationToken);

            if (user == null)
                return new ResultDto("User not found");

            return new ResultDto(new GetUserQueryResult(user));
        }
    }
}
