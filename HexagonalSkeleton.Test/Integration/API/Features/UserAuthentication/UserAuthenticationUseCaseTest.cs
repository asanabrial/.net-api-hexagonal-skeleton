using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HexagonalSkeleton.API.Models.Auth;
using HexagonalSkeleton.API.Models.Users;
using HexagonalSkeleton.Test.TestInfrastructure.Factories;

namespace HexagonalSkeleton.Test.Integration.UseCases
{
    /// <summary>
    /// Integration test for the User Authentication use case.
    /// Tests login functionality with proper credential validation.
    /// Uses Testcontainers for complete isolation and real database testing.
    /// </summary>
    public class UserAuthenticationUseCaseTest : IClassFixture<ConfiguredTestWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly ConfiguredTestWebApplicationFactory _factory;

        public UserAuthenticationUseCaseTest(ConfiguredTestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnAuthenticationToken()
        {
            // === ARRANGE ===
            // First register a user
            var registrationRequest = CreateValidUserRegistrationRequest();
            var registrationResponse = await _client.PostAsJsonAsync("/api/registration", registrationRequest);
            registrationResponse.StatusCode.Should().Be(HttpStatusCode.Created);

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
            
            // Verify authentication response
            result!.AccessToken.Should().NotBeNullOrEmpty();
            result.TokenType.Should().Be("Bearer");
            result.ExpiresIn.Should().BeGreaterThan(0);
            
            // Verify user information
            result.User.Should().NotBeNull();
            result.User.Email.Should().Be(loginRequest.Email);
            result.User.FirstName.Should().Be(registrationRequest.FirstName);
            result.User.LastName.Should().Be(registrationRequest.LastName);
        }

        [Fact]
        public async Task Login_WithNonExistentUser_ShouldReturnNotFound()
        {
            // === ARRANGE ===
            var loginRequest = new LoginRequest
            {
                Email = "nonexistent@example.com",
                Password = "AnyPassword123!",
                RememberMe = false
            };

            // === ACT ===
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // === ASSERT ===
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Login_WithIncorrectPassword_ShouldReturnUnauthorized()
        {
            // === ARRANGE ===
            // First register a user
            var registrationRequest = CreateValidUserRegistrationRequest();
            var registrationResponse = await _client.PostAsJsonAsync("/api/registration", registrationRequest);
            registrationResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var loginRequest = new LoginRequest
            {
                Email = registrationRequest.Email,
                Password = "WrongPassword123!",
                RememberMe = false
            };

            // === ACT ===
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // === ASSERT ===
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_WithInvalidEmailFormat_ShouldReturnBadRequest()
        {
            // === ARRANGE ===
            var loginRequest = new LoginRequest
            {
                Email = "invalid-email-format",
                Password = "AnyPassword123!",
                RememberMe = false
            };

            // === ACT ===
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // === ASSERT ===
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WithRememberMe_ShouldReturnExtendedToken()
        {
            // === ARRANGE ===
            // First register a user
            var registrationRequest = CreateValidUserRegistrationRequest();
            var registrationResponse = await _client.PostAsJsonAsync("/api/registration", registrationRequest);
            registrationResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var loginRequest = new LoginRequest
            {
                Email = registrationRequest.Email,
                Password = registrationRequest.Password,
                RememberMe = true
            };

            // === ACT ===
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // === ASSERT ===
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            result.Should().NotBeNull();
            result!.AccessToken.Should().NotBeNullOrEmpty();
            
            // Remember me should provide longer expiration
            result.ExpiresIn.Should().BeGreaterThan(3600); // More than 1 hour
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task Login_WithEmptyEmail_ShouldReturnBadRequest(string? email)
        {
            // === ARRANGE ===
            var loginRequest = new LoginRequest
            {
                Email = email!,
                Password = "AnyPassword123!",
                RememberMe = false
            };

            // === ACT ===
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // === ASSERT ===
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task Login_WithEmptyPassword_ShouldReturnBadRequest(string? password)
        {
            // === ARRANGE ===
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = password!,
                RememberMe = false
            };

            // === ACT ===
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // === ASSERT ===
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private static CreateUserRequest CreateValidUserRegistrationRequest()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var password = "SecureTestPassword123!";
            
            return new CreateUserRequest
            {
                Email = $"auth.test.{timestamp}@example.com",
                Password = password,
                PasswordConfirmation = password,
                FirstName = "Auth",
                LastName = "Test",
                PhoneNumber = $"+1666{timestamp % 1000000:D6}",
                Birthdate = DateTime.Today.AddYears(-28),
                Latitude = 37.7749,  // San Francisco coordinates
                Longitude = -122.4194,
                AboutMe = "Integration test user for authentication use case"
            };
        }
    }
}
