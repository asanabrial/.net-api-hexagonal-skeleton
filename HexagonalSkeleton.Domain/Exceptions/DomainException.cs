namespace HexagonalSkeleton.Domain.Exceptions
{
    /// <summary>
    /// Base exception for all domain-related business rule violations
    /// </summary>
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message) { }
        protected DomainException(string message, Exception innerException) : base(message, innerException) { }
    }
}
