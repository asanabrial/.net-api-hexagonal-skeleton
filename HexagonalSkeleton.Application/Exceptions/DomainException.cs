namespace HexagonalSkeleton.Application.Exceptions
{
    /// <summary>
    /// Base class for domain-specific business exceptions
    /// These typically map to HTTP 4xx client error codes
    /// </summary>
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message) { }
        protected DomainException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Base class for business rule violations
    /// Maps to HTTP 422 Unprocessable Entity
    /// </summary>
    public abstract class BusinessException : DomainException
    {
        protected BusinessException(string message) : base(message) { }
        protected BusinessException(string message, Exception innerException) : base(message, innerException) { }
    }
}
