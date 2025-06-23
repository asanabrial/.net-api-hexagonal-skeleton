using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain.ValueObjects;
using HexagonalSkeleton.Domain.Specifications.Users;
using HexagonalSkeleton.Application.Common.Pagination;
using MediatR;
using AutoMapper;
using FluentValidation;
using HexagonalSkeleton.Application.Extensions;

namespace HexagonalSkeleton.Application.Query
{
    /// <summary>
    /// Handler for GetAllUsersQuery with pagination support
    /// Simplified to use generic PagedQueryResult directly
    /// Follows CQRS pattern and uses domain specifications for filtering
    /// </summary>
    public class GetAllUsersQueryHandler(
        IValidator<GetAllUsersQuery> validator,
        IUserReadRepository userReadRepository,
        IMapper mapper)
        : IRequestHandler<GetAllUsersQuery, PagedQueryResult<UserDto>>
    {
        public async Task<PagedQueryResult<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            // Validate input using FluentValidation
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                throw new Exceptions.ValidationException(validationResult.ToDictionary());

            // Create pagination parameters using domain value object
            var pagination = request.ToPaginationParams();

            // Create search specification if search term is provided
            UserSearchSpecification? searchSpecification = null;
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                searchSpecification = new UserSearchSpecification(request.SearchTerm);
            }            // Get paginated users using domain value objects and specifications
            var pagedDomainResult = await userReadRepository.GetPagedAsync(
                pagination, 
                searchSpecification, 
                cancellationToken);            // Map domain entities to DTOs
            var userDtos = mapper.Map<List<UserDto>>(pagedDomainResult.Items);
            
            // Return generic paginated result directly - SIMPLIFIED!
            return PagedQueryResult<UserDto>.FromDomain(pagedDomainResult, userDtos);
        }
    }
}
