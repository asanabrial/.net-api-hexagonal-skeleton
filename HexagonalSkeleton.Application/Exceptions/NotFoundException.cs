namespace HexagonalSkeleton.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested resource is not found
    /// ASP.NET Core automatically maps KeyNotFoundException to HTTP 404 Not Found
    /// </summary>
    public class NotFoundException : KeyNotFoundException
    {
        public NotFoundException(string resource, object identifier)
            : base($"{resource} with identifier '{identifier}' was not found") { }

        public NotFoundException(string message) : base(message) { }
    }
}
