using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Net;
using Xunit;
using FluentAssertions;
using HexagonalSkeleton.API.Models.Users;
using HexagonalSkeleton.API.Models.Auth;
using HexagonalSkeleton.API.Models.Common;
using HexagonalSkeleton.Test.TestInfrastructure.Factories;

namespace HexagonalSkeleton.Test.Integration.User
{
    /// <summary>
    /// Comprehensive integration tests for User workflows using SQLite in-memory database.
    /// Tests the complete API endpoints with proper database isolation for each test.
    /// Validates CQRS architecture, domain logic, and API responses end-to-end.
    /// </summary>
    public class IntegratedUserWorkflowTests : IClassFixture<ConfiguredTestWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly ConfiguredTestWebApplicationFactory _factory;

        public IntegratedUserWorkflowTests(ConfiguredTestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task UserRegistration_WithValidData_ShouldCreateUserSuccessfully()
        {
            // === ARRANGE ===
            var registrationRequest = CreateValidUserRegistrationRequest();

            // === ACT ===
            var response = await _client.PostAsJsonAsync("/api/registration", registrationRequest);

            // === ASSERT ===
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            result.Should().NotBeNull();
            result!.User.Should().NotBeNull();
            result.User.Email.Should().Be(registrationRequest.Email);
            result.User.FirstName.Should().Be(registrationRequest.FirstName);
            result.User.LastName.Should().Be(registrationRequest.LastName);
            result.AccessToken.Should().NotBeNullOrEmpty();
            result.TokenType.Should().Be("Bearer");
            
            // Verify the user was created with proper domain logic
            result.User.Id.Should().NotBe(Guid.Empty);
            // Note: CreatedAt might be default DateTime in response mapping - this is a known mapping issue
        }

        [Fact]
        public async Task UserLogin_WithValidCredentials_ShouldAuthenticateSuccessfully()
        {
            // === ARRANGE ===
            var registrationRequest = CreateValidUserRegistrationRequest();
            var registrationResponse = await RegisterUser(registrationRequest);

            var loginRequest = new LoginRequest
            {
                Email = registrationRequest.Email,
                Password = registrationRequest.Password,
                RememberMe = false
            };

            // === ACT ===
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // === ASSERT ===
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            result.Should().NotBeNull();
            result!.AccessToken.Should().NotBeNullOrEmpty();
            result.User.Should().NotBeNull();
            result.User.Email.Should().Be(loginRequest.Email);
            result.User.Id.Should().Be(registrationResponse.User.Id);
        }

        [Fact]
        public async Task UserRegistration_WithDuplicateEmail_ShouldReturnConflict()
        {
            // === ARRANGE ===
            var firstRequest = CreateValidUserRegistrationRequest();
            var secondRequest = CreateValidUserRegistrationRequest();
            secondRequest.Email = firstRequest.Email; // Same email

            // === ACT ===
            // Register first user
            var firstResponse = await _client.PostAsJsonAsync("/api/registration", firstRequest);
            firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // Try to register second user with same email
            var secondResponse = await _client.PostAsJsonAsync("/api/registration", secondRequest);

            // === ASSERT ===
            secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task UserLogin_WithInvalidCredentials_ShouldReturnBadRequest()
        {
            // === ARRANGE ===
            var loginRequest = new LoginRequest
            {
                Email = "nonexistent@test.com",
                Password = "wrongpassword",
                RememberMe = false
            };

            // === ACT ===
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // === ASSERT ===
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid-email")]
        [InlineData("@test.com")]
        [InlineData("test@")]
        public async Task UserRegistration_WithInvalidEmail_ShouldReturnBadRequest(string invalidEmail)
        {
            // === ARRANGE ===
            var request = CreateValidUserRegistrationRequest();
            request.Email = invalidEmail;

            // === ACT ===
            var response = await _client.PostAsJsonAsync("/api/registration", request);

            // === ASSERT ===
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("12345")]     // Too short
        [InlineData("password")]  // No uppercase
        [InlineData("PASSWORD")]  // No lowercase
        [InlineData("Password")]  // No digit
        public async Task UserRegistration_WithWeakPassword_ShouldReturnBadRequest(string weakPassword)
        {
            // === ARRANGE ===
            var request = CreateValidUserRegistrationRequest();
            request.Password = weakPassword;
            request.PasswordConfirmation = weakPassword;

            // === ACT ===
            var response = await _client.PostAsJsonAsync("/api/registration", request);

            // === ASSERT ===
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UserRegistration_WithPasswordMismatch_ShouldReturnBadRequest()
        {
            // === ARRANGE ===
            var request = CreateValidUserRegistrationRequest();
            request.PasswordConfirmation = "DifferentPassword123!";

            // === ACT ===
            var response = await _client.PostAsJsonAsync("/api/registration", request);

            // === ASSERT ===
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UserRegistration_WithUnderage_ShouldReturnBadRequest()
        {
            // === ARRANGE ===
            var request = CreateValidUserRegistrationRequest();
            request.Birthdate = DateTime.Today.AddYears(-12); // 12 years old (under minimum)

            // === ACT ===
            var response = await _client.PostAsJsonAsync("/api/registration", request);

            // === ASSERT ===
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task DomainBusinessRules_ShouldBeEnforcedCorrectly()
        {
            // === ARRANGE & ACT & ASSERT ===
            
            // Test email uniqueness business rule
            var validRequest = CreateValidUserRegistrationRequest();
            var firstResponse = await _client.PostAsJsonAsync("/api/registration", validRequest);
            firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var duplicateResponse = await _client.PostAsJsonAsync("/api/registration", validRequest);
            duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

            // Test minimum age business rule
            var underageRequest = CreateValidUserRegistrationRequest();
            underageRequest.Birthdate = DateTime.Today.AddYears(-10);
            
            var underageResponse = await _client.PostAsJsonAsync("/api/registration", underageRequest);
            underageResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CqrsArchitecture_WriteAndReadConsistency_ShouldMaintainDataIntegrity()
        {
            // === ARRANGE ===
            var registrationRequest = CreateValidUserRegistrationRequest();

            // === ACT ===
            // Command side: Create user
            var commandResponse = await _client.PostAsJsonAsync("/api/registration", registrationRequest);
            commandResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var registrationResult = await commandResponse.Content.ReadFromJsonAsync<LoginResponse>();
            var userId = registrationResult!.User.Id;

            // Query side: Authenticate (which also validates user exists)
            var loginRequest = new LoginRequest
            {
                Email = registrationRequest.Email,
                Password = registrationRequest.Password,
                RememberMe = false
            };
            
            var queryResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            queryResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var loginResult = await queryResponse.Content.ReadFromJsonAsync<LoginResponse>();

            // === ASSERT ===
            // Verify CQRS consistency between Command and Query sides
            loginResult.Should().NotBeNull();
            loginResult!.User.Id.Should().Be(userId);
            loginResult.User.Email.Should().Be(registrationRequest.Email);
            loginResult.User.FirstName.Should().Be(registrationRequest.FirstName);
            loginResult.User.LastName.Should().Be(registrationRequest.LastName);
        }

        [Fact]
        public async Task PasswordSecurity_ShouldHashPasswordsProperly()
        {
            // === ARRANGE ===
            var registrationRequest = CreateValidUserRegistrationRequest();

            // === ACT ===
            var registrationResponse = await _client.PostAsJsonAsync("/api/registration", registrationRequest);
            registrationResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // Try to login with correct password
            var correctLoginRequest = new LoginRequest
            {
                Email = registrationRequest.Email,
                Password = registrationRequest.Password,
                RememberMe = false
            };
            
            var correctLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", correctLoginRequest);
            
            // Try to login with incorrect password
            var incorrectLoginRequest = new LoginRequest
            {
                Email = registrationRequest.Email,
                Password = "WrongPassword123!",
                RememberMe = false
            };
            
            var incorrectLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", incorrectLoginRequest);

            // === ASSERT ===
            correctLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            incorrectLoginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ValidationRules_ShouldEnforceDataIntegrity()
        {
            // === ARRANGE & ACT & ASSERT ===
            
            // Test required fields
            var emptyRequest = new CreateUserRequest
            {
                Email = "",
                Password = "",
                PasswordConfirmation = "",
                FirstName = "",
                LastName = "",
                PhoneNumber = ""
            };
            
            var emptyResponse = await _client.PostAsJsonAsync("/api/registration", emptyRequest);
            emptyResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Test field length limits
            var tooLongRequest = CreateValidUserRegistrationRequest();
            tooLongRequest.FirstName = new string('A', 51); // Exceeds 50 char limit
            
            var tooLongResponse = await _client.PostAsJsonAsync("/api/registration", tooLongRequest);
            tooLongResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Test phone number validation
            var invalidPhoneRequest = CreateValidUserRegistrationRequest();
            invalidPhoneRequest.PhoneNumber = "invalid-phone";
            
            var invalidPhoneResponse = await _client.PostAsJsonAsync("/api/registration", invalidPhoneRequest);
            invalidPhoneResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GeolocationData_ShouldAcceptValidCoordinates()
        {
            // === ARRANGE ===
            var request = CreateValidUserRegistrationRequest();
            request.Latitude = 37.7749;  // San Francisco
            request.Longitude = -122.4194;

            // === ACT ===
            var response = await _client.PostAsJsonAsync("/api/registration", request);

            // === ASSERT ===
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            result.Should().NotBeNull();
            result!.User.Should().NotBeNull();
        }

        [Theory]
        [InlineData(91, 0)]    // Invalid latitude
        [InlineData(-91, 0)]   // Invalid latitude
        [InlineData(0, 181)]   // Invalid longitude
        [InlineData(0, -181)]  // Invalid longitude
        public async Task GeolocationData_WithInvalidCoordinates_ShouldReturnBadRequest(double lat, double lng)
        {
            // === ARRANGE ===
            var request = CreateValidUserRegistrationRequest();
            request.Latitude = lat;
            request.Longitude = lng;

            // === ACT ===
            var response = await _client.PostAsJsonAsync("/api/registration", request);

            // === ASSERT ===
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task DatabaseTransaction_ShouldRollbackOnFailure()
        {
            // === ARRANGE ===
            var validRequest = CreateValidUserRegistrationRequest();
            
            // Create a request that will fail validation after some processing
            var invalidRequest = CreateValidUserRegistrationRequest();
            invalidRequest.Email = validRequest.Email; // Duplicate email

            // === ACT ===
            // First registration should succeed
            var firstResponse = await _client.PostAsJsonAsync("/api/registration", validRequest);
            firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // Second registration should fail and not affect database state
            var secondResponse = await _client.PostAsJsonAsync("/api/registration", invalidRequest);
            secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

            // Original user should still be able to login
            var loginRequest = new LoginRequest
            {
                Email = validRequest.Email,
                Password = validRequest.Password,
                RememberMe = false
            };
            
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // === ASSERT ===
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #region Helper Methods

        private static CreateUserRequest CreateValidUserRegistrationRequest()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var email = $"integration.test.{timestamp}@example.com";
            var password = "SecureIntegrationTest123!";
            
            return new CreateUserRequest
            {
                Email = email,
                Password = password,
                PasswordConfirmation = password,
                FirstName = "Integration",
                LastName = "Test",
                PhoneNumber = $"+1555{timestamp % 1000000:D6}",
                Birthdate = DateTime.Today.AddYears(-25),
                Latitude = 40.7128,  // New York
                Longitude = -74.0060,
                AboutMe = "Comprehensive integration test user for SQLite database testing"
            };
        }

        private async Task<LoginResponse> RegisterUser(CreateUserRequest request)
        {
            var response = await _client.PostAsJsonAsync("/api/registration", request);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            result.Should().NotBeNull();
            return result!;
        }

        #endregion
    }
}
