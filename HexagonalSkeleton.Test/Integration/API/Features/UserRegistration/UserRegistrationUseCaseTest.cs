using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HexagonalSkeleton.API.Models.Users;
using HexagonalSkeleton.API.Models.Auth;
using HexagonalSkeleton.Test.TestInfrastructure.Factories;
using HexagonalSkeleton.Test.TestInfrastructure.Base;

namespace HexagonalSkeleton.Test.Integration.UseCases
{
    /// <summary>
    /// Integration test for the User Registration use case.
    /// Tests the complete flow: API -> Application -> Domain -> Infrastructure -> Database
    /// Uses Testcontainers with real PostgreSQL and MongoDB databases for comprehensive testing.
    /// Implements automatic database cleanup between tests to ensure isolation.
    /// </summary>
    public class UserRegistrationUseCaseTest : BaseIntegrationTestWithCleanup, IClassFixture<ConfiguredTestWebApplicationFactory>
    {
        public UserRegistrationUseCaseTest(ConfiguredTestWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task RegisterUser_WithValidData_ShouldCreateUserSuccessfully()
        {
            // === ARRANGE ===
            var request = CreateValidUserRegistrationRequest();

            // === ACT ===
            var response = await _client.PostAsJsonAsync("/api/registration", request);

            // === ASSERT ===
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Server Error: {errorContent}");
            }
            
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            result.Should().NotBeNull();
            
            // Verify authentication response
            result!.AccessToken.Should().NotBeNullOrEmpty();
            result.TokenType.Should().Be("Bearer");
            result.ExpiresIn.Should().BeGreaterThan(0);
            
            // Verify user data
            result.User.Should().NotBeNull();
            result.User.Id.Should().NotBe(Guid.Empty);
            result.User.Email.Should().Be(request.Email);
            result.User.FirstName.Should().Be(request.FirstName);
            result.User.LastName.Should().Be(request.LastName);
            result.User.FullName.Should().Be($"{request.FirstName} {request.LastName}");
            result.User.PhoneNumber.Should().Be(request.PhoneNumber);
            result.User.Birthdate.Should().NotBeNull();
            result.User.Latitude.Should().Be(request.Latitude);
            result.User.Longitude.Should().Be(request.Longitude);
            result.User.AboutMe.Should().Be(request.AboutMe);
        }

        [Fact]
        public async Task RegisterUser_WithDuplicateEmail_ShouldReturnConflict()
        {
            // === ARRANGE ===
            var request = CreateValidUserRegistrationRequest();

            // First registration - should succeed
            var firstResponse = await _client.PostAsJsonAsync("/api/registration", request);
            firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // === ACT ===
            // Second registration with same email - should fail
            var secondResponse = await _client.PostAsJsonAsync("/api/registration", request);

            // === ASSERT ===
            secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
            
            var errorContent = await secondResponse.Content.ReadAsStringAsync();
            errorContent.Should().Contain("already exists");
        }

        [Fact]
        public async Task RegisterUser_WithInvalidEmail_ShouldReturnBadRequest()
        {
            // === ARRANGE ===
            var request = CreateValidUserRegistrationRequest();
            request.Email = "invalid-email-format";

            // === ACT ===
            var response = await _client.PostAsJsonAsync("/api/registration", request);

            // === ASSERT ===
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RegisterUser_WithPasswordMismatch_ShouldReturnBadRequest()
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
        public async Task RegisterUser_WithUnderageUser_ShouldReturnBadRequest()
        {
            // === ARRANGE ===
            var request = CreateValidUserRegistrationRequest();
            request.Birthdate = DateTime.Today.AddYears(-10); // 10 years old

            // === ACT ===
            var response = await _client.PostAsJsonAsync("/api/registration", request);

            // === ASSERT ===
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            
            var errorContent = await response.Content.ReadAsStringAsync();
            errorContent.Should().Contain("13 years old");
        }

        [Theory]
        [InlineData("")]
        [InlineData("weak")]
        [InlineData("nouppercaseordigit")]
        [InlineData("NOLOWERCASEORDIGIT")]
        [InlineData("NoDigit")]
        public async Task RegisterUser_WithWeakPassword_ShouldReturnBadRequest(string weakPassword)
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

        [Theory]
        [InlineData(91, 0)]    // Invalid latitude
        [InlineData(-91, 0)]   // Invalid latitude  
        [InlineData(0, 181)]   // Invalid longitude
        [InlineData(0, -181)]  // Invalid longitude
        public async Task RegisterUser_WithInvalidCoordinates_ShouldReturnBadRequest(double lat, double lng)
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

        private static CreateUserRequest CreateValidUserRegistrationRequest()
        {
            var uniqueId = Guid.NewGuid().ToString("N")[..12]; // Use 12 chars of GUID for uniqueness
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var password = "SecureTestPassword123!";
            
            return new CreateUserRequest
            {
                Email = $"test.user.{uniqueId}.{timestamp}@example.com",
                Password = password,
                PasswordConfirmation = password,
                FirstName = "Integration",
                LastName = "Test",
                PhoneNumber = $"+1555{timestamp % 1000000:D6}",
                Birthdate = DateTime.Today.AddYears(-25),
                Latitude = 40.7128,  // New York coordinates
                Longitude = -74.0060,
                AboutMe = "Integration test user for registration use case"
            };
        }
    }
}
