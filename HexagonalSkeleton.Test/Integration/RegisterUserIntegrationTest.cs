using System.Text;
using System.Text.Json;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using HexagonalSkeleton.API;
using HexagonalSkeleton.API.Models.Users;
using HexagonalSkeleton.API.Models.Auth;
using HexagonalSkeleton.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HexagonalSkeleton.Test.Integration
{
    public class RegisterUserIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;        public RegisterUserIntegrationTest(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove all DbContext related registrations
                    var descriptors = services.Where(d => 
                        d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                        d.ServiceType == typeof(AppDbContext) ||
                        d.ServiceType.IsGenericType && 
                        d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>) ||
                        d.ImplementationType?.FullName?.Contains("DbContext") == true ||
                        d.ServiceType.FullName?.Contains("DbContext") == true)
                        .ToList();
                    
                    foreach (var descriptor in descriptors)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database for testing with scoped lifetime
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb" + Guid.NewGuid());
                    }, ServiceLifetime.Scoped);
                });
            });
            
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
            var response = await _client.PostAsync("/api/user", content);

            // Assert
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync();
            
            // Debug: Print the actual response to understand the structure
            System.Console.WriteLine($"Response content: {responseContent}");
            
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
            Assert.True(loginResponse.User.Id > 0, $"Expected User.Id > 0, but got {loginResponse.User.Id}");
            Assert.Equal("John", loginResponse.User.FirstName);
            Assert.Equal("Doe", loginResponse.User.LastName);
            Assert.Equal("John Doe", loginResponse.User.FullName);
            Assert.Equal("test@example.com", loginResponse.User.Email);
            Assert.Equal("+1234567890", loginResponse.User.PhoneNumber);
            Assert.Equal(request.Birthdate.Date, loginResponse.User.Birthdate?.Date);
            Assert.Equal(40.7128, loginResponse.User.Latitude);
            Assert.Equal(-74.0060, loginResponse.User.Longitude);
            Assert.Equal("Test about me", loginResponse.User.AboutMe);
            Assert.True(loginResponse.User.CreatedAt > DateTime.MinValue);
        }
    }
}
