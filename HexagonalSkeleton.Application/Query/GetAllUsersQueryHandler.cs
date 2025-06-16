using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain.Ports;
using MediatR;

namespace HexagonalSkeleton.Application.Query
{
    public class GetAllUsersQueryHandler(
        IUserReadRepository userReadRepository)
        : IRequestHandler<GetAllUsersQuery, ResultDto>
    {
        public async Task<ResultDto> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await userReadRepository.GetAllAsync(cancellationToken: cancellationToken);
            return new ResultDto(users.Select(s => new GetAllUsersQueryResult(s)));
        }
    }
}
