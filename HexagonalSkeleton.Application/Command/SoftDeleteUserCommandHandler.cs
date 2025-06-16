using FluentValidation;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain.Ports;
using MediatR;

namespace HexagonalSkeleton.Application.Command
{
    public class SoftDeleteUserCommandHandler(
        IValidator<SoftDeleteUserCommand> validator,
        IUserWriteRepository userWriteRepository)
        : IRequestHandler<SoftDeleteUserCommand, ResultDto>
    {
        public async Task<ResultDto> Handle(SoftDeleteUserCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return new ResultDto(result.ToDictionary());

            await userWriteRepository.SoftDeleteAsync(request.Id, cancellationToken);

            return new ResultDto(true);
        }
    }
}
