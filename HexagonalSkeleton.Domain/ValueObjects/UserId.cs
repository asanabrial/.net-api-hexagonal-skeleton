namespace HexagonalSkeleton.Domain.ValueObjects
{
    /// <summary>
    /// Value object for User ID
    /// Provides type safety and prevents primitive obsession
    /// </summary>
    public record UserId
    {
        public Guid Value { get; }

        public UserId(Guid value)
        {
            if (value == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(value));
            
            Value = value;
        }

        public static implicit operator Guid(UserId userId) => userId.Value;
        public static implicit operator UserId(Guid value) => new(value);

        public override string ToString() => Value.ToString();
    }
}
