namespace HexagonalSkeleton.Domain.ValueObjects
{
    /// <summary>
    /// Value Object representing user status in the system.
    /// Encapsulates business rules around user states and transitions.
    /// Immutable and self-validating following DDD principles.
    /// </summary>
    public sealed record UserStatus
    {
        public static readonly UserStatus Active = new("Active");
        public static readonly UserStatus Inactive = new("Inactive"); 
        public static readonly UserStatus Suspended = new("Suspended");
        public static readonly UserStatus Deleted = new("Deleted");

        public string Value { get; }

        private UserStatus(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Creates a UserStatus from string value with validation
        /// </summary>
        public static UserStatus FromString(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status cannot be null or empty", nameof(status));

            return status.ToLowerInvariant() switch
            {
                "active" => Active,
                "inactive" => Inactive,
                "suspended" => Suspended,
                "deleted" => Deleted,
                _ => throw new ArgumentException($"Invalid user status: {status}", nameof(status))
            };
        }

        /// <summary>
        /// Business rule: Can the user perform actions in this status?
        /// </summary>
        public bool CanPerformActions() => this == Active;

        /// <summary>
        /// Business rule: Can the user login in this status?
        /// </summary>
        public bool CanLogin() => this == Active || this == Inactive;

        /// <summary>
        /// Business rule: Is this a terminal status (cannot transition out)?
        /// </summary>
        public bool IsTerminal() => this == Deleted;

        /// <summary>
        /// Business rule: Valid status transitions
        /// </summary>
        public bool CanTransitionTo(UserStatus newStatus)
        {
            if (IsTerminal()) 
                return false;

            return (this.Value, newStatus.Value) switch
            {
                (var current, var target) when current == target => false, // No self-transitions
                (_, "Deleted") => true, // Can always delete
                ("Inactive", "Active") => true,
                ("Active", "Inactive") => true,
                ("Active", "Suspended") => true,
                ("Suspended", "Active") => true,
                _ => false
            };
        }

        public static implicit operator string(UserStatus status) => status.Value;
        public override string ToString() => Value;
    }
}
