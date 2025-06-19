using FluentValidation;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Domain.Ports;
using MediatR;

namespace HexagonalSkeleton.Application.Query
{
    public class GetUserQueryHandler(
        IValidator<GetUserQuery> validator,
        IUserReadRepository userReadRepository)
        : IRequestHandler<GetUserQuery, GetUserQueryResult>
    {        public async Task<GetUserQueryResult> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                throw new Exceptions.ValidationException(result.ToDictionary());

            var user = await userReadRepository.GetByIdAsync(
                id: request.Id,
                cancellationToken: cancellationToken);

            if (user == null)
                throw new NotFoundException("User", request.Id);

            return new GetUserQueryResult(user);
        }
    }
}
