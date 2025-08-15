using AutoMapper;
using FluentValidation;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Application.Features.UserProfile.Dto;
using HexagonalSkeleton.Domain.Ports;
using MediatR;

namespace HexagonalSkeleton.Application.Features.UserProfile.Commands
{    public class UpdateProfileUserCommandHandler(
        IValidator<UpdateProfileUserCommand> validator,
        IUserWriteRepository userWriteRepository,
        IMapper mapper)
        : IRequestHandler<UpdateProfileUserCommand, UserProfileDto>
    {        public async Task<UserProfileDto> Handle(UpdateProfileUserCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                throw new Exceptions.ValidationException(result.ToDictionary());

            // Get the user (including deleted ones for domain validation)
            var user = await userWriteRepository.GetByIdUnfilteredAsync(request.Id, cancellationToken);
            if (user is null) 
                throw new NotFoundException("User", request.Id);

            user.UpdateProfile(request.FirstName, request.LastName, request.Birthdate, request.AboutMe);
            
            // Update phone number separately if provided
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                user.UpdatePhoneNumber(request.PhoneNumber);
            }
            
            await userWriteRepository.UpdateAsync(user, cancellationToken);
            
            // Map user data to DTO using AutoMapper
            return mapper.Map<UserProfileDto>(user);
        }
    }
}
