namespace HexagonalSkeleton.Application.IntegrationEvents
{
    /// <summary>
    /// Base interface for integration events - Cross-bounded context communication
    /// Minimal interface following SOLID principles
    /// </summary>
    public interface IIntegrationEvent
    {
        Guid EventId { get; }
        DateTime OccurredOn { get; }
        string EventType { get; }
    }

    /// <summary>
    /// User Created Integration Event - Essential data only
    /// For CQRS read model synchronization
    /// </summary>
    public record UserCreatedIntegrationEvent : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
        public string EventType => nameof(UserCreatedIntegrationEvent);
        
        // Essential data only
        public Guid UserId { get; init; }
        public string Email { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }

        public UserCreatedIntegrationEvent() { }

        public UserCreatedIntegrationEvent(
            Guid userId,
            string email,
            string firstName,
            string lastName,
            string phoneNumber,
            DateTime createdAt)
        {
            UserId = userId;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            CreatedAt = createdAt;
        }
    }

    /// <summary>
    /// User Logged In Integration Event - For activity tracking
    /// </summary>
    public record UserLoggedInIntegrationEvent : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
        public string EventType => nameof(UserLoggedInIntegrationEvent);
        
        public Guid UserId { get; init; }
        public DateTime LoginTime { get; init; }

        public UserLoggedInIntegrationEvent() { }

        public UserLoggedInIntegrationEvent(Guid userId, DateTime loginTime)
        {
            UserId = userId;
            LoginTime = loginTime;
        }
    }
}
