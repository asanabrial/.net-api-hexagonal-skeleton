using FluentValidation;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Domain.Ports;
using MediatR;
using HexagonalSkeleton.Application.Features.UserManagement.Dto;

namespace HexagonalSkeleton.Application.Features.UserManagement.Commands
{
    /// <summary>
    /// Command handler for hard deleting a user (permanent deletion)
    /// Now uses exceptions instead of result validation pattern
    /// </summary>
    public class HardDeleteUserCommandHandler(
            IValidator<HardDeleteUserCommand> validator,
            IUserReadRepository userReadRepository,
            IUserWriteRepository userWriteRepository)
            : IRequestHandler<HardDeleteUserCommand, UserDeletionDto>
    {        public async Task<UserDeletionDto> Handle(HardDeleteUserCommand request, CancellationToken cancellationToken)
        {
            // Validate the request - throw if invalid
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                throw new Exceptions.ValidationException(validationResult.ToDictionary());

            // Check if user exists before attempting deletion - throw if not found
            var user = await userReadRepository.GetByIdAsync(request.Id, cancellationToken);
            if (user == null)
                throw new NotFoundException("User", request.Id);

            // Perform the deletion
            await userWriteRepository.DeleteAsync(request.Id, cancellationToken);            // Return deletion result
            return new UserDeletionDto 
            { 
                UserId = request.Id
            };
        }
    }
}
