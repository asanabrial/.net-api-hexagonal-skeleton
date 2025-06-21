using FluentValidation;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Application.Extensions;
using HexagonalSkeleton.Domain.Ports;
using MediatR;

namespace HexagonalSkeleton.Application.Command
{
    /// <summary>
    /// Command handler for soft deleting a user (logical deletion)
    /// Follows DDD principles by working through the aggregate root
    /// </summary>
    public class SoftDeleteUserCommandHandler(
        IValidator<SoftDeleteUserCommand> validator,
        IUserReadRepository userReadRepository,
        IUserWriteRepository userWriteRepository)
        : IRequestHandler<SoftDeleteUserCommand, SoftDeleteUserCommandResult>
    {
        public async Task<SoftDeleteUserCommandResult> Handle(SoftDeleteUserCommand request, CancellationToken cancellationToken)
        {            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                throw new Exceptions.ValidationException(result.ToDictionary());

            // Get the user aggregate to apply business rules
            var user = await userReadRepository.GetByIdAsync(request.Id, cancellationToken);
            if (user == null)
                throw new NotFoundException("User", request.Id);

            // Use the aggregate's business method for soft deletion
            user.Delete();

            // Persist the changes through the write repository
            await userWriteRepository.UpdateAsync(user, cancellationToken);

            return new SoftDeleteUserCommandResult();
        }
    }
}
