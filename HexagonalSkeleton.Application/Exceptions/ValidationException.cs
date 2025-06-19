namespace HexagonalSkeleton.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when validation fails
    /// ASP.NET Core automatically maps ArgumentException to HTTP 400 Bad Request
    /// </summary>
    public class ValidationException : ArgumentException
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(IDictionary<string, string[]> errors) 
            : base("Validation failed")
        {
            Errors = errors ?? new Dictionary<string, string[]>();
        }

        public ValidationException(string field, string error) 
            : base($"Validation failed for {field}: {error}")
        {
            Errors = new Dictionary<string, string[]>
            {
                { field, new[] { error } }
            };
        }

        public ValidationException(string message) : base(message)
        {
            Errors = new Dictionary<string, string[]>
            {
                { "General", new[] { message } }
            };
        }
    }
}
