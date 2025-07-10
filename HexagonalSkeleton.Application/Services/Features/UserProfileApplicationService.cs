using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain.Services;
using HexagonalSkeleton.Application.Models;

namespace HexagonalSkeleton.Application.Services.Features
{
    /// <summary>
    /// User Profile Management Application Service
    /// Orchestrates user profile-related business operations
    /// Follows Screaming Architecture by making profile management intent clear
    /// </summary>
    public interface IUserProfileApplicationService
    {
        Task<User> UpdateProfileAsync(Guid userId, UserProfileUpdateData updateData, CancellationToken cancellationToken = default);
        Task<User?> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default);
        Task UpdateLocationAsync(Guid userId, double latitude, double longitude, CancellationToken cancellationToken = default);
    }

    public class UserProfileApplicationService : IUserProfileApplicationService
    {
        private readonly IUserWriteRepository _userWriteRepository;
        private readonly IUserBasicReader _userReader;
        private readonly IUserDomainService _domainService;

        public UserProfileApplicationService(
            IUserWriteRepository userWriteRepository,
            IUserBasicReader userReader,
            IUserDomainService domainService)
        {
            _userWriteRepository = userWriteRepository;
            _userReader = userReader;
            _domainService = domainService;
        }

        public async Task<User> UpdateProfileAsync(Guid userId, UserProfileUpdateData updateData, CancellationToken cancellationToken = default)
        {
            // 1. Get existing user
            var existingUser = await _userWriteRepository.GetTrackedByIdAsync(userId, cancellationToken);
            if (existingUser == null)
            {
                throw new HexagonalSkeleton.Application.Exceptions.NotFoundException($"User with ID {userId} not found");
            }

            // 2. Apply domain business rules
            if (!_domainService.CanUserUpdateProfile(existingUser, existingUser))
            {
                throw new HexagonalSkeleton.Domain.Exceptions.InsufficientPermissionException(
                    userId.ToString(), "update", "profile");
            }

            // 3. Update user properties - simplified implementation
            // For now, we maintain the existing user without complex updates
            // This can be extended later when specific update requirements are defined

            // 4. Persist
            await _userWriteRepository.UpdateAsync(existingUser, cancellationToken);
            await _userWriteRepository.SaveChangesAsync(cancellationToken);

            return existingUser;
        }

        public async Task<User?> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _userReader.GetByIdAsync(userId, cancellationToken);
        }

        public async Task UpdateLocationAsync(Guid userId, double latitude, double longitude, CancellationToken cancellationToken = default)
        {
            var existingUser = await _userWriteRepository.GetTrackedByIdAsync(userId, cancellationToken);
            if (existingUser == null)
            {
                throw new HexagonalSkeleton.Application.Exceptions.NotFoundException($"User with ID {userId} not found");
            }

            existingUser.UpdateLocation(latitude, longitude);

            await _userWriteRepository.UpdateAsync(existingUser, cancellationToken);
            await _userWriteRepository.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Value object for user profile update data
    /// Encapsulates profile update information
    /// </summary>
    public record UserProfileUpdateData(
        string FirstName,
        string LastName,
        string AboutMe,
        double Latitude,
        double Longitude
    );
}
