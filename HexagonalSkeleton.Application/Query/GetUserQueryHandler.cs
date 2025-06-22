using FluentValidation;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Application.Extensions;
using HexagonalSkeleton.Domain.Ports;
using MediatR;
using AutoMapper;

namespace HexagonalSkeleton.Application.Query
{    public class GetUserQueryHandler(
        IValidator<GetUserQuery> validator,
        IUserReadRepository userReadRepository,
        IMapper mapper)
        : IRequestHandler<GetUserQuery, GetUserQueryResult>
    {        public async Task<GetUserQueryResult> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                throw new Exceptions.ValidationException(result.ToDictionary());

            var user = await userReadRepository.GetByIdAsync(
                id: request.Id,
                cancellationToken: cancellationToken);            if (user == null)
                throw new NotFoundException("User", request.Id);

            return mapper.Map<GetUserQueryResult>(user);
        }
    }
}
