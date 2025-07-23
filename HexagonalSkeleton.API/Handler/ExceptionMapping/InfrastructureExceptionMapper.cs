using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HexagonalSkeleton.API.Handler.ExceptionMapping
{
    /// <summary>
    /// Maps infrastructure layer exceptions (Entity Framework, database, etc.) to appropriate HTTP responses
    /// </summary>
    public class InfrastructureExceptionMapper : IExceptionMapper
    {
        private readonly ILogger<InfrastructureExceptionMapper> _logger;

        public InfrastructureExceptionMapper(ILogger<InfrastructureExceptionMapper> logger)
        {
            _logger = logger;
        }

        public bool CanHandle(Exception exception)
        {
            return exception switch
            {
                DbUpdateException => true,
                PostgresException => true,
                _ => false
            };
        }

        public ProblemDetails MapToProblemDetails(Exception exception, string requestPath)
        {
            return exception switch
            {
                DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx) => MapUniqueConstraintViolation(dbEx, requestPath),
                DbUpdateException dbEx => MapGeneralDbUpdateException(dbEx, requestPath),
                PostgresException pgEx when pgEx.SqlState == "23505" => MapPostgresUniqueConstraint(pgEx, requestPath),
                PostgresException pgEx => MapPostgresException(pgEx, requestPath),
                _ => throw new ArgumentException($"Cannot handle exception of type {exception.GetType().Name}")
            };
        }

        private bool IsUniqueConstraintViolation(DbUpdateException exception)
        {
            // Check if inner exception is PostgreSQL unique constraint violation
            return exception.InnerException is PostgresException pgEx && pgEx.SqlState == "23505";
        }

        private ProblemDetails MapUniqueConstraintViolation(DbUpdateException exception, string requestPath)
        {
            var pgException = exception.InnerException as PostgresException;
            var detail = ExtractUserFriendlyMessage(pgException);
            
            _logger.LogWarning("Unique constraint violation: {Detail}", detail);

            return new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Data conflict",
                Detail = detail,
                Instance = requestPath,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10"
            };
        }

        private ProblemDetails MapGeneralDbUpdateException(DbUpdateException exception, string requestPath)
        {
            _logger.LogError(exception, "Database update error occurred");

            return new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Database error occurred",
                Detail = "An error occurred while saving changes to the database",
                Instance = requestPath
            };
        }

        private ProblemDetails MapPostgresUniqueConstraint(PostgresException exception, string requestPath)
        {
            var detail = ExtractUserFriendlyMessage(exception);
            
            _logger.LogWarning("PostgreSQL unique constraint violation: {Detail}", detail);

            return new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Data conflict",
                Detail = detail,
                Instance = requestPath,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10"
            };
        }

        private ProblemDetails MapPostgresException(PostgresException exception, string requestPath)
        {
            _logger.LogError(exception, "PostgreSQL error occurred: {SqlState}", exception.SqlState);

            return new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Database error occurred",
                Detail = "A database error occurred while processing your request",
                Instance = requestPath
            };
        }

        private static string ExtractUserFriendlyMessage(PostgresException? pgException)
        {
            if (pgException == null)
                return "The resource already exists";

            // Extract constraint name to provide meaningful messages
            if (pgException.ConstraintName?.Contains("Email", StringComparison.OrdinalIgnoreCase) == true)
                return "A user with this email address already exists";
            
            if (pgException.ConstraintName?.Contains("Phone", StringComparison.OrdinalIgnoreCase) == true)
                return "A user with this phone number already exists";

            // Generic message for other unique constraints
            return "The resource already exists";
        }
    }
}
