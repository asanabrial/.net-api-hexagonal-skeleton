using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using HexagonalSkeleton.API.Models.Users;
using HexagonalSkeleton.API.Models.Auth;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Test.TestInfrastructure.Base;
using HexagonalSkeleton.Test.TestInfrastructure.Factories;
using HexagonalSkeleton.Test.TestInfrastructure.Helpers;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Xunit;
using AutoMapper;

namespace HexagonalSkeleton.Test.Integration.UserProfile;

[Collection("Integration Collection")]
public class UserProfileControllerLogicalDeletionTest : BaseIntegrationTest, IClassFixture<ConfiguredTestWebApplicationFactory>
{
    public UserProfileControllerLogicalDeletionTest(ConfiguredTestWebApplicationFactory factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task GetMyProfile_WhenUserIsDeleted_ShouldReturn401()
    {
        // Arrange - Create user and get auth token
        var uniqueEmail = $"profile.deletion.test.{Guid.NewGuid():N}@example.com";
        var uniquePhone = $"+1{DateTime.UtcNow.Ticks % 9000000000 + 1000000000}";
        
        var createUserRequest = new CreateUserRequest
        {
            FirstName = "Test",
            LastName = "User Profile Deletion",
            Email = uniqueEmail,
            PhoneNumber = uniquePhone,
            Password = "TestPassword123!",
            PasswordConfirmation = "TestPassword123!",
            Birthdate = new DateTime(1990, 1, 1),
            Latitude = 40.7128,
            Longitude = -74.0060,
            AboutMe = "Test user for profile deletion test"
        };

        var userJson = JsonSerializer.Serialize(createUserRequest);
        var userContent = new StringContent(userJson, Encoding.UTF8, "application/json");
        var registrationResponse = await _client.PostAsync("/api/auth/register", userContent);
        registrationResponse.EnsureSuccessStatusCode();
        
        var registrationResult = await registrationResponse.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(registrationResult, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        var userId = loginResponse!.User.Id;
        var token = loginResponse.AccessToken;

        // Set authorization header
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Wait for CDC pipeline to synchronize user from PostgreSQL to MongoDB (elegant polling)
        var userSyncSuccess = await _mongoHelper.WaitForUserExistsAsync(userId, TimeSpan.FromSeconds(10));
        userSyncSuccess.Should().BeTrue("User should be synchronized to MongoDB via CDC");

        // Act 1 - Verify profile is accessible before deletion
        var profileResponse = await _client.GetAsync("/api/profile");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act 2 - Delete the user (logical deletion) using the proper command
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        await mediator.Send(new HexagonalSkeleton.Application.Features.UserManagement.Commands.SoftDeleteUserManagementCommand(userId));

        // Wait for CDC to synchronize the deletion (elegant polling)
        var deletionSyncSuccess = await _mongoHelper.WaitForUserDeletedAsync(userId, TimeSpan.FromSeconds(10));
        deletionSyncSuccess.Should().BeTrue("User deletion should be synchronized to MongoDB via CDC");

        // Act 3 - Try to access profile after deletion
        var deletedProfileResponse = await _client.GetAsync("/api/profile");

        // Assert - Should return 401 Unauthorized because the authentication system validates user existence in real-time
        // When a user is logically deleted, they are filtered out by the read repository,
        // causing the JWT validation to fail as the user is no longer found in the system
        deletedProfileResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyProfile_WhenUserIsActive_ShouldReturn200()
    {
        // Arrange - Create user and get auth token
        var uniqueEmail = $"profile.active.test.{Guid.NewGuid():N}@example.com";
        var uniquePhone = $"+1{DateTime.UtcNow.Ticks % 9000000000 + 1000000000}";
        
        var createUserRequest = new CreateUserRequest
        {
            FirstName = "Test",
            LastName = "User Profile Active", 
            Email = uniqueEmail,
            PhoneNumber = uniquePhone,
            Password = "TestPassword123!",
            PasswordConfirmation = "TestPassword123!",
            Birthdate = new DateTime(1990, 1, 1),
            Latitude = 40.7128,
            Longitude = -74.0060,
            AboutMe = "Test user for profile active test"
        };

        var userJson = JsonSerializer.Serialize(createUserRequest);
        var userContent = new StringContent(userJson, Encoding.UTF8, "application/json");
        var registrationResponse = await _client.PostAsync("/api/auth/register", userContent);
        registrationResponse.EnsureSuccessStatusCode();
        
        var registrationResult = await registrationResponse.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(registrationResult, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        var token = loginResponse!.AccessToken;

        // Extract user ID from JWT for CDC synchronization monitoring
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "nameid")?.Value 
                         ?? jwtToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value
                         ?? jwtToken.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim))
        {
            var allClaims = string.Join(", ", jwtToken.Claims.Select(c => $"{c.Type}:{c.Value}"));
            throw new InvalidOperationException($"User ID claim not found in JWT. Available claims: {allClaims}");
        }
        
        var userId = Guid.Parse(userIdClaim);

        // Wait for CDC synchronization from PostgreSQL (write) to MongoDB (read)
        await WaitForCdcSynchronization(userId);

        // Set authorization header
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - Get profile
        var profileResponse = await _client.GetAsync("/api/profile");

        // Assert - Should return 200 OK with user data
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var profileContent = await profileResponse.Content.ReadAsStringAsync();
        var profileData = JsonSerializer.Deserialize<UserResponse>(profileContent, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        profileData.Should().NotBeNull();
        profileData!.Email.Should().Be(createUserRequest.Email);
        profileData.FirstName.Should().Be(createUserRequest.FirstName);
        profileData.LastName.Should().Be(createUserRequest.LastName);
        profileData.PhoneNumber.Should().Be(createUserRequest.PhoneNumber);
    }

        /// <summary>
        /// Waits for CDC to synchronize user from PostgreSQL to MongoDB using elegant polling
        /// </summary>
        private async Task WaitForCdcSynchronization(Guid userId)
        {
            using var scope = _factory.Services.CreateScope();
            var mongoHelper = scope.ServiceProvider.GetRequiredService<MongoDbSyncHelper>();
            
            var success = await mongoHelper.WaitForUserExistsAsync(userId);
            if (!success)
            {
                throw new TimeoutException(
                    $"CDC synchronization did not complete within expected time for user {userId}. " +
                    $"Check that PostgreSQL WAL level is 'logical' and Debezium connector is properly configured.");
            }
        }
}
