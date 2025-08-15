using HexagonalSkeleton.Application.Features.UserManagement.Dto;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Application.Common.Pagination;
using HexagonalSkeleton.Application.Services;
using MediatR;
using AutoMapper;
using FluentValidation;

namespace HexagonalSkeleton.Application.Features.UserManagement.Queries
{
    /// <summary>
    /// Handler for getting users with advanced filtering using Specification pattern
    /// Follows CQRS, Clean Architecture, and SOLID principles for UserManagement context
    /// Single responsibility: orchestrate the query execution flow
    /// </summary>
    public class GetAllUsersManagementQueryHandler(
        IValidator<GetAllUsersManagementQuery> validator,
        IUserReadRepository userReadRepository,
        IUserSpecificationService specificationService,
        IMapper mapper)
        : IRequestHandler<GetAllUsersManagementQuery, PagedQueryResult<GetAllUsersDto>>
    {
        /// <summary>
        /// Handles the query execution with clean separation of concerns
        /// Each step has a single responsibility and is easily testable
        /// </summary>
        public async Task<PagedQueryResult<GetAllUsersDto>> Handle(GetAllUsersManagementQuery request, CancellationToken cancellationToken)
        {
            // Step 1: Validate input (Input validation responsibility)
            await ValidateRequestAsync(request, cancellationToken);

            // Step 2: Build domain specification (Business logic responsibility)
            var specification = specificationService.BuildSpecification(request);

            // Step 3: Execute domain query (Data access responsibility)
            var pagedDomainResult = await ExecuteQueryAsync(request, specification, cancellationToken);

            // Step 4: Map to DTOs and return (Output transformation responsibility)
            return MapToResult(pagedDomainResult);
        }

        /// <summary>
        /// Validates the incoming request using FluentValidation
        /// Throws ValidationException if validation fails
        /// </summary>
        private async Task ValidateRequestAsync(GetAllUsersManagementQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new Exceptions.ValidationException(validationResult.ToDictionary());
            }
        }

        /// <summary>
        /// Executes the domain query with specification and pagination
        /// Delegates to the repository (following Dependency Inversion Principle)
        /// </summary>
        private async Task<Domain.ValueObjects.PagedResult<Domain.User>> ExecuteQueryAsync(
            GetAllUsersManagementQuery request, 
            Domain.Specifications.ISpecification<Domain.User> specification, 
            CancellationToken cancellationToken)
        {
            var pagination = request.ToPaginationParams();
            return await userReadRepository.GetUsersAsync(specification, pagination, cancellationToken);
        }

        /// <summary>
        /// Maps domain entities to DTOs for the application layer
        /// Uses AutoMapper for consistent mapping
        /// </summary>
        private PagedQueryResult<GetAllUsersDto> MapToResult(Domain.ValueObjects.PagedResult<Domain.User> pagedDomainResult)
        {
            var userDtos = mapper.Map<List<GetAllUsersDto>>(pagedDomainResult.Items);
            return PagedQueryResult<GetAllUsersDto>.FromDomain(pagedDomainResult, userDtos);
        }
    }
}
