using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using HexagonalSkeleton.Infrastructure;
using System.Net;
using System.Text.Json;
using Xunit;

namespace HexagonalSkeleton.Test.Integration.API.Controllers
{
    /// <summary>
    /// Integration tests for exception handling in ExceptionDemoController
    /// Verifies that exceptions are properly mapped to HTTP status codes
    /// </summary>
    public class ExceptionDemoControllerIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;        public ExceptionDemoControllerIntegrationTest(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    // Remove the real database configuration
                    var descriptors = services.Where(d => 
                        d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                        d.ServiceType == typeof(AppDbContext)).ToList();
                    
                    foreach (var descriptor in descriptors)
                        services.Remove(descriptor);

                    // Add InMemory DbContext for testing
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));
                });
                  // Override the connection string to avoid MySQL AutoDetect
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:HexagonalSkeleton"] = "Server=localhost;Database=test;"
                    });
                });
            });
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GET_Success_Returns200()
        {
            // Act
            var response = await _client.GetAsync("/api/ExceptionDemo/success");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonSerializer.Deserialize<JsonElement>(content);
            Assert.Equal("Everything is working fine!", jsonDoc.GetProperty("message").GetString());
        }

        [Fact]
        public async Task GET_ValidationError_Returns400()
        {
            // Act
            var response = await _client.GetAsync("/api/ExceptionDemo/validation-error");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            
            var content = await response.Content.ReadAsStringAsync();
            var problemDetails = JsonSerializer.Deserialize<JsonElement>(content);
            Assert.Equal("Validation failed", problemDetails.GetProperty("title").GetString());
            Assert.Equal(400, problemDetails.GetProperty("status").GetInt32());
        }

        [Fact]
        public async Task GET_NotFound_Returns404()
        {
            // Act
            var response = await _client.GetAsync("/api/ExceptionDemo/not-found");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            
            var content = await response.Content.ReadAsStringAsync();
            var problemDetails = JsonSerializer.Deserialize<JsonElement>(content);
            Assert.Equal("Resource not found", problemDetails.GetProperty("title").GetString());
            Assert.Equal(404, problemDetails.GetProperty("status").GetInt32());
            Assert.Contains("999", problemDetails.GetProperty("detail").GetString());
        }

        [Fact]
        public async Task GET_AuthError_Returns401()
        {
            // Act
            var response = await _client.GetAsync("/api/ExceptionDemo/auth-error");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            
            var content = await response.Content.ReadAsStringAsync();
            var problemDetails = JsonSerializer.Deserialize<JsonElement>(content);
            Assert.Equal("Authentication failed", problemDetails.GetProperty("title").GetString());
            Assert.Equal(401, problemDetails.GetProperty("status").GetInt32());
        }

        [Fact]
        public async Task GET_Forbidden_Returns403()
        {
            // Act
            var response = await _client.GetAsync("/api/ExceptionDemo/forbidden");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            
            var content = await response.Content.ReadAsStringAsync();
            var problemDetails = JsonSerializer.Deserialize<JsonElement>(content);
            Assert.Equal("Authorization failed", problemDetails.GetProperty("title").GetString());
            Assert.Equal(403, problemDetails.GetProperty("status").GetInt32());
        }

        [Fact]
        public async Task GET_Conflict_Returns409()
        {
            // Act
            var response = await _client.GetAsync("/api/ExceptionDemo/conflict");

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            
            var content = await response.Content.ReadAsStringAsync();
            var problemDetails = JsonSerializer.Deserialize<JsonElement>(content);
            Assert.Equal("Resource conflict", problemDetails.GetProperty("title").GetString());
            Assert.Equal(409, problemDetails.GetProperty("status").GetInt32());
        }

        [Fact]
        public async Task GET_BusinessRule_Returns422()
        {
            // Act
            var response = await _client.GetAsync("/api/ExceptionDemo/business-rule");

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            
            var content = await response.Content.ReadAsStringAsync();
            var problemDetails = JsonSerializer.Deserialize<JsonElement>(content);
            Assert.Equal("Business rule violation", problemDetails.GetProperty("title").GetString());
            Assert.Equal(422, problemDetails.GetProperty("status").GetInt32());
        }

        [Fact]
        public async Task GET_RateLimit_Returns429()
        {
            // Act
            var response = await _client.GetAsync("/api/ExceptionDemo/rate-limit");

            // Assert
            Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            
            var content = await response.Content.ReadAsStringAsync();
            var problemDetails = JsonSerializer.Deserialize<JsonElement>(content);
            Assert.Equal("Too many requests", problemDetails.GetProperty("title").GetString());
            Assert.Equal(429, problemDetails.GetProperty("status").GetInt32());
        }

        [Theory]
        [InlineData("/api/ExceptionDemo/validation-error", HttpStatusCode.BadRequest)]
        [InlineData("/api/ExceptionDemo/not-found", HttpStatusCode.NotFound)]
        [InlineData("/api/ExceptionDemo/auth-error", HttpStatusCode.Unauthorized)]
        [InlineData("/api/ExceptionDemo/forbidden", HttpStatusCode.Forbidden)]
        [InlineData("/api/ExceptionDemo/conflict", HttpStatusCode.Conflict)]
        [InlineData("/api/ExceptionDemo/business-rule", HttpStatusCode.UnprocessableEntity)]
        [InlineData("/api/ExceptionDemo/rate-limit", HttpStatusCode.TooManyRequests)]
        public async Task GET_AllExceptionEndpoints_ReturnCorrectStatusCodes(string endpoint, HttpStatusCode expectedStatus)
        {
            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.Equal(expectedStatus, response.StatusCode);
            
            // All error responses should be ProblemDetails JSON
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            }
        }
    }
}
