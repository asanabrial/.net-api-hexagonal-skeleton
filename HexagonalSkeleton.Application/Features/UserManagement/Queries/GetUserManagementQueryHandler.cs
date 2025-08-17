using FluentValidation;
using AutoMapper;
using HexagonalSkeleton.Application.Features.UserManagement.Dto;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Domain.Ports;
using MediatR;

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
        private readonly IMapper _mapper;

        public GetUserManagementQueryHandler(
            IValidator<GetUserManagementQuery> validator,
            IUserReadRepository userReadRepository,
            IMapper mapper)
        {
            _validator = validator;
            _userReadRepository = userReadRepository;
            _mapper = mapper;
        }

        public async Task<GetUserDto> Handle(GetUserManagementQuery request, CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(request, cancellationToken);

            var user = await _userReadRepository.GetByIdUnfilteredAsync(request.Id, cancellationToken);

            if (user == null)
            {
                throw new NotFoundException($"User with identifier '{request.Id}' was not found");
            }

            return _mapper.Map<GetUserDto>(user);
        }
    }
}
