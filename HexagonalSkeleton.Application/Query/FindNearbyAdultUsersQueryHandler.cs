using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Application.Common.Pagination;
using HexagonalSkeleton.Domain.Specifications.Users;
using HexagonalSkeleton.Domain.ValueObjects;
using MediatR;
using AutoMapper;

namespace HexagonalSkeleton.Application.Query
{
    /// <summary>
    /// Handler for finding nearby adult users with complete profiles
    /// Demonstrates advanced use of Specification pattern for complex business queries
    /// Shows how specifications can be composed for sophisticated filtering
    /// </summary>
    public class FindNearbyAdultUsersQueryHandler(
        IUserReadRepository userReadRepository,
        IMapper mapper)
        : IRequestHandler<FindNearbyAdultUsersQuery, PagedQueryResult<UserDto>>
    {
        public async Task<PagedQueryResult<UserDto>> Handle(FindNearbyAdultUsersQuery request, CancellationToken cancellationToken)
        {
            // Build complex specification using composition
            var specification = UserSpecificationBuilder.Create()
                .OnlyActive()                    // Business rule: only active users
                .OnlyAdults()                    // Business rule: only adults
                .WithCompleteProfile()           // Business rule: complete profiles only
                .WithinLocation(                 // Geographic filtering
                    request.Latitude, 
                    request.Longitude, 
                    request.RadiusInKm)
                .Build();

            // Create pagination parameters
            var pagination = new PaginationParams(
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                sortBy: "CreatedAt",
                sortDirection: "desc");

            // Execute query with specification
            var pagedDomainResult = await userReadRepository.GetUsersAsync(
                specification, 
                pagination, 
                cancellationToken);

            // Map domain entities to DTOs
            var userDtos = mapper.Map<List<UserDto>>(pagedDomainResult.Items);
            
            // Return result
            return PagedQueryResult<UserDto>.FromDomain(pagedDomainResult, userDtos);
        }
    }
}
