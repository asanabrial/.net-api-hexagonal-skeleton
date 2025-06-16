namespace HexagonalSkeleton.Domain.ValueObjects
{
    /// <summary>
    /// Value object for User ID
    /// Provides type safety and prevents primitive obsession
    /// </summary>
    public record UserId
    {
        public int Value { get; }

        public UserId(int value)
        {
            if (value <= 0)
                throw new ArgumentException("User ID must be greater than zero", nameof(value));
            
            Value = value;
        }

        public static implicit operator int(UserId userId) => userId.Value;
        public static implicit operator UserId(int value) => new(value);

        public override string ToString() => Value.ToString();
    }
}
