using HexagonalSkeleton.Application.Common.Pagination;
using HexagonalSkeleton.Application.Features.SocialNetwork.Dto;
using MediatR;

namespace HexagonalSkeleton.Application.Features.SocialNetwork.Queries
{
    /// <summary>
    /// Query to find nearby adult users with complete profiles
    /// Demonstrates complex filtering using Specification pattern
    /// Example of business-focused query that combines multiple criteria
    /// </summary>
    public class FindNearbyAdultUsersQuery : IRequest<PagedQueryResult<NearbyUserDto>>
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double RadiusInKm { get; set; } = 50; // Default 50km
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public FindNearbyAdultUsersQuery(double latitude, double longitude, double radiusInKm = 50)
        {
            Latitude = latitude;
            Longitude = longitude;
            RadiusInKm = radiusInKm;
        }
    }
}
