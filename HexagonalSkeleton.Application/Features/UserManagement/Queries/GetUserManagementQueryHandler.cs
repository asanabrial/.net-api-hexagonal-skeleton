using FluentValidation;
using HexagonalSkeleton.Application.Features.UserManagement.Dto;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Application.Common.Messaging;
using HexagonalSkeleton.Application.Mapping;

namespace HexagonalSkeleton.Application.Features.UserManagement.Queries
{
    /// <summary>
    /// Handler for GetUserManagementQuery - handles user retrieval for management purposes
    /// Uses unfiltered repository method to include deleted users for admin visibility
    /// </summary>
    public class GetUserManagementQueryHandler : IRequestHandler<GetUserManagementQuery, GetUserDto>
    {
        private readonly IValidator<GetUserManagementQuery> _validator;
        private readonly IUserReadRepository _userReadRepository;

        public GetUserManagementQueryHandler(
            IValidator<GetUserManagementQuery> validator,
            IUserReadRepository userReadRepository)
        {
            _validator = validator;
            _userReadRepository = userReadRepository;
        }

        public async Task<GetUserDto> Handle(GetUserManagementQuery request, CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(request, cancellationToken);

            var user = await _userReadRepository.GetByIdUnfilteredAsync(request.Id, cancellationToken);

            if (user == null)
            {
                throw new NotFoundException($"User with identifier '{request.Id}' was not found");
            }

            return user.ToGetUserDto();
        }
    }
}
