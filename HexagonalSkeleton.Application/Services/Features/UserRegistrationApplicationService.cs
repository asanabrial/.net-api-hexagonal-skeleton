using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain.Services;
using HexagonalSkeleton.Application.Models;

namespace HexagonalSkeleton.Application.Services.Features
{
    /// <summary>
    /// User Registration Application Service
    /// Orchestrates user registration business operations
    /// Follows Screaming Architecture by making registration intent clear
    /// </summary>
    public interface IUserRegistrationApplicationService
    {
        Task<User> RegisterUserAsync(UserRegistrationData registrationData, CancellationToken cancellationToken = default);
        Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> IsPhoneAvailableAsync(string phoneNumber, CancellationToken cancellationToken = default);
    }

    public class UserRegistrationApplicationService : IUserRegistrationApplicationService
    {
        private readonly IUserWriteRepository _userWriteRepository;
        private readonly IUserExistenceChecker _existenceChecker;
        private readonly IUserDomainService _domainService;
        private readonly IAuthenticationService _authService;

        public UserRegistrationApplicationService(
            IUserWriteRepository userWriteRepository,
            IUserExistenceChecker existenceChecker,
            IUserDomainService domainService,
            IAuthenticationService authService)
        {
            _userWriteRepository = userWriteRepository;
            _existenceChecker = existenceChecker;
            _domainService = domainService;
            _authService = authService;
        }

        public async Task<User> RegisterUserAsync(UserRegistrationData registrationData, CancellationToken cancellationToken = default)
        {
            // 1. Validate business rules
            _domainService.ValidatePasswordStrength(registrationData.Password);
            
            // 2. Check uniqueness
            var emailExists = await _existenceChecker.ExistsByEmailAsync(registrationData.Email, cancellationToken);
            var phoneExists = await _existenceChecker.ExistsByPhoneNumberAsync(registrationData.PhoneNumber, cancellationToken);
            _domainService.ValidateUserUniqueness(emailExists, phoneExists, registrationData.Email, registrationData.PhoneNumber);

            // 3. Create user with domain service
            var passwordSalt = _authService.GenerateSalt();
            var passwordHash = _authService.HashPassword(registrationData.Password, passwordSalt);

            var user = _domainService.CreateUser(
                registrationData.Email,
                passwordSalt,
                passwordHash,
                registrationData.FirstName,
                registrationData.LastName,
                registrationData.Birthdate,
                registrationData.PhoneNumber,
                registrationData.Latitude,
                registrationData.Longitude,
                registrationData.AboutMe);

            // 4. Persist
            await _userWriteRepository.CreateAsync(user, cancellationToken);
            await _userWriteRepository.SaveChangesAsync(cancellationToken);

            return user;
        }

        public async Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default)
        {
            return !await _existenceChecker.ExistsByEmailAsync(email, cancellationToken);
        }

        public async Task<bool> IsPhoneAvailableAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            return !await _existenceChecker.ExistsByPhoneNumberAsync(phoneNumber, cancellationToken);
        }
    }

    /// <summary>
    /// Value object for user registration data
    /// Encapsulates all required registration information
    /// </summary>
    public record UserRegistrationData(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        DateTime Birthdate,
        string PhoneNumber,
        double Latitude,
        double Longitude,
        string AboutMe
    );
}
