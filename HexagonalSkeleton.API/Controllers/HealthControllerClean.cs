using Microsoft.AspNetCore.Mvc;

namespace HexagonalSkeleton.API.Controllers
{
    /// <summary>
    /// Health check controller following Clean Architecture principles
    /// Provides simple health status without direct dependencies on infrastructure
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Basic health check endpoint
        /// Returns 200 OK if the API is running
        /// </summary>
        [HttpGet]
        public IActionResult GetHealth()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Service = "HexagonalSkeleton.API"
            });
        }

        /// <summary>
        /// Detailed health check with basic system information
        /// Following Clean Architecture by not accessing infrastructure directly
        /// </summary>
        [HttpGet("detailed")]
        public IActionResult GetDetailedHealth()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Service = "HexagonalSkeleton.API",
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                MachineName = Environment.MachineName,
                ProcessorCount = Environment.ProcessorCount,
                WorkingSet = GC.GetTotalMemory(false)
            });
        }
    }
}
