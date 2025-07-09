using HexagonalSkeleton.Infrastructure.Persistence.Command;
using HexagonalSkeleton.Infrastructure.Persistence.Query;
using HexagonalSkeleton.Infrastructure.Persistence.Query.Documents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace HexagonalSkeleton.API.Controllers
{
    /// <summary>
    /// Health check controller for CQRS infrastructure
    /// Monitors the health of both command and query stores
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class HealthController : ControllerBase
    {
        private readonly CommandDbContext _commandDbContext;
        private readonly QueryDbContext _queryDbContext;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            CommandDbContext commandDbContext,
            QueryDbContext queryDbContext,
            ILogger<HealthController> logger)
        {
            _commandDbContext = commandDbContext ?? throw new ArgumentNullException(nameof(commandDbContext));
            _queryDbContext = queryDbContext ?? throw new ArgumentNullException(nameof(queryDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Check overall health of the CQRS system
        /// </summary>
        /// <returns>Health status of both command and query stores</returns>
        [HttpGet]
        [ProducesResponseType(typeof(HealthStatus), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(HealthStatus), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetHealth()
        {
            var health = new HealthStatus();

            try
            {
                // Check Command Store (PostgreSQL)
                health.CommandStore = await CheckCommandStoreHealth();
                
                // Check Query Store (MongoDB)
                health.QueryStore = await CheckQueryStoreHealth();

                health.Overall = health.CommandStore.IsHealthy && health.QueryStore.IsHealthy 
                    ? "Healthy" : "Unhealthy";

                var statusCode = health.CommandStore.IsHealthy && health.QueryStore.IsHealthy
                    ? StatusCodes.Status200OK
                    : StatusCodes.Status503ServiceUnavailable;

                return StatusCode(statusCode, health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking system health");
                health.Overall = "Unhealthy";
                health.Error = ex.Message;
                return StatusCode(StatusCodes.Status503ServiceUnavailable, health);
            }
        }

        /// <summary>
        /// Check health of command store only
        /// </summary>
        /// <returns>Command store health status</returns>
        [HttpGet("command")]
        [ProducesResponseType(typeof(StoreHealth), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StoreHealth), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetCommandStoreHealth()
        {
            try
            {
                var health = await CheckCommandStoreHealth();
                var statusCode = health.IsHealthy ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;
                return StatusCode(statusCode, health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking command store health");
                var health = new StoreHealth
                {
                    StoreName = "PostgreSQL (Command)",
                    IsHealthy = false,
                    Error = ex.Message
                };
                return StatusCode(StatusCodes.Status503ServiceUnavailable, health);
            }
        }

        /// <summary>
        /// Check health of query store only
        /// </summary>
        /// <returns>Query store health status</returns>
        [HttpGet("query")]
        [ProducesResponseType(typeof(StoreHealth), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StoreHealth), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetQueryStoreHealth()
        {
            try
            {
                var health = await CheckQueryStoreHealth();
                var statusCode = health.IsHealthy ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;
                return StatusCode(statusCode, health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking query store health");
                var health = new StoreHealth
                {
                    StoreName = "MongoDB (Query)",
                    IsHealthy = false,
                    Error = ex.Message
                };
                return StatusCode(StatusCodes.Status503ServiceUnavailable, health);
            }
        }

        private async Task<StoreHealth> CheckCommandStoreHealth()
        {
            var health = new StoreHealth { StoreName = "PostgreSQL (Command)" };
            
            try
            {
                var startTime = DateTime.UtcNow;
                
                // Test database connectivity
                await _commandDbContext.Database.CanConnectAsync();
                
                // Get basic statistics
                var userCount = await _commandDbContext.Users.CountAsync();
                
                var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                
                health.IsHealthy = true;
                health.ResponseTimeMs = responseTime;
                health.Details = new Dictionary<string, object>
                {
                    { "UserCount", userCount },
                    { "DatabaseProvider", _commandDbContext.Database.ProviderName ?? "Unknown" }
                };
            }
            catch (Exception ex)
            {
                health.IsHealthy = false;
                health.Error = ex.Message;
            }

            return health;
        }

        private async Task<StoreHealth> CheckQueryStoreHealth()
        {
            var health = new StoreHealth { StoreName = "MongoDB (Query)" };
            
            try
            {
                var startTime = DateTime.UtcNow;
                
                // Test database connectivity
                var isHealthy = await _queryDbContext.IsHealthyAsync();
                
                if (isHealthy)
                {
                    // Get basic statistics
                    var userCount = await _queryDbContext.Users.CountDocumentsAsync(Builders<UserQueryDocument>.Filter.Empty);
                    var dbStats = await _queryDbContext.GetDatabaseStatsAsync();
                    
                    var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    
                    health.IsHealthy = true;
                    health.ResponseTimeMs = responseTime;
                    health.Details = new Dictionary<string, object>
                    {
                        { "UserCount", userCount },
                        { "Collections", dbStats.Collections },
                        { "DataSizeMB", Math.Round(dbStats.DataSize / (1024.0 * 1024.0), 2) },
                        { "IndexSizeMB", Math.Round(dbStats.IndexSize / (1024.0 * 1024.0), 2) }
                    };
                }
                else
                {
                    health.IsHealthy = false;
                    health.Error = "MongoDB ping failed";
                }
            }
            catch (Exception ex)
            {
                health.IsHealthy = false;
                health.Error = ex.Message;
            }

            return health;
        }
    }

    /// <summary>
    /// Overall health status of the CQRS system
    /// </summary>
    public class HealthStatus
    {
        public string Overall { get; set; } = "Unknown";
        public StoreHealth CommandStore { get; set; } = new() { StoreName = "PostgreSQL (Command)" };
        public StoreHealth QueryStore { get; set; } = new() { StoreName = "MongoDB (Query)" };
        public string? Error { get; set; }
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Health status of an individual store
    /// </summary>
    public class StoreHealth
    {
        public string StoreName { get; set; } = string.Empty;
        public bool IsHealthy { get; set; }
        public double? ResponseTimeMs { get; set; }
        public Dictionary<string, object>? Details { get; set; }
        public string? Error { get; set; }
    }
}
