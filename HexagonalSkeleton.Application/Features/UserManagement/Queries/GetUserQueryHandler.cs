using FluentValidation;
using HexagonalSkeleton.Application.Features.UserManagement.Dto;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Domain.Ports;
using MediatR;
using AutoMapper;

namespace HexagonalSkeleton.Application.Features.UserManagement.Queries
{    public class GetUserQueryHandler(
        IValidator<GetUserQuery> validator,
        IUserReadRepository userReadRepository,
        IMapper mapper)        : IRequestHandler<GetUserQuery, GetUserDto>
    {        public async Task<GetUserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                throw new Exceptions.ValidationException(result.ToDictionary());

            var user = await userReadRepository.GetByIdAsync(
                id: request.Id,
                cancellationToken: cancellationToken);

            if (user == null)
                throw new NotFoundException("User", request.Id);

            return mapper.Map<GetUserDto>(user);
        }
    }
}
