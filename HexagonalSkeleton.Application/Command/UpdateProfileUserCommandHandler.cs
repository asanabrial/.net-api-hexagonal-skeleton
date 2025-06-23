using FluentValidation;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Application.Extensions;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain.Services;
using MediatR;
using AutoMapper;

namespace HexagonalSkeleton.Application.Command
{    public class UpdateProfileUserCommandHandler(
        IValidator<UpdateProfileUserCommand> validator,
        IUserReadRepository userReadRepository,
        IUserWriteRepository userWriteRepository,
        IMapper mapper)
        : IRequestHandler<UpdateProfileUserCommand, UserDto>
    {        public async Task<UserDto> Handle(UpdateProfileUserCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                throw new Exceptions.ValidationException(result.ToDictionary());

            var user = await userReadRepository.GetByIdAsync(id: request.Id, cancellationToken: cancellationToken);
            if (user is null) 
                throw new NotFoundException("User", request.Id);            user.UpdateProfile(request.FirstName, request.LastName, request.Birthdate, request.AboutMe);
            await userWriteRepository.UpdateAsync(user, cancellationToken);
            
            // Map user data to DTO using AutoMapper
            return mapper.Map<UserDto>(user);
        }
    }
}
