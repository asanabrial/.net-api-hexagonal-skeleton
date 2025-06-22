using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain.Ports;
using MediatR;
using AutoMapper;

namespace HexagonalSkeleton.Application.Query
{    public class GetAllUsersQueryHandler(
        IUserReadRepository userReadRepository,
        IMapper mapper)
        : IRequestHandler<GetAllUsersQuery, GetAllUsersQueryResult>
    {        public async Task<GetAllUsersQueryResult> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await userReadRepository.GetAllAsync(cancellationToken: cancellationToken);
            
            var userDtos = mapper.Map<IList<UserDto>>(users);
            return new GetAllUsersQueryResult { Users = userDtos };
        }
    }
}
