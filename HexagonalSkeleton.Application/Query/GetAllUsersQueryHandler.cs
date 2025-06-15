using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain;
using MediatR;

namespace HexagonalSkeleton.Application.Query
{
    public class GetAllUsersQueryHandler(
        IUserRepository unitOfWork)
        : IRequestHandler<GetAllUsersQuery, ResultDto>
    {
        public async Task<ResultDto> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await unitOfWork.GetAllUsersAsync(cancellationToken: cancellationToken);
            return new ResultDto(users.Select(s => new GetAllUsersQueryResult(s)));
        }
    }
}
