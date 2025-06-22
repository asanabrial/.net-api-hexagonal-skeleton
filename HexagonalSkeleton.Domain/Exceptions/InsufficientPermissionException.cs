namespace HexagonalSkeleton.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a user tries to perform an action they don't have permission for
    /// This is different from AuthorizationException (application layer) as this is a domain rule
    /// </summary>
    public class InsufficientPermissionException : DomainException
    {
        public string UserId { get; }
        public string Action { get; }
        public string Resource { get; }

        public InsufficientPermissionException(string userId, string action, string resource) 
            : base($"User '{userId}' does not have permission to '{action}' on resource '{resource}'")
        {
            UserId = userId;
            Action = action;
            Resource = resource;
        }
    }
}
