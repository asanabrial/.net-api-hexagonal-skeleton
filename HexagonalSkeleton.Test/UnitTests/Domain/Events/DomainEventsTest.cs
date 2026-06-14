using Xunit;
using HexagonalSkeleton.Domain.Events;

namespace HexagonalSkeleton.Test.Unit.User.Domain.Events;

public class DomainEventsTest
{
    [Fact]
    public void UserCreatedEvent_NullEmail_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new UserCreatedEvent(Guid.NewGuid(), null!, "John", "Doe", "+1234567890"));
    }

    [Fact]
    public void UserCreatedEvent_NullName_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new UserCreatedEvent(Guid.NewGuid(), "test@example.com", null!, "Doe", "+1234567890"));
    }

    [Fact]
    public void UserCreatedEvent_NullSurname_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new UserCreatedEvent(Guid.NewGuid(), "test@example.com", "John", null!, "+1234567890"));
    }
}
