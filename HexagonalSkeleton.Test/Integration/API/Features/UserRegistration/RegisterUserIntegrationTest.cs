using System.Text;
using System.Text.Json;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using HexagonalSkeleton.API;
using HexagonalSkeleton.API.Models.Users;
using HexagonalSkeleton.API.Models.Auth;
using Microsoft.EntityFrameworkCore;
using HexagonalSkeleton.Infrastructure.Persistence.Command;
using HexagonalSkeleton.Test.TestInfrastructure.Factories;

namespace HexagonalSkeleton.Test.Integration
{    
    public class RegisterUserIntegrationTest : IClassFixture<ConfiguredTestWebApplicationFactory>
    {
        private readonly ConfiguredTestWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public RegisterUserIntegrationTest(ConfiguredTestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }        [Fact]
        public async Task RegisterUser_ShouldReturnAllUserData()
        {            // Arrange
            var request = new CreateUserRequest
            {
                Email = "test@example.com",
                Password = "Password123!",
                PasswordConfirmation = "Password123!",
                FirstName = "John",
                LastName = "Doe",
                Birthdate = DateTime.UtcNow.AddYears(-25),
                PhoneNumber = "+1234567890",
                Latitude = 40.7128,
                Longitude = -74.0060,
                AboutMe = "Test about me"
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/auth/register", content);

            // Assert
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync();
            
            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(loginResponse);
            Assert.NotNull(loginResponse.AccessToken);
            Assert.NotEmpty(loginResponse.AccessToken);
            Assert.Equal("Bearer", loginResponse.TokenType);

            // Verify all user data is returned
            Assert.NotNull(loginResponse.User);
            Assert.NotEqual(Guid.Empty, loginResponse.User.Id);
            Assert.Equal("John", loginResponse.User.FirstName);
            Assert.Equal("Doe", loginResponse.User.LastName);
            Assert.Equal("John Doe", loginResponse.User.FullName);
            Assert.Equal("test@example.com", loginResponse.User.Email);
            Assert.Equal("+1234567890", loginResponse.User.PhoneNumber);
            Assert.Equal(request.Birthdate.Date, loginResponse.User.Birthdate?.Date);
            Assert.Equal(40.7128, loginResponse.User.Latitude);            Assert.Equal(-74.0060, loginResponse.User.Longitude);
            Assert.Equal("Test about me", loginResponse.User.AboutMe);
            Assert.True(loginResponse.User.CreatedAt > DateTime.MinValue);
        }        [Fact]
        public async Task RegisterUser_DuplicateEmail_ShouldReturn409Conflict()
        {
            // Create a new client with its own database for this test to avoid interference
            var testDatabaseName = "DuplicateEmailTest" + Guid.NewGuid().ToString();
            
            using var testFactory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove all DbContext related registrations
                    var descriptors = services.Where(d => 
                        d.ServiceType == typeof(DbContextOptions<CommandDbContext>) ||
                        d.ServiceType == typeof(CommandDbContext) ||
                        d.ServiceType.IsGenericType && 
                        d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>) ||
                        d.ImplementationType?.FullName?.Contains("DbContext") == true ||
                        d.ServiceType.FullName?.Contains("DbContext") == true)
                        .ToList();
                    
                    foreach (var descriptor in descriptors)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database for testing
                    services.AddDbContext<CommandDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(testDatabaseName);
                    }, ServiceLifetime.Scoped);
                });
            });
            
            using var testClient = testFactory.CreateClient();
            
            // Arrange
            var uniqueEmail = $"duplicate{Guid.NewGuid():N}@example.com";
            var uniquePhone = $"+1{DateTime.UtcNow.Ticks % 9000000000 + 1000000000}";
            var request = new CreateUserRequest
            {
                Email = uniqueEmail,
                Password = "Password123!",
                PasswordConfirmation = "Password123!",
                FirstName = "John",
                LastName = "Doe",
                Birthdate = DateTime.UtcNow.AddYears(-25),
                PhoneNumber = uniquePhone,
                Latitude = 40.7128,
                Longitude = -74.0060,
                AboutMe = "Test about me"
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");            // Act - Create user first time (should succeed)
            var firstResponse = await testClient.PostAsync("/api/auth/register", content);
            firstResponse.EnsureSuccessStatusCode();

            // Act - Try to create user with same email (should fail)
            var secondResponse = await testClient.PostAsync("/api/auth/register", content);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Conflict, secondResponse.StatusCode);
            
            var responseContent = await secondResponse.Content.ReadAsStringAsync();
            
            // Parse the problem details response
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;
            
            Assert.True(root.TryGetProperty("title", out var titleElement));
            Assert.Equal("Data conflict", titleElement.GetString());
            
            Assert.True(root.TryGetProperty("status", out var statusElement));
            Assert.Equal(409, statusElement.GetInt32());
            
            Assert.True(root.TryGetProperty("detail", out var detailElement));
            Assert.Contains(uniqueEmail, detailElement.GetString());
            
            // Check that email is included in extensions
            Assert.True(root.TryGetProperty("email", out var emailElement));
            Assert.Equal(uniqueEmail, emailElement.GetString());
        }
    }
}
