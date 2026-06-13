namespace HexagonalSkeleton.Infrastructure.Mapping
{
    /// <summary>
    /// Normalizes DateTime values to UTC for persistence.
    /// Reproduces the DateTime conversion rules previously handled by AutoMapper
    /// in InfrastructureMappingProfile (explicit UTC handling for PostgreSQL).
    /// </summary>
    internal static class DateTimeNormalizer
    {
        /// <summary>
        /// Returns the value as UTC: Utc kind is returned as-is, Unspecified is
        /// flagged as Utc, anything else is converted to universal time.
        /// </summary>
        public static DateTime ToUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            _ => value.ToUniversalTime()
        };

        /// <summary>
        /// Nullable variant: null stays null, otherwise the same normalization applies.
        /// </summary>
        public static DateTime? ToUtc(DateTime? value) =>
            value.HasValue ? ToUtc(value.Value) : null;
    }
}
