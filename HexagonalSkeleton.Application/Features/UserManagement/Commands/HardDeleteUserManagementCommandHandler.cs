using FluentValidation;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Application.Common.Messaging;
using HexagonalSkeleton.Application.Features.UserManagement.Dto;

namespace HexagonalSkeleton.Application.Features.UserManagement.Commands
{
    /// <summary>
    /// Command handler for hard deleting a user (permanent deletion)
    /// Now uses exceptions instead of result validation pattern
    /// </summary>
    public class HardDeleteUserManagementCommandHandler(
            IValidator<HardDeleteUserManagementCommand> validator,
            IUserWriteRepository userWriteRepository)
            : IRequestHandler<HardDeleteUserManagementCommand, UserDeletionDto>
    {        public async Task<UserDeletionDto> Handle(HardDeleteUserManagementCommand request, CancellationToken cancellationToken)
        {
            // Validate the request - throw if invalid
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                throw new Exceptions.ValidationException(validationResult.ToDictionary());

            // Check if user exists (including already soft-deleted ones) before attempting deletion.
            // A hard delete is valid even on a soft-deleted user, so we only need an existence check here.
            var user = await userWriteRepository.GetByIdUnfilteredAsync(request.Id, cancellationToken);
            if (user == null)
                throw new NotFoundException("User", request.Id);

            // Perform a real physical deletion (hard delete) - distinct from the soft delete path.
            await userWriteRepository.RemoveAsync(request.Id, cancellationToken);            // Return deletion result
            return new UserDeletionDto 
            { 
                UserId = request.Id
            };
        }
    }
}
