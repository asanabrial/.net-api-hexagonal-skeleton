using HexagonalSkeleton.Domain.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace HexagonalSkeleton.Domain.Services
{
    /// <summary>
    /// Factory for domain services
    /// Follows Dependency Inversion and Single Responsibility principles
    /// Centralizes domain service creation logic
    /// </summary>
    public interface IDomainServiceFactory
    {
        TService CreateService<TService>() where TService : class;
    }

    public class DomainServiceFactory : IDomainServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DomainServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TService CreateService<TService>() where TService : class
        {
            var service = _serviceProvider.GetService<TService>();
            
            if (service == null)
            {
                throw new InvalidOperationException($"Domain service {typeof(TService).Name} not registered");
            }

            return service;
        }
    }

    /// <summary>
    /// Enhanced User Domain Service with better separation of concerns
    /// Demonstrates SOLID principles in action
    /// </summary>
    public interface IUserDomainService
    {
        User CreateUser(string email, string passwordSalt, string passwordHash, string firstName, 
            string lastName, DateTime birthdate, string phoneNumber, double latitude, double longitude, string aboutMe);
        
        bool CanUserUpdateProfile(User requestingUser, User targetUser);
        bool CanUsersInteract(User user1, User user2);
        void ValidatePasswordStrength(string password);
        void ValidateUserUniqueness(bool emailExists, bool phoneExists, string email, string phoneNumber);
    }

    /// <summary>
    /// Implementation of enhanced User Domain Service
    /// Follows Single Responsibility and Dependency Inversion principles
    /// </summary>
    public class EnhancedUserDomainService : IUserDomainService
    {
        private readonly IUserExistenceChecker _existenceChecker;

        public EnhancedUserDomainService(IUserExistenceChecker existenceChecker)
        {
            _existenceChecker = existenceChecker;
        }

        public User CreateUser(string email, string passwordSalt, string passwordHash, string firstName, 
            string lastName, DateTime birthdate, string phoneNumber, double latitude, double longitude, string aboutMe)
        {
            // Delegate to static methods for core domain logic
            return UserDomainService.CreateUser(email, passwordSalt, passwordHash, firstName, lastName, 
                birthdate, phoneNumber, latitude, longitude, aboutMe);
        }

        public bool CanUserUpdateProfile(User requestingUser, User targetUser)
        {
            return UserDomainService.CanUserUpdateProfile(requestingUser, targetUser);
        }

        public bool CanUsersInteract(User user1, User user2)
        {
            return UserDomainService.CanUsersInteract(user1, user2);
        }

        public void ValidatePasswordStrength(string password)
        {
            UserDomainService.ValidatePasswordStrength(password);
        }

        public void ValidateUserUniqueness(bool emailExists, bool phoneExists, string email, string phoneNumber)
        {
            UserDomainService.ValidateUserUniqueness(emailExists, phoneExists, email, phoneNumber);
        }
    }
}
