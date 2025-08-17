using AutoMapper;
using FluentValidation;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Application.Features.UserManagement.Dto;
using HexagonalSkeleton.Domain.Ports;
using MediatR;

namespace HexagonalSkeleton.Application.Features.UserManagement.Commands
{    
    /// <summary>
    /// Handler for UpdateUserManagementCommand - handles user updates in management context
    /// </summary>
    public class UpdateUserManagementCommandHandler(
        IValidator<UpdateUserManagementCommand> validator,
        IUserWriteRepository userWriteRepository,
        IMapper mapper)
        : IRequestHandler<UpdateUserManagementCommand, UpdateUserDto>
    {        
        public async Task<UpdateUserDto> Handle(UpdateUserManagementCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                throw new Exceptions.ValidationException(result.ToDictionary());

            // Get the existing user (including deleted ones for domain validation)
            var user = await userWriteRepository.GetByIdUnfilteredAsync(request.Id, cancellationToken);
            if (user == null)
                throw new NotFoundException("User", request.Id);

            // Update user properties using domain methods
            user.UpdateProfile(
                request.FirstName,
                request.LastName,
                request.Birthdate,
                request.AboutMe);

            user.UpdatePhoneNumber(request.PhoneNumber);
            user.UpdateLocation(request.Latitude, request.Longitude);
            await userWriteRepository.UpdateAsync(user, cancellationToken);
            
            // Map user data to DTO using AutoMapper
            return mapper.Map<UpdateUserDto>(user);
        }
    }
}
