using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.Specifications;
using HexagonalSkeleton.Domain.Specifications.Users;
using HexagonalSkeleton.Application.Query;

namespace HexagonalSkeleton.Application.Services
{
    /// <summary>
    /// Implementation of user specification building service
    /// Encapsulates the logic for translating query parameters to domain specifications
    /// Follows Single Responsibility and Open/Closed principles
    /// </summary>
    public class UserSpecificationService : IUserSpecificationService
    {
        /// <summary>
        /// Builds a user specification from query parameters
        /// Uses the builder pattern for clean and readable specification construction
        /// </summary>
        public ISpecification<User> BuildSpecification(GetAllUsersQuery query)
        {
            var builder = UserSpecificationBuilder.Create();

            ApplyActiveFilter(builder, query);
            ApplySearchFilter(builder, query);
            ApplyAgeFilters(builder, query);
            ApplyStatusFilters(builder, query);
            ApplyLocationFilter(builder, query);

            return builder.Build();
        }

        /// <summary>
        /// Applies the active user filter (business rule: exclude deleted users by default)
        /// </summary>
        private static void ApplyActiveFilter(UserSpecificationBuilder builder, GetAllUsersQuery query)
        {
            if (query.OnlyActive == true)
            {
                builder.OnlyActive();
            }
        }

        /// <summary>
        /// Applies search term filter for text-based searches
        /// Covers name, lastname, email, and phone number
        /// </summary>
        private static void ApplySearchFilter(UserSpecificationBuilder builder, GetAllUsersQuery query)
        {
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                builder.WithSearchTerm(query.SearchTerm);
            }
        }

        /// <summary>
        /// Applies age-related filters (range and adult status)
        /// </summary>
        private static void ApplyAgeFilters(UserSpecificationBuilder builder, GetAllUsersQuery query)
        {
            // Apply age range filter if both min and max are provided
            if (query.MinAge.HasValue && query.MaxAge.HasValue)
            {
                builder.WithAgeRange(query.MinAge.Value, query.MaxAge.Value);
            }

            // Apply adult filter (18+)
            if (query.OnlyAdults == true)
            {
                builder.OnlyAdults();
            }
        }

        /// <summary>
        /// Applies status-based filters (complete profiles)
        /// </summary>
        private static void ApplyStatusFilters(UserSpecificationBuilder builder, GetAllUsersQuery query)
        {
            if (query.OnlyCompleteProfiles == true)
            {
                builder.WithCompleteProfile();
            }
        }

        /// <summary>
        /// Applies location-based filter using coordinates and radius
        /// All three parameters (latitude, longitude, radius) must be provided
        /// </summary>
        private static void ApplyLocationFilter(UserSpecificationBuilder builder, GetAllUsersQuery query)
        {
            if (HasLocationParameters(query))
            {
                builder.WithinLocation(
                    query.Latitude!.Value, 
                    query.Longitude!.Value, 
                    query.RadiusInKm!.Value);
            }
        }

        /// <summary>
        /// Checks if all required location parameters are provided
        /// </summary>
        private static bool HasLocationParameters(GetAllUsersQuery query)
        {
            return query.Latitude.HasValue && 
                   query.Longitude.HasValue && 
                   query.RadiusInKm.HasValue;
        }
    }
}
