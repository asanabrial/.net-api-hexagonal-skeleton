using HexagonalSkeleton.Application.Features.UserManagement.Dto;
using HexagonalSkeleton.Application.Common.Messaging;

namespace HexagonalSkeleton.Application.Features.UserManagement.Queries
{
    /// <summary>
    /// Query to get user information for management purposes
    /// This is for user management and SHOULD include deleted users with their deletion status
    /// </summary>
    public class GetUserManagementQuery : IRequest<GetUserDto>
    {
        public Guid Id { get; set; }

        public GetUserManagementQuery() { }

        public GetUserManagementQuery(Guid id)
        {
            Id = id;
        }
    }
}
