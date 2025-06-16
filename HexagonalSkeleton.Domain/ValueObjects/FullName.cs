namespace HexagonalSkeleton.Domain.ValueObjects
{
    /// <summary>
    /// Value object for user's full name
    /// </summary>
    public record FullName
    {
        public string FirstName { get; }
        public string LastName { get; }

        public FullName(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name cannot be null or empty", nameof(firstName));
            
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name cannot be null or empty", nameof(lastName));

            if (firstName.Length > 100)
                throw new ArgumentException("First name cannot exceed 100 characters", nameof(firstName));
            
            if (lastName.Length > 100)
                throw new ArgumentException("Last name cannot exceed 100 characters", nameof(lastName));

            FirstName = firstName.Trim();
            LastName = lastName.Trim();
        }

        public string GetFullName() => $"{FirstName} {LastName}";
        
        public string GetInitials() => $"{FirstName[0]}{LastName[0]}".ToUpperInvariant();

        public override string ToString() => GetFullName();
    }
}
