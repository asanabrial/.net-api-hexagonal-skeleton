using FluentValidation;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain.Ports;
using MediatR;

namespace HexagonalSkeleton.Application.Command
{
    /// <summary>
    /// Command handler for hard deleting a user (permanent deletion)
    /// Follows DDD principles by first checking if the entity exists
    /// </summary>
    public class HardDeleteUserCommandHandler(
            IValidator<HardDeleteUserCommand> validator,
            IUserReadRepository userReadRepository,
            IUserWriteRepository userWriteRepository)
            : IRequestHandler<HardDeleteUserCommand, ResultDto>
    {
        public async Task<ResultDto> Handle(HardDeleteUserCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return new ResultDto(result.ToDictionary());            // Check if user exists before attempting deletion
            var user = await userReadRepository.GetByIdAsync(request.Id, cancellationToken);
            if (user == null)
                return new ResultDto(new Dictionary<string, string[]> 
                { 
                    { "User", new[] { "User not found" } } 
                });

            await userWriteRepository.DeleteAsync(request.Id, cancellationToken);

            return new ResultDto(true);
        }
    }
}
