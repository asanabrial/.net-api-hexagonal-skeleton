using FluentValidation;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain.Ports;
using MediatR;
namespace HexagonalSkeleton.Application.Command
{    public class HardDeleteUserCommandHandler(
            IValidator<HardDeleteUserCommand> validator,
            IUserWriteRepository userWriteRepository)
            : IRequestHandler<HardDeleteUserCommand, ResultDto>
    {
        public async Task<ResultDto> Handle(HardDeleteUserCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return new ResultDto(result.ToDictionary());

            await userWriteRepository.DeleteAsync(request.Id, cancellationToken);

            return new ResultDto(true);
        }
    }
}
