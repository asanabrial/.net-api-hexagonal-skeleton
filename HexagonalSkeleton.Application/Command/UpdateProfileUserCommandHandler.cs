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
        : IRequestHandler<UpdateProfileUserCommand, ResultDto>
    {
        public async Task<ResultDto> Handle(UpdateProfileUserCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return new ResultDto(result.ToDictionary());

            var user = await userReadRepository.GetByIdAsync(id: request.Id, cancellationToken: cancellationToken);
            if (user is null) 
                return new ResultDto("User not found");

            UserDomainService.UpdateUserProfile(user, request.Name, request.Surname, request.Birthdate, request.AboutMe);
            await userWriteRepository.UpdateProfileAsync(user, cancellationToken);
            return new ResultDto(true);
        }
    }
}
