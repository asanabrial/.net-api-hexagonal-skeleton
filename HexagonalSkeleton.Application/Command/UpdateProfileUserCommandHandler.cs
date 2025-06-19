using FluentValidation;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain.Services;
using MediatR;

namespace HexagonalSkeleton.Application.Command
{
    public class UpdateProfileUserCommandHandler(
        IValidator<UpdateProfileUserCommand> validator,
        IUserReadRepository userReadRepository,
        IUserWriteRepository userWriteRepository)
        : IRequestHandler<UpdateProfileUserCommand, UpdateProfileUserCommandResult>
    {
        public async Task<UpdateProfileUserCommandResult> Handle(UpdateProfileUserCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return new UpdateProfileUserCommandResult(result.ToDictionary());

            var user = await userReadRepository.GetByIdAsync(id: request.Id, cancellationToken: cancellationToken);
            if (user is null) 
                return new UpdateProfileUserCommandResult(new Dictionary<string, string[]> 
                { 
                    { "User", new[] { "User not found" } } 
                });            user.UpdateProfile(request.FirstName, request.LastName, request.Birthdate, request.AboutMe);
            await userWriteRepository.UpdateAsync(user, cancellationToken);
            return new UpdateProfileUserCommandResult(user);
        }
    }
}
