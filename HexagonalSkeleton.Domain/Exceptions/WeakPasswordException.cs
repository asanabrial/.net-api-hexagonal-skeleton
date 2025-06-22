namespace HexagonalSkeleton.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when password doesn't meet business requirements
    /// </summary>
    public class WeakPasswordException : DomainException
    {
        public WeakPasswordException() 
            : base("Password does not meet strength requirements")
        {
        }
    }
}
