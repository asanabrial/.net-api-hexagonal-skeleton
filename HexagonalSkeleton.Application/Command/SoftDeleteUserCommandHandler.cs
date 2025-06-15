using FluentValidation;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain;
using MediatR;

namespace HexagonalSkeleton.Application.Command
{
    public class SoftDeleteUserCommandHandler(
        IValidator<SoftDeleteUserCommand> validator,
        IUserRepository unitOfWork)
        : IRequestHandler<SoftDeleteUserCommand, ResultDto>
    {
        public async Task<ResultDto> Handle(SoftDeleteUserCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return new ResultDto(result.ToDictionary());

            await unitOfWork.SoftDeleteUser(request.Id);


            return new ResultDto(await unitOfWork.SaveChangesAsync(cancellationToken));
        }
    }
}
