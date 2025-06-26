using Xunit;
using HexagonalSkeleton.Domain.Events;

namespace HexagonalSkeleton.Test.Unit.User.Domain.Events;

public class DomainEventsTest
{
    [Fact]
    public void UserCreatedEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var userId = 123;
        var email = "test@example.com";
        var name = "John";
        var surname = "Doe";
        var phoneNumber = "+1234567890";

        // Act
        var userCreatedEvent = new UserCreatedEvent(userId, email, name, surname, phoneNumber);

        // Assert
        Assert.Equal(userId, userCreatedEvent.UserId);
        Assert.Equal(email, userCreatedEvent.Email);
        Assert.Equal(name, userCreatedEvent.Name);
        Assert.Equal(surname, userCreatedEvent.Surname);
        Assert.Equal(phoneNumber, userCreatedEvent.PhoneNumber);
        Assert.True(userCreatedEvent.CreatedAt <= DateTime.UtcNow);
        Assert.True(userCreatedEvent.DateOccurred <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void UserCreatedEvent_NullEmail_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new UserCreatedEvent(123, null!, "John", "Doe", "+1234567890"));
    }

    [Fact]
    public void UserCreatedEvent_NullName_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new UserCreatedEvent(123, "test@example.com", null!, "Doe", "+1234567890"));
    }

    [Fact]
    public void UserCreatedEvent_NullSurname_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new UserCreatedEvent(123, "test@example.com", "John", null!, "+1234567890"));
    }

    [Fact]
    public void UserProfileUpdatedEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var userId = 123;
        var email = "test@example.com";
        var previousName = "John";
        var newName = "Jane";

        // Act
        var profileUpdatedEvent = new UserProfileUpdatedEvent(userId, email, previousName, newName);

        // Assert
        Assert.Equal(userId, profileUpdatedEvent.UserId);
        Assert.Equal(email, profileUpdatedEvent.Email);
        Assert.Equal(previousName, profileUpdatedEvent.PreviousName);
        Assert.Equal(newName, profileUpdatedEvent.NewName);
        Assert.True(profileUpdatedEvent.DateOccurred <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void UserLoggedInEvent_ShouldInitializeCorrectly()
    {
        // Arrange
        var userId = 123;
        var email = "test@example.com";
        var loginTime = DateTime.UtcNow;

        // Act
        var loggedInEvent = new UserLoggedInEvent(userId, email, loginTime);

        // Assert
        Assert.Equal(userId, loggedInEvent.UserId);
        Assert.Equal(email, loggedInEvent.Email);
        Assert.Equal(loginTime, loggedInEvent.LoginTime);
        Assert.True(loggedInEvent.DateOccurred <= DateTimeOffset.UtcNow);
    }
}
