using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Application.Common.Pagination;
using MediatR;
using AutoMapper;
using FluentValidation;

namespace HexagonalSkeleton.Application.Query
{    /// <summary>
    /// Handler for getting users with search capability
    /// Simple approach: search term OR show all active users
    /// </summary>
    public class GetAllUsersQueryHandler(
        IValidator<GetAllUsersQuery> validator,
        IUserReadRepository userReadRepository,
        IMapper mapper)
        : IRequestHandler<GetAllUsersQuery, PagedQueryResult<UserDto>>
    {        public async Task<PagedQueryResult<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            // Validate input using FluentValidation
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                throw new Exceptions.ValidationException(validationResult.ToDictionary());

            // Create pagination parameters using domain value object
            var pagination = request.ToPaginationParams();

            // Simple decision: search or show all active users
            var pagedDomainResult = !string.IsNullOrWhiteSpace(request.SearchTerm)
                ? await userReadRepository.SearchUsersAsync(pagination, request.SearchTerm, cancellationToken)
                : await userReadRepository.GetUsersAsync(pagination, cancellationToken);

            // Map domain entities to DTOs
            var userDtos = mapper.Map<List<UserDto>>(pagedDomainResult.Items);
            
            // Return result
            return PagedQueryResult<UserDto>.FromDomain(pagedDomainResult, userDtos);
        }
    }
}
