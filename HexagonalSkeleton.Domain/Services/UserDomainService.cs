using HexagonalSkeleton.Domain.ValueObjects;
using HexagonalSkeleton.Domain.Ports;

namespace HexagonalSkeleton.Domain.Services
{
    /// <summary>
    /// Domain service for complex user business logic that doesn't belong to a single aggregate
    /// </summary>
    public class UserDomainService
    {
        /// <summary>
        /// Create a new user with all business validations
        /// Uses the aggregate's factory method
        /// </summary>
        public static User CreateUser(
            string email,
            string passwordSalt,
            string passwordHash,
            string firstName,
            string lastName,
            DateTime birthdate,
            string phoneNumber,
            double latitude,
            double longitude,
            string aboutMe = "")
        {
            // Additional business validations can go here
            ValidateUserCreation(email, firstName, lastName);

            return User.Create(
                email, passwordSalt, passwordHash, firstName, lastName, 
                birthdate, phoneNumber, latitude, longitude, aboutMe);
        }

        /// <summary>
        /// Check if a user can be updated by another user (authorization logic)
        /// </summary>
        public static bool CanUserUpdateProfile(User requestingUser, User targetUser)
        {
            // Business rule: Users can only update their own profile
            // Admins might have special permissions (future implementation)
            return requestingUser.Id == targetUser.Id && !requestingUser.IsDeleted;
        }

        /// <summary>
        /// Determine if users can interact based on business rules
        /// </summary>
        public static bool CanUsersInteract(User user1, User user2)
        {
            // Business rules for user interaction
            if (user1.IsDeleted || user2.IsDeleted)
                return false;

            // Both users must be adults for certain interactions
            if (!user1.IsAdult() || !user2.IsAdult())
                return false;

            return true;
        }

        /// <summary>
        /// Find users within a certain radius (geographical business logic)
        /// </summary>
        public static IEnumerable<User> FindNearbyUsers(User centerUser, IEnumerable<User> allUsers, double radiusInKm)
        {
            if (centerUser.IsDeleted)
                return Enumerable.Empty<User>();

            return allUsers
                .Where(u => !u.IsDeleted && u.Id != centerUser.Id)
                .Where(u => centerUser.IsNearby(u, radiusInKm));
        }

        /// <summary>
        /// Validate if email format is acceptable for business requirements
        /// </summary>
        public static bool IsValidBusinessEmail(string email)
        {
            try
            {
                var emailVO = new Email(email);
                
                // Additional business rules for email
                // Example: company might not allow certain domains
                var blockedDomains = new[] { "tempmail.com", "10minutemail.com" };
                var domain = emailVO.Value.Split('@')[1];
                
                return !blockedDomains.Contains(domain, StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Calculate user compatibility score based on business rules
        /// </summary>
        public static double CalculateCompatibilityScore(User user1, User user2)
        {
            if (!CanUsersInteract(user1, user2))
                return 0;

            double score = 0;

            // Age proximity (within 5 years adds points)
            var ageDifference = Math.Abs(user1.GetAge() - user2.GetAge());
            if (ageDifference <= 5)
                score += 20;

            // Geographic proximity (within 10km adds points)
            var distance = user1.CalculateDistanceTo(user2);
            if (distance <= 10)
                score += 30;

            // Profile completeness
            if (!string.IsNullOrEmpty(user1.AboutMe) && !string.IsNullOrEmpty(user2.AboutMe))
                score += 25;

            return Math.Min(score, 100); // Cap at 100
        }

        /// <summary>
        /// Determine if a user profile is complete according to business rules
        /// </summary>
        public static bool IsProfileComplete(User user)
        {
            return !string.IsNullOrEmpty(user.AboutMe) &&
                   user.Birthdate.HasValue &&
                   !string.IsNullOrEmpty(user.FullName.FirstName) &&
                   !string.IsNullOrEmpty(user.FullName.LastName);
        }

        /// <summary>
        /// Check if user registration data is unique across the system
        /// </summary>
        public static async Task<bool> IsUserRegistrationDataUniqueAsync(
            string email, 
            string phoneNumber, 
            IUserReadRepository readRepository, 
            CancellationToken cancellationToken = default)
        {
            var emailExists = await readRepository.ExistsByEmailAsync(email, cancellationToken);
            var phoneExists = await readRepository.ExistsByPhoneNumberAsync(phoneNumber, cancellationToken);
            
            return !emailExists && !phoneExists;
        }

        /// <summary>
        /// Validate password strength according to business rules
        /// </summary>
        public static bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        /// <summary>
        /// Calculate distance between two users for proximity features
        /// </summary>
        public static double CalculateDistanceBetweenUsers(User user1, User user2)
        {
            if (user1 == null || user2 == null)
                throw new ArgumentNullException("Users cannot be null for distance calculation");

            return user1.CalculateDistanceTo(user2);
        }

        /// <summary>
        /// Determine if users are in the same geographical area
        /// </summary>
        public static bool AreUsersNearby(User user1, User user2, double radiusInKm = 50)
        {
            return CalculateDistanceBetweenUsers(user1, user2) <= radiusInKm;
        }

        // Private validation methods
        private static void ValidateUserCreation(string email, string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name is required for user creation", nameof(firstName));
            
            if (string.IsNullOrWhiteSpace(lastName))                throw new ArgumentException("Last name is required for user creation", nameof(lastName));            if (!IsValidBusinessEmail(email))
                throw new ArgumentException("Email does not meet business requirements", nameof(email));
        }
    }
}
