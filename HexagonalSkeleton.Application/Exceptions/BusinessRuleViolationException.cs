namespace HexagonalSkeleton.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when a business rule is violated
    /// Maps to HTTP 422 Unprocessable Entity
    /// </summary>
    public class BusinessRuleViolationException : BusinessException
    {
        public BusinessRuleViolationException(string message) : base(message) { }
        public BusinessRuleViolationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
