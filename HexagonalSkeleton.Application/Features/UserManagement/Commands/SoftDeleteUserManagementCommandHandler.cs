using FluentValidation;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Application.Features.UserManagement.Dto;
using HexagonalSkeleton.Domain.Ports;
using MediatR;

namespace HexagonalSkeleton.Application.Features.UserManagement.Commands
{
    /// <summary>
    /// Command handler for soft deleting a user (logical deletion)
    /// Uses domain events for automatic integration event publishing via CommandDbContext
    /// </summary>
    public class SoftDeleteUserManagementCommandHandler(
        IValidator<SoftDeleteUserManagementCommand> validator,
        IUserWriteRepository userWriteRepository)
        : IRequestHandler<SoftDeleteUserManagementCommand, UserDeletionDto>
    {
        public async Task<UserDeletionDto> Handle(SoftDeleteUserManagementCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                throw new Exceptions.ValidationException(result.ToDictionary());

            // Get the user aggregate (including deleted ones for domain validation)
            var user = await userWriteRepository.GetByIdUnfilteredAsync(request.Id, cancellationToken);
            if (user == null)
                throw new NotFoundException("User", request.Id);

            // Use the aggregate's business method for soft deletion
            // This will add a domain event to the aggregate
            user.Delete();

            // Persist the changes through the write repository
            // Domain events will be automatically published by CommandDbContext
            await userWriteRepository.UpdateAsync(user, cancellationToken);

            // Return deletion result
            return new UserDeletionDto 
            { 
                UserId = request.Id
            };
        }
    }
}
