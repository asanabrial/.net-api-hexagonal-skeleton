using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using HexagonalSkeleton.API.Models.Auth;
using HexagonalSkeleton.API.Models.Users;
using HexagonalSkeleton.API.Models.Common;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;
using HexagonalSkeleton.Test.TestInfrastructure.Factories;
using HexagonalSkeleton.Test.TestInfrastructure.Base;

namespace HexagonalSkeleton.Test.Integration.User
{
    public class GetAllUsersIntegrationTest : BaseIntegrationTest, IClassFixture<ConfiguredTestWebApplicationFactory>
    {
        public GetAllUsersIntegrationTest(ConfiguredTestWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task GetAllUsers_WithExistingUsers_ShouldReturnUsers()
        {
            // Arrange - Create test user first to get token
            var uniqueEmail = $"getalltest{Guid.NewGuid():N}@example.com";
            var uniquePhone = $"+1{DateTime.UtcNow.Ticks % 9000000000 + 1000000000}";
            var testUserRequest = new CreateUserRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = uniqueEmail,
                Password = "TestPassword123!",
                PasswordConfirmation = "TestPassword123!",
                PhoneNumber = uniquePhone,
                Birthdate = new DateTime(1990, 1, 1),
                Latitude = 40.7128,
                Longitude = -74.0060,
                AboutMe = "Test user for GetAll test"
            };

            // Create user and get token
            var userJson = JsonSerializer.Serialize(testUserRequest);
            var userContent = new StringContent(userJson, Encoding.UTF8, "application/json");
            var userResponse = await _client.PostAsync("/api/auth/register", userContent);
            
            var userResponseContent = await userResponse.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(userResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            // Set authorization header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse!.AccessToken);

            // Wait a moment for CQRS event propagation to complete
            await Task.Delay(100);

            // Act - Get all users
            var getAllResponse = await _client.GetAsync("/api/users");

            // Assert
            getAllResponse.EnsureSuccessStatusCode();
            var getAllContent = await getAllResponse.Content.ReadAsStringAsync();
            
            var usersResponse = JsonSerializer.Deserialize<PagedResponse<UserResponse>>(getAllContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            Assert.NotNull(usersResponse);
            Assert.NotNull(usersResponse.Data);
            Assert.True(usersResponse.TotalCount > 0, $"Expected at least 1 user, but got {usersResponse.TotalCount}");
            Assert.True(usersResponse.Data.Count() > 0, $"Expected at least 1 user in data, but got {usersResponse.Data.Count()}");
        }
    }
}
