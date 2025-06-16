namespace HexagonalSkeleton.Domain.ValueObjects
{
    /// <summary>
    /// Value object for phone numbers with validation
    /// </summary>
    public record PhoneNumber
    {
        public string Value { get; }

        public PhoneNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Phone number cannot be null or empty", nameof(value));

            var cleanValue = CleanPhoneNumber(value);
            
            if (!IsValidPhoneNumber(cleanValue))
                throw new ArgumentException("Invalid phone number format", nameof(value));

            Value = cleanValue;
        }

        private static string CleanPhoneNumber(string phoneNumber)
        {
            // Remove all non-digit characters except + at the beginning
            var cleaned = string.Empty;
            for (int i = 0; i < phoneNumber.Length; i++)
            {
                var c = phoneNumber[i];
                if (char.IsDigit(c) || (i == 0 && c == '+'))
                {
                    cleaned += c;
                }
            }
            return cleaned;
        }

        private static bool IsValidPhoneNumber(string phoneNumber)
        {
            // Basic validation: should have at least 7 digits and at most 15 (international format)
            var digitCount = phoneNumber.Count(char.IsDigit);
            return digitCount >= 7 && digitCount <= 15;
        }

        public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
        public static implicit operator PhoneNumber(string phoneNumber) => new(phoneNumber);

        public override string ToString() => Value;
    }
}
