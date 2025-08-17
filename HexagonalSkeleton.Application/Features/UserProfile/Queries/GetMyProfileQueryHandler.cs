using FluentValidation;
using HexagonalSkeleton.Application.Features.UserProfile.Dto;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Domain.Ports;
using MediatR;
using AutoMapper;

namespace HexagonalSkeleton.Application.Features.UserProfile.Queries
{
    /// <summary>
    /// Handler for getting current user's profile
    /// Uses read repository which filters out deleted users
    /// This ensures deleted users cannot access their profile
    /// </summary>
    public class GetMyProfileQueryHandler(
        IValidator<GetMyProfileQuery> validator,
        IUserReadRepository userReadRepository,
        IMapper mapper)
        : IRequestHandler<GetMyProfileQuery, UserProfileDto>
    {
        public async Task<UserProfileDto> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                throw new Exceptions.ValidationException(result.ToDictionary());

            // Use read repository - this will NOT return deleted users
            var user = await userReadRepository.GetByIdAsync(
                id: request.UserId,
                cancellationToken: cancellationToken);

            if (user == null)
                throw new NotFoundException("User profile not found or user has been deleted", request.UserId);

            return mapper.Map<UserProfileDto>(user);
        }
    }
}
