using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using HexagonalSkeleton.API.Models.Auth;
using HexagonalSkeleton.API.Models.Users;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

namespace HexagonalSkeleton.Test.Integration.User
{
    public class GetAllUsersIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public GetAllUsersIntegrationTest(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetAllUsers_WithExistingUsers_ShouldReturnUsers()
        {
            // Arrange - Create test user first to get token
            var testUserRequest = new CreateUserRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = "getalltest@example.com",
                Password = "TestPassword123!",
                PasswordConfirmation = "TestPassword123!",
                PhoneNumber = "+1234567801",
                Birthdate = new DateTime(1990, 1, 1),
                Latitude = 40.7128,
                Longitude = -74.0060,
                AboutMe = "Test user for GetAll test"
            };

            // Create user and get token
            var userJson = JsonSerializer.Serialize(testUserRequest);
            var userContent = new StringContent(userJson, Encoding.UTF8, "application/json");
            var userResponse = await _client.PostAsync("/api/user", userContent);
            
            var userResponseContent = await userResponse.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(userResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            // Set authorization header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse!.AccessToken);

            // Act - Get all users
            var getAllResponse = await _client.GetAsync("/api/user");

            // Assert
            getAllResponse.EnsureSuccessStatusCode();
            var getAllContent = await getAllResponse.Content.ReadAsStringAsync();
            
            // Debug: Print the response
            Console.WriteLine($"GetAll Response: {getAllContent}");
            
            var usersResponse = JsonSerializer.Deserialize<UsersResponse>(getAllContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            Assert.NotNull(usersResponse);
            Assert.NotNull(usersResponse.Data);
            Assert.True(usersResponse.TotalCount > 0, $"Expected at least 1 user, but got {usersResponse.TotalCount}");
            Assert.True(usersResponse.Data.Count() > 0, $"Expected at least 1 user in data, but got {usersResponse.Data.Count()}");
        }
    }
}
