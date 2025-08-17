using HexagonalSkeleton.Application.Features.UserProfile.Dto;
using MediatR;

namespace HexagonalSkeleton.Application.Features.UserProfile.Queries
{
    /// <summary>
    /// Query to get current user's profile information
    /// This is for authenticated users accessing their own profile
    /// Should NOT return deleted users
    /// </summary>
    public class GetMyProfileQuery : IRequest<UserProfileDto>
    {
        public Guid UserId { get; set; }

        public GetMyProfileQuery() { }

        public GetMyProfileQuery(Guid userId)
        {
            UserId = userId;
        }
    }
}
