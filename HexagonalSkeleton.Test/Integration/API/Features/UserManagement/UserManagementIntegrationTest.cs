using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;
using FluentAssertions;
using HexagonalSkeleton.API.Models.Users;
using HexagonalSkeleton.API.Models.Auth;
using HexagonalSkeleton.API.Models.Common;
using HexagonalSkeleton.Test.TestInfrastructure.Factories;
using HexagonalSkeleton.Test.TestInfrastructure.Base;

namespace HexagonalSkeleton.Test.Integration.API.Features.UserManagement
{
    /// <summary>
    /// Integration tests for User Management workflows.
    /// Tests the complete flow from API to database using Testcontainers.
    /// Validates CQRS architecture, domain logic, and API responses.
    /// Implements automatic database cleanup between tests to ensure isolation.
    /// </summary>
    public class UserManagementIntegrationTest : BaseIntegrationTest<ConfiguredTestWebApplicationFactory>, IClassFixture<ConfiguredTestWebApplicationFactory>
    {
        public UserManagementIntegrationTest(ConfiguredTestWebApplicationFactory factory) : base(factory)
        {
        }

        /// <summary>
        /// Configures authentication by registering a test user and setting the Authorization header
        /// </summary>
        private async Task SetupAuthenticationAsync()
        {
            var uniqueEmail = $"auth{Guid.NewGuid():N}@example.com";
            var uniquePhone = $"+1{DateTime.UtcNow.Ticks % 9000000000 + 1000000000}";
            
            var authUserRequest = new CreateUserRequest
            {
                Email = uniqueEmail,
                FirstName = "Auth",
                LastName = "User",
                PhoneNumber = uniquePhone,
                Password = "TempPass123!",
                PasswordConfirmation = "TempPass123!",
                Latitude = 40.7128,
                Longitude = -74.0060,
                Birthdate = DateTime.Now.AddYears(-25)
            };

            // Register user and get token
            var userResponse = await _client.PostAsJsonAsync("/api/auth/register", authUserRequest);
            
            // Ensure registration was successful
            userResponse.EnsureSuccessStatusCode();
            
            var userResponseContent = await userResponse.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<AuthenticatedRegistrationResponse>(userResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            // Ensure we got a valid token
            if (string.IsNullOrWhiteSpace(loginResponse?.AccessToken))
                throw new InvalidOperationException("Failed to get access token from registration response");
            
            // Set authorization header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

            // Wait for CQRS event propagation
            await Task.Delay(100);
        }

        [Fact]
        public async Task CreateUser_ValidData_ShouldReturnCreatedResult()
        {
            // Arrange
            var uniqueEmail = $"user{Guid.NewGuid():N}@example.com";
            var uniquePhone = $"+1{DateTime.UtcNow.Ticks % 9000000000 + 1000000000}"; // Generate unique 10-digit phone
            var request = new CreateUserRequest
            {
                Email = uniqueEmail,
                FirstName = "Test",
                LastName = "User",
                PhoneNumber = uniquePhone,
                Latitude = 40.7128,
                Longitude = -74.0060,
                Birthdate = DateTime.Now.AddYears(-25),
                Password = "TestPassword123!",
                PasswordConfirmation = "TestPassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var result = await response.Content.ReadFromJsonAsync<AuthenticatedRegistrationResponse>();
            result.Should().NotBeNull();
            result!.User.Email.Should().Be(uniqueEmail);
        }

        [Fact]
        public async Task CreateUser_DuplicateEmail_ShouldReturnConflict()
        {
            // Arrange
            var uniqueEmail = $"duplicate{Guid.NewGuid():N}@example.com";
            var uniquePhone1 = $"+1{DateTime.UtcNow.Ticks % 9000000000 + 1000000000}";
            var uniquePhone2 = $"+1{(DateTime.UtcNow.Ticks + 1) % 9000000000 + 1000000000}";
            
            var registerRequest = new CreateUserRequest
            {
                Email = uniqueEmail,
                FirstName = "Test",
                LastName = "User",
                PhoneNumber = uniquePhone1,
                Latitude = 40.7128,
                Longitude = -74.0060,
                Birthdate = DateTime.Now.AddYears(-25),
                Password = "TestPassword123!",
                PasswordConfirmation = "TestPassword123!"
            };

            // Create first user
            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Try to create duplicate with different phone
            registerRequest.PhoneNumber = uniquePhone2;
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturnPagedResults()
        {
            // Arrange - Setup authentication
            await SetupAuthenticationAsync();

            // Act
            var response = await _client.GetAsync("/api/users?page=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<PagedResponse<UserResponse>>();
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateUserProfile_ValidData_ShouldReturnSuccess()
        {
            // Arrange - Create a user first
            var uniqueEmail = $"update{Guid.NewGuid():N}@example.com";
            var uniquePhone = $"+1{DateTime.UtcNow.Ticks % 9000000000 + 1000000000}";
            var registerRequest = new CreateUserRequest
            {
                Email = uniqueEmail,
                FirstName = "Original",
                LastName = "User",
                PhoneNumber = uniquePhone,
                Latitude = 40.7128,
                Longitude = -74.0060,
                Birthdate = DateTime.Now.AddYears(-25),
                Password = "TestPassword123!",
                PasswordConfirmation = "TestPassword123!"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            var createResult = await createResponse.Content.ReadFromJsonAsync<AuthenticatedRegistrationResponse>();
            var userId = createResult!.User.Id;

            // Setup authorization with the token from registration
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", createResult.AccessToken);

            var updateRequest = new UpdateProfileRequest
            {
                FirstName = "Updated",
                LastName = "UserProfile",
                PhoneNumber = $"+1{(DateTime.UtcNow.Ticks + 2) % 9000000000 + 1000000000}", // Different unique phone
                Birthdate = DateTime.Now.AddYears(-30), // Adding birthdate as required by validator
                AboutMe = "Updated profile information" // Adding AboutMe as required by validator
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/users/{userId}/profile", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<UserResponse>();
            result.Should().NotBeNull();
            result!.FirstName.Should().Be("Updated");
            result.LastName.Should().Be("UserProfile");
        }

        [Fact]
        public async Task DeactivateUser_ValidUser_ShouldReturnSuccess()
        {
            // Arrange - Create a user first
            var uniqueEmail = $"deactivate{Guid.NewGuid():N}@example.com";
            var uniquePhone = $"+1{DateTime.UtcNow.Ticks % 9000000000 + 1000000000}";
            var registerRequest = new CreateUserRequest
            {
                Email = uniqueEmail,
                FirstName = "Test",
                LastName = "User",
                PhoneNumber = uniquePhone,
                Latitude = 40.7128,
                Longitude = -74.0060,
                Birthdate = DateTime.Now.AddYears(-25),
                Password = "TestPassword123!",
                PasswordConfirmation = "TestPassword123!"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            var createResult = await createResponse.Content.ReadFromJsonAsync<AuthenticatedRegistrationResponse>();
            var userId = createResult!.User.Id;

            // Setup authorization with the token from registration
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", createResult.AccessToken);

            // Act
            var response = await _client.PatchAsync($"/api/users/{userId}/deactivate", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<DeleteUserResponse>();
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteUser_ValidUser_ShouldReturnSuccess()
        {
            // Arrange - Create a user first
            var uniqueEmail = $"delete{Guid.NewGuid():N}@example.com";
            var uniquePhone = $"+1{DateTime.UtcNow.Ticks % 9000000000 + 1000000000}";
            var registerRequest = new CreateUserRequest
            {
                Email = uniqueEmail,
                FirstName = "Test",
                LastName = "User",
                PhoneNumber = uniquePhone,
                Latitude = 40.7128,
                Longitude = -74.0060,
                Birthdate = DateTime.Now.AddYears(-25),
                Password = "TestPassword123!",
                PasswordConfirmation = "TestPassword123!"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            var createResult = await createResponse.Content.ReadFromJsonAsync<AuthenticatedRegistrationResponse>();
            var userId = createResult!.User.Id;

            // Setup authorization with the token from registration
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", createResult.AccessToken);

            // Act
            var response = await _client.DeleteAsync($"/api/users/{userId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task GetUserById_ValidUser_ShouldReturnUser()
        {
            // Arrange - Create a user first
            var uniqueEmail = $"getbyid{Guid.NewGuid():N}@example.com";
            var uniquePhone = $"+1{DateTime.UtcNow.Ticks % 9000000000 + 1000000000}";
            var registerRequest = new CreateUserRequest
            {
                Email = uniqueEmail,
                FirstName = "Test",
                LastName = "User",
                PhoneNumber = uniquePhone,
                Latitude = 40.7128,
                Longitude = -74.0060,
                Birthdate = DateTime.Now.AddYears(-25),
                Password = "TestPassword123!",
                PasswordConfirmation = "TestPassword123!"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            var createResult = await createResponse.Content.ReadFromJsonAsync<AuthenticatedRegistrationResponse>();
            var userId = createResult!.User.Id;

            // Setup authorization with the token from registration
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", createResult.AccessToken);

            // Wait for CQRS event propagation
            await Task.Delay(50);

            // Act
            var response = await _client.GetAsync($"/api/users/{userId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<UserResponse>();
            result.Should().NotBeNull();
            result!.Id.Should().Be(userId);
            result.Email.Should().Be(uniqueEmail);
        }

        [Fact]
        public async Task GetUserById_NonExistentUser_ShouldReturnNotFound()
        {
            // Arrange - Setup authentication
            await SetupAuthenticationAsync();
            
            var nonExistentUserId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/users/{nonExistentUserId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
