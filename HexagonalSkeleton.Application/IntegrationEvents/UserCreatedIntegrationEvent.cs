using MassTransit;

namespace HexagonalSkeleton.Application.IntegrationEvents
{
    /// <summary>
    /// Base interface for integration events
    /// Used for cross-bounded context communication
    /// </summary>
    public interface IIntegrationEvent
    {
        Guid Id { get; }
        DateTime OccurredOn { get; }
        string EventType { get; }
    }

    /// <summary>
    /// Integration event for user creation using MassTransit
    /// Contains complete user data for CQRS synchronization
    /// Published to RabbitMQ for reliable cross-service communication
    /// </summary>
    public record UserCreatedIntegrationEvent : IIntegrationEvent
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
        public string EventType => nameof(UserCreatedIntegrationEvent);
        
        public Guid UserId { get; init; }
        public string Email { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public DateTime? Birthdate { get; init; }
        public string AboutMe { get; init; } = string.Empty;
        public string Country { get; init; } = string.Empty;
        public string State { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string FullAddress { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public double? Latitude { get; init; }
        public double? Longitude { get; init; }
        
        // Denormalized fields for search optimization
        public string[] SearchTerms { get; init; } = Array.Empty<string>();
        public int? Age { get; init; }

        public UserCreatedIntegrationEvent() { } // Required for MassTransit deserialization

        public UserCreatedIntegrationEvent(
            Guid userId,
            string email,
            string fullName,
            string firstName,
            string lastName,
            string phoneNumber,
            DateTime? birthdate,
            string aboutMe,
            string country,
            string state,
            string city,
            string fullAddress,
            DateTime createdAt,
            string[] searchTerms,
            int? age,
            double? latitude = null,
            double? longitude = null)
        {
            UserId = userId;
            Email = email;
            FullName = fullName;
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            Birthdate = birthdate;
            AboutMe = aboutMe;
            Country = country;
            State = state;
            City = city;
            FullAddress = fullAddress;
            CreatedAt = createdAt;
            SearchTerms = searchTerms;
            Age = age;
            Latitude = latitude;
            Longitude = longitude;
        }
    }

    /// <summary>
    /// Integration event for user profile updates
    /// </summary>
    public record UserProfileUpdatedIntegrationEvent : IIntegrationEvent
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
        public string EventType => nameof(UserProfileUpdatedIntegrationEvent);
        
        public Guid UserId { get; init; }
        public string Email { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public DateTime? Birthdate { get; init; }
        public string AboutMe { get; init; } = string.Empty;
        public string Country { get; init; } = string.Empty;
        public string State { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string FullAddress { get; init; } = string.Empty;
        public DateTime UpdatedAt { get; init; }
        public double? Latitude { get; init; }
        public double? Longitude { get; init; }
        public string[] SearchTerms { get; init; } = Array.Empty<string>();
        public int? Age { get; init; }

        public UserProfileUpdatedIntegrationEvent() { }

        public UserProfileUpdatedIntegrationEvent(
            Guid userId,
            string email,
            string fullName,
            string firstName,
            string lastName,
            string phoneNumber,
            DateTime? birthdate,
            string aboutMe,
            string country,
            string state,
            string city,
            string fullAddress,
            DateTime updatedAt,
            string[] searchTerms,
            int? age,
            double? latitude = null,
            double? longitude = null)
        {
            UserId = userId;
            Email = email;
            FullName = fullName;
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            Birthdate = birthdate;
            AboutMe = aboutMe;
            Country = country;
            State = state;
            City = city;
            FullAddress = fullAddress;
            UpdatedAt = updatedAt;
            SearchTerms = searchTerms;
            Age = age;
            Latitude = latitude;
            Longitude = longitude;
        }
    }

    /// <summary>
    /// Integration event for user login tracking
    /// </summary>
    public record UserLoggedInIntegrationEvent : IIntegrationEvent
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
        public string EventType => nameof(UserLoggedInIntegrationEvent);
        
        public Guid UserId { get; init; }
        public string Email { get; init; } = string.Empty;
        public DateTime LoginTime { get; init; }
        public string IpAddress { get; init; } = string.Empty;
        public string UserAgent { get; init; } = string.Empty;

        public UserLoggedInIntegrationEvent() { }

        public UserLoggedInIntegrationEvent(
            Guid userId,
            string email,
            DateTime loginTime,
            string ipAddress = "",
            string userAgent = "")
        {
            UserId = userId;
            Email = email;
            LoginTime = loginTime;
            IpAddress = ipAddress;
            UserAgent = userAgent;
        }
    }

    /// <summary>
    /// Integration event for user deletion/deactivation
    /// </summary>
    public record UserDeletedIntegrationEvent : IIntegrationEvent
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
        public string EventType => nameof(UserDeletedIntegrationEvent);
        
        public Guid UserId { get; init; }
        public string Email { get; init; } = string.Empty;
        public DateTime DeletedAt { get; init; }
        public string Reason { get; init; } = string.Empty;
        public bool IsHardDelete { get; init; } = false;

        public UserDeletedIntegrationEvent() { }

        public UserDeletedIntegrationEvent(
            Guid userId,
            string email,
            DateTime deletedAt,
            string reason,
            bool isHardDelete = false)
        {
            UserId = userId;
            Email = email;
            DeletedAt = deletedAt;
            Reason = reason;
            IsHardDelete = isHardDelete;
        }
    }
}
