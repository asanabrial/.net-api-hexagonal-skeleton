using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain.Ports;
using MediatR;

namespace HexagonalSkeleton.Application.Query
{
    public class GetAllUsersQueryHandler(
        IUserReadRepository userReadRepository)
        : IRequestHandler<GetAllUsersQuery, GetAllUsersQueryResult>
    {
        public async Task<GetAllUsersQueryResult> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await userReadRepository.GetAllAsync(cancellationToken: cancellationToken);
            return new GetAllUsersQueryResult(users);
        }
    }
}
