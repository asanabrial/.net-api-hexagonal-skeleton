using HexagonalSkeleton.Domain.Common;
using HexagonalSkeleton.Domain.Events;
using HexagonalSkeleton.Domain.ValueObjects;
using HexagonalSkeleton.Domain.Exceptions;

namespace HexagonalSkeleton.Domain
{
    /// <summary>
    /// User aggregate root - Central entity in the user bounded context
    /// Implements DDD principles with value objects, domain events, and business rules
    /// Inherits from BaseEntity for common audit properties
    /// </summary>
    public class User : BaseEntity
    {
        // Private constructors force use of factory methods
        private User() { }

        private User(
            Email email,
            string passwordSalt,
            string passwordHash,
            FullName fullName,
            DateTime birthdate,
            PhoneNumber phoneNumber,
            Location location,
            string aboutMe)
        {
            Email = email ?? throw new ArgumentNullException(nameof(email));
            PasswordSalt = passwordSalt ?? throw new ArgumentNullException(nameof(passwordSalt));
            PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
            FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
            Birthdate = birthdate;
            PhoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
            Location = location ?? throw new ArgumentNullException(nameof(location));
            AboutMe = aboutMe ?? string.Empty;
            
            LastLogin = DateTime.UtcNow;
            
            // Raise domain event
            AddDomainEvent(new UserCreatedEvent(Id, Email.Value, FullName.FirstName, FullName.LastName, PhoneNumber.Value));
        }

        // Value Objects - Encapsulate business rules and validation
        public Email Email { get; private set; } = null!;
        public FullName FullName { get; private set; } = null!;
        public PhoneNumber PhoneNumber { get; private set; } = null!;
        public Location Location { get; private set; } = null!;

        // Primitive properties that don't warrant value objects yet
        public DateTime? Birthdate { get; private set; }
        public string AboutMe { get; private set; } = string.Empty;
        public string? ProfileImageName { get; private set; }
        
        // Authentication related - could be moved to separate aggregate
        public string PasswordSalt { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public DateTime LastLogin { get; private set; }

        // Factory method for creating new users
        public static User Create(
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
            // Business rule: User must be at least 13 years old
            var age = CalculateAge(birthdate);
            if (age < 13)
                throw UserDomainException.InvalidAge(age, 13);

            var emailVO = new Email(email);
            var fullNameVO = new FullName(firstName, lastName);
            var phoneNumberVO = new PhoneNumber(phoneNumber);
            var locationVO = new Location(latitude, longitude);

            return new User(emailVO, passwordSalt, passwordHash, fullNameVO, birthdate, phoneNumberVO, locationVO, aboutMe);
        }

        // Factory method for recreation from persistence (without domain events)
        public static User Reconstitute(
            Guid id,
            string email,
            string firstName,
            string lastName,
            DateTime birthdate,
            string phoneNumber,
            double latitude,
            double longitude,
            string aboutMe,
            string passwordSalt,
            string passwordHash,
            DateTime lastLogin,
            DateTime createdAt,
            DateTime? updatedAt,
            DateTime? deletedAt,
            bool isDeleted,
            string? profileImageName = null)
        {
            var user = new User();
            user.Id = id;
            user.Email = new Email(email);
            user.FullName = new FullName(firstName, lastName);
            user.PhoneNumber = new PhoneNumber(phoneNumber);
            user.Location = new Location(latitude, longitude);
            user.Birthdate = birthdate;
            user.AboutMe = aboutMe ?? string.Empty;
            user.PasswordSalt = passwordSalt;
            user.PasswordHash = passwordHash;
            user.LastLogin = lastLogin;
            user.CreatedAt = createdAt;
            user.UpdatedAt = updatedAt;
            user.DeletedAt = deletedAt;
            user.IsDeleted = isDeleted;
            user.ProfileImageName = profileImageName;

            return user;
        }

        // Business methods with domain logic
        public void UpdateProfile(string firstName, string lastName, DateTime birthdate, string aboutMe)
        {
            if (IsDeleted)
                throw UserDomainException.OperationOnDeletedUser("UpdateProfile");

            var previousName = FullName.FirstName;
            var newFullName = new FullName(firstName, lastName);
            
            // Business rule: User must be at least 13 years old
            var age = CalculateAge(birthdate);
            if (age < 13)
                throw UserDomainException.InvalidAge(age, 13);

            FullName = newFullName;
            Birthdate = birthdate;
            AboutMe = aboutMe ?? string.Empty;
            
            MarkAsUpdated();

            // Raise domain event if name changed
            if (previousName != firstName)
            {
                AddDomainEvent(new UserProfileUpdatedEvent(Id, Email.Value, previousName, firstName));
            }
        }

        public void UpdateLocation(double latitude, double longitude)
        {
            if (IsDeleted)
                throw new InvalidOperationException("Cannot update location of deleted user");

            Location = new Location(latitude, longitude);
            MarkAsUpdated();
        }

        public void UpdatePhoneNumber(string phoneNumber)
        {
            if (IsDeleted)
                throw new InvalidOperationException("Cannot update phone number of deleted user");

            PhoneNumber = new PhoneNumber(phoneNumber);
            MarkAsUpdated();
        }

        public void SetProfileImage(string fileName)
        {
            if (IsDeleted)
                throw new InvalidOperationException("Cannot set profile image of deleted user");

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));

            ProfileImageName = fileName;
            MarkAsUpdated();
        }

        public void RemoveProfileImage()
        {
            ProfileImageName = null;
            MarkAsUpdated();
        }

        public void RecordLogin()
        {
            if (IsDeleted)
                throw new InvalidOperationException("Deleted user cannot log in");

            LastLogin = DateTime.UtcNow;
            MarkAsUpdated();

            AddDomainEvent(new UserLoggedInEvent(Id, Email.Value, LastLogin));
        }

        public void ChangePassword(string newPasswordSalt, string newPasswordHash)
        {
            if (IsDeleted)
                throw new InvalidOperationException("Cannot change password of deleted user");

            if (string.IsNullOrWhiteSpace(newPasswordSalt))
                throw new ArgumentException("Password salt cannot be null or empty", nameof(newPasswordSalt));
            
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("Password hash cannot be null or empty", nameof(newPasswordHash));

            PasswordSalt = newPasswordSalt;
            PasswordHash = newPasswordHash;
            MarkAsUpdated();
        }

        // Domain methods for business queries
        public int GetAge()
        {
            return Birthdate.HasValue ? CalculateAge(Birthdate.Value) : 0;
        }

        public bool IsAdult()
        {
            return GetAge() >= 18;
        }

        public double CalculateDistanceTo(User otherUser)
        {
            return Location.CalculateDistanceTo(otherUser.Location);
        }

        public bool IsNearby(User otherUser, double radiusInKm)
        {
            return CalculateDistanceTo(otherUser) <= radiusInKm;
        }

        // Override delete to add business logic
        public override void Delete()
        {
            base.Delete();
            // Could add domain event here if needed
        }

        // Private helper methods
        private static int CalculateAge(DateTime birthdate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthdate.Year;
            
            if (birthdate.Date > today.AddYears(-age))
                age--;
                  return age;
        }
    }
}
