using HexagonalSkeleton.Application.Common.Pagination;
using HexagonalSkeleton.Application.Features.UserManagement.Dto;
using MediatR;

namespace HexagonalSkeleton.Application.Features.UserManagement.Queries
{
    /// <summary>
    /// Query to get all users with advanced filtering support using Specification pattern
    /// Follows CQRS pattern and clean architecture principles for UserManagement context
    /// Supports multiple filter criteria that can be combined
    /// </summary>
    public class GetAllUsersManagementQuery : PagedQuery, IRequest<PagedQueryResult<GetAllUsersDto>>
    {
        /// <summary>
        /// Constructor for parameterless instantiation
        /// </summary>
        public GetAllUsersManagementQuery() : base() { }

        /// <summary>
        /// Constructor with pagination and filtering parameters
        /// </summary>
        public GetAllUsersManagementQuery(
            int? pageNumber = null,
            int? pageSize = null,
            string? searchTerm = null,
            string? sortBy = null,
            string? sortDirection = null,
            int? minAge = null,
            int? maxAge = null,
            bool? onlyAdults = null,
            bool? onlyActive = null,
            bool? onlyCompleteProfiles = null,
            double? latitude = null,
            double? longitude = null,
            double? radiusInKm = null)
            : base(pageNumber, pageSize, searchTerm, sortBy, sortDirection)
        {
            MinAge = minAge;
            MaxAge = maxAge;
            OnlyAdults = onlyAdults;
            OnlyActive = onlyActive;
            OnlyCompleteProfiles = onlyCompleteProfiles;
            Latitude = latitude;
            Longitude = longitude;
            RadiusInKm = radiusInKm;
        }

        // Business logic filter properties
        
        /// <summary>
        /// Minimum age filter
        /// </summary>
        public int? MinAge { get; set; }

        /// <summary>
        /// Maximum age filter
        /// </summary>
        public int? MaxAge { get; set; }

        /// <summary>
        /// Filter only adult users (18+)
        /// </summary>
        public bool? OnlyAdults { get; set; }

        /// <summary>
        /// Filter only active users (business rule: exclude deleted users)
        /// Default to true for safety
        /// </summary>
        public bool? OnlyActive { get; set; } = true;

        /// <summary>
        /// Filter users with complete profiles only
        /// </summary>
        public bool? OnlyCompleteProfiles { get; set; }

        /// <summary>
        /// Center latitude for location-based filtering
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Center longitude for location-based filtering
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// Radius in kilometers for location-based filtering
        /// </summary>
        public double? RadiusInKm { get; set; }
    }
}
