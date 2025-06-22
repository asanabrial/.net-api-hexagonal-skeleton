using HexagonalSkeleton.Application.Common.Pagination;
using HexagonalSkeleton.Domain.Specifications;
using HexagonalSkeleton.Domain.ValueObjects;
using HexagonalSkeleton.Domain.Ports;
using AutoMapper;
using MediatR;

namespace HexagonalSkeleton.Application.Common.Handlers
{
    /// <summary>
    /// Base handler for paginated queries
    /// Provides common functionality for pagination, mapping, and specification handling
    /// Follows CQRS pattern and DDD principles
    /// </summary>
    public abstract class PagedQueryHandler<TQuery, TResult, TDomain, TDto> : IRequestHandler<TQuery, TResult>
        where TQuery : PagedQuery, IRequest<TResult>
        where TResult : class
        where TDomain : class
        where TDto : class
    {
        protected readonly IMapper Mapper;

        protected PagedQueryHandler(IMapper mapper)
        {
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public abstract Task<TResult> Handle(TQuery request, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a specification from the search term
        /// Must be implemented by derived classes for entity-specific search logic
        /// </summary>
        protected abstract Specification<TDomain>? CreateSearchSpecification(string searchTerm);

        /// <summary>
        /// Gets the repository for the specific domain entity
        /// Must be implemented by derived classes
        /// </summary>
        protected abstract Task<PagedResult<TDomain>> GetPagedDataAsync(
            PaginationParams pagination, 
            Specification<TDomain>? specification, 
            CancellationToken cancellationToken);

        /// <summary>
        /// Creates the result object from paginated DTOs
        /// Must be implemented by derived classes
        /// </summary>
        protected abstract TResult CreateResult(PagedQueryResult<TDto> pagedDtos);

        /// <summary>
        /// Common pagination logic that can be reused across all paginated handlers
        /// </summary>
        protected async Task<PagedQueryResult<TDto>> ExecutePaginatedQueryAsync(
            TQuery query,
            CancellationToken cancellationToken)
        {
            // Create pagination parameters using domain value object
            var pagination = query.ToPaginationParams();

            // Create search specification if search term is provided
            Specification<TDomain>? searchSpecification = null;
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                searchSpecification = CreateSearchSpecification(query.SearchTerm);
            }

            // Get paginated domain entities
            var pagedDomainResult = await GetPagedDataAsync(pagination, searchSpecification, cancellationToken);
            
            // Map domain entities to DTOs
            var dtos = Mapper.Map<List<TDto>>(pagedDomainResult.Items);
            
            // Create application-layer paginated result
            return PagedQueryResult<TDto>.FromDomain(pagedDomainResult, dtos);
        }

        /// <summary>
        /// Validates query parameters before processing
        /// Can be overridden for additional validation logic
        /// </summary>
        protected virtual void ValidateQuery(TQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (!query.IsValid())
                throw new ArgumentException("Invalid pagination parameters", nameof(query));
        }
    }
}
