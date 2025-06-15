using FluentValidation;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain;
using MediatR;
namespace HexagonalSkeleton.Application.Command
{
    public class HardDeleteUserCommandHandler(
            IValidator<HardDeleteUserCommand> validator,
            IUserRepository userRepository)
            : IRequestHandler<HardDeleteUserCommand, ResultDto>
    {
        public async Task<ResultDto> Handle(HardDeleteUserCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return new ResultDto(result.ToDictionary());

            await userRepository.HardDeleteUser(request.Id);

            return new ResultDto(await userRepository.SaveChangesAsync(cancellationToken));
        }
    }
}
