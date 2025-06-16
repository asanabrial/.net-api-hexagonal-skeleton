using FluentValidation;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain.Ports;
using MediatR;

namespace HexagonalSkeleton.Application.Command
{
    public class UpdateUserCommandHandler(
        IValidator<UpdateUserCommand> validator,
        IUserReadRepository userReadRepository,
        IUserWriteRepository userWriteRepository)
        : IRequestHandler<UpdateUserCommand, ResultDto>
    {
        public async Task<ResultDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return new ResultDto(result.ToDictionary());

            // Get the existing user
            var user = await userReadRepository.GetByIdAsync(request.Id, cancellationToken);
            if (user == null)
                return new ResultDto(new { Error = "User not found" });            // Update user properties using domain methods
            user.UpdateProfile(
                request.FirstName,
                request.LastName,
                request.Birthdate,
                request.AboutMe);

            user.UpdatePhoneNumber(request.PhoneNumber);
            user.UpdateLocation(request.Latitude, request.Longitude);

            await userWriteRepository.UpdateAsync(user, cancellationToken);
            return new ResultDto(true);
        }
    }
}
