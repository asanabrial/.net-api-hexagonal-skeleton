using Xunit;
using Moq;
using FluentValidation;
using MediatR;
using HexagonalSkeleton.Application.Command;
using HexagonalSkeleton.Application.Query;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain.Services;
using AutoMapper;

namespace HexagonalSkeleton.Test.Integration.User;

/// <summary>
/// Integration tests to verify the hexagonal architecture boundaries and flow
/// </summary>
public class UserWorkflowIntegrationTest
{
    [Fact]
    public async Task UserRegistrationAndRetrieval_EndToEndFlow_ShouldWorkCorrectly()
    {        // Arrange - Set up all the dependencies
        var mockValidator = new Mock<IValidator<RegisterUserCommand>>();
        var mockPublisher = new Mock<IPublisher>();
        var mockUserWriteRepository = new Mock<IUserWriteRepository>();
        var mockUserReadRepository = new Mock<IUserReadRepository>();
        var mockAuthenticationService = new Mock<IAuthenticationService>();
        var mockMapper = new Mock<IMapper>();

        var registerHandler = new RegisterUserCommandHandler(
            mockValidator.Object,
            mockPublisher.Object,
            mockUserWriteRepository.Object,
            mockUserReadRepository.Object,
            mockAuthenticationService.Object,
            mockMapper.Object);        var getUserValidator = new Mock<IValidator<GetUserQuery>>();
        var getUserHandler = new GetUserQueryHandler(
            getUserValidator.Object,
            mockUserReadRepository.Object,
            mockMapper.Object);

        // Test data
        var command = TestHelper.CreateRegisterUserCommand();
        var userId = 1;
        var salt = "salt123";
        var hash = "hash123";
        var jwtToken = "jwt-token-123";
        var cancellationToken = CancellationToken.None;

        // Set up mocks for registration
        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<RegisterUserCommand>(), cancellationToken))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        mockUserReadRepository
            .Setup(r => r.ExistsByEmailAsync(command.Email, cancellationToken))
            .ReturnsAsync(false);

        mockUserReadRepository
            .Setup(r => r.ExistsByPhoneNumberAsync(command.PhoneNumber, cancellationToken))
            .ReturnsAsync(false);

        mockAuthenticationService
            .Setup(a => a.GenerateSalt())
            .Returns(salt);

        mockAuthenticationService
            .Setup(a => a.HashPassword(command.Password, salt))
            .Returns(hash);

        mockUserWriteRepository
            .Setup(r => r.CreateAsync(It.IsAny<HexagonalSkeleton.Domain.User>(), cancellationToken))
            .ReturnsAsync(userId);        mockAuthenticationService
            .Setup(a => a.GenerateJwtTokenAsync(userId, cancellationToken))
            .ReturnsAsync(jwtToken);        // Setup AutoMapper mock to return a properly mapped result with nested structure
        mockMapper
            .Setup(m => m.Map<RegisterUserCommandResult>(It.IsAny<HexagonalSkeleton.Domain.User>()))
            .Returns((HexagonalSkeleton.Domain.User user) => new RegisterUserCommandResult(string.Empty)
            {
                User = new UserInfoResult
                {
                    Id = user.Id,
                    FirstName = user.FullName.FirstName,
                    LastName = user.FullName.LastName,
                    Email = user.Email.Value,
                    PhoneNumber = user.PhoneNumber?.Value,
                    Birthdate = user.Birthdate,
                    Latitude = user.Location?.Latitude,
                    Longitude = user.Location?.Longitude,
                    AboutMe = user.AboutMe,
                    CreatedAt = user.CreatedAt
                }
            });

        // Set up the user retrieval mock for the registration handler
        var createdUser = TestHelper.CreateTestUser(
            id: userId,
            email: command.Email,
            firstName: command.FirstName,
            lastName: command.LastName,
            phoneNumber: command.PhoneNumber);

        mockUserReadRepository
            .Setup(r => r.GetByIdAsync(userId, cancellationToken))
            .ReturnsAsync(createdUser);

        // Act - Execute the registration command
        var registrationResult = await registerHandler.Handle(command, cancellationToken);        // Assert - Verify registration was successful
        Assert.NotNull(registrationResult);
        Assert.NotNull(registrationResult.AccessToken);
        Assert.NotNull(registrationResult.User);
        Assert.Equal(command.Email, registrationResult.User.Email);
        Assert.Equal($"{command.FirstName} {command.LastName}", registrationResult.User.FullName);

        // Verify the domain service was used correctly
        mockUserWriteRepository.Verify(r => r.CreateAsync(It.Is<HexagonalSkeleton.Domain.User>(u => 
            u.Email.Value == command.Email.ToLowerInvariant() &&
            u.FullName.FirstName == command.FirstName &&
            u.FullName.LastName == command.LastName &&
            u.PhoneNumber.Value == command.PhoneNumber), cancellationToken), Times.Once);        // Now test retrieval of the created user
        getUserValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetUserQuery>(), cancellationToken))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        // Setup AutoMapper mock for GetUser operation
        mockMapper
            .Setup(m => m.Map<GetUserQueryResult>(It.IsAny<HexagonalSkeleton.Domain.User>()))
            .Returns((HexagonalSkeleton.Domain.User user) => new GetUserQueryResult
            {
                Id = user.Id,
                FirstName = user.FullName.FirstName,
                LastName = user.FullName.LastName,
                Birthdate = user.Birthdate,
                Email = user.Email.Value,
                LastLogin = user.LastLogin
            });

        var getUserQuery = new GetUserQuery(userId);
        var getUserResult = await getUserHandler.Handle(getUserQuery, cancellationToken);

        // Assert - Verify user retrieval was successful
        Assert.NotNull(getUserResult);
        Assert.Equal(userId, getUserResult.Id);
        Assert.Equal(command.Email.ToLowerInvariant(), getUserResult.Email);
        Assert.Equal(command.FirstName, getUserResult.FirstName);
        Assert.Equal(command.LastName, getUserResult.LastName);        // Verify all repository interactions
        mockUserReadRepository.Verify(r => r.ExistsByEmailAsync(command.Email, cancellationToken), Times.Once);
        mockUserReadRepository.Verify(r => r.ExistsByPhoneNumberAsync(command.PhoneNumber, cancellationToken), Times.Once);
        mockUserReadRepository.Verify(r => r.GetByIdAsync(userId, cancellationToken), Times.AtLeast(2)); // Called by both registration and retrieval
    }

    [Fact]
    public void UserDomainService_BusinessRules_ShouldEnforceCorrectly()
    {
        // This test verifies that domain services enforce business rules correctly
        // across different use cases

        // Test 1: Password strength validation
        Assert.True(UserDomainService.IsPasswordStrong("ValidPassword123!"));
        Assert.False(UserDomainService.IsPasswordStrong("weak"));

        // Test 2: Business email validation
        Assert.True(UserDomainService.IsValidBusinessEmail("user@example.com"));
        Assert.False(UserDomainService.IsValidBusinessEmail("user@tempmail.com"));

        // Test 3: User creation with business validation
        var user = UserDomainService.CreateUser(
            "test@example.com", "salt", "hash", "John", "Doe",
            DateTime.UtcNow.AddYears(-25), "+1234567890", 40.7128, -74.0060, "About me");

        Assert.NotNull(user);
        Assert.False(user.IsDeleted);
        Assert.True(UserDomainService.IsProfileComplete(user));

        // Test 4: User authorization rules
        var user1 = TestHelper.CreateTestUser(id: 1);
        var user2 = TestHelper.CreateTestUser(id: 2);

        Assert.True(UserDomainService.CanUserUpdateProfile(user1, user1));
        Assert.False(UserDomainService.CanUserUpdateProfile(user1, user2));

        // Test 5: User interaction rules        Assert.True(UserDomainService.CanUsersInteract(user1, user2));
        
        user1.Delete();
        Assert.False(UserDomainService.CanUsersInteract(user1, user2));
    }

    [Fact]
    public void ValueObjects_ShouldMaintainInvariants()
    {
        // Test that value objects maintain their invariants across operations

        // Email validation
        var email = new HexagonalSkeleton.Domain.ValueObjects.Email("Test@Example.COM");
        Assert.Equal("test@example.com", email.Value);

        // Phone number validation and formatting
        var phone = new HexagonalSkeleton.Domain.ValueObjects.PhoneNumber("+1 (555) 123-4567");
        Assert.Equal("+15551234567", phone.Value);

        // Full name validation and formatting
        var fullName = new HexagonalSkeleton.Domain.ValueObjects.FullName("  John  ", "  Doe  ");
        Assert.Equal("John", fullName.FirstName);
        Assert.Equal("Doe", fullName.LastName);
        Assert.Equal("John Doe", fullName.GetFullName());
        Assert.Equal("JD", fullName.GetInitials());
    }
}
