using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using HexagonalSkeleton.Domain.Events;
using HexagonalSkeleton.Application.Events;

namespace HexagonalSkeleton.Test.Application.Events;

public class UserProfileUpdatedEventHandlerTest
{
    private readonly Mock<ILogger<UserProfileUpdatedEventHandler>> _mockLogger;
    private readonly UserProfileUpdatedEventHandler _handler;

    public UserProfileUpdatedEventHandlerTest()
    {
        _mockLogger = new Mock<ILogger<UserProfileUpdatedEventHandler>>();
        _handler = new UserProfileUpdatedEventHandler(_mockLogger.Object);
    }

    [Fact]
    public async Task Handle_UserProfileUpdatedEvent_ShouldLogUpdate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userProfileUpdatedEvent = new UserProfileUpdatedEvent(
            userId: userId,
            email: "test@example.com",
            previousName: "John",
            newName: "Jane");
        var cancellationToken = CancellationToken.None;

        // Act
        await _handler.Handle(userProfileUpdatedEvent, cancellationToken);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User profile updated") && 
                                              v.ToString()!.Contains(userId.ToString()) &&
                                              v.ToString()!.Contains("test@example.com") &&
                                              v.ToString()!.Contains("John") &&
                                              v.ToString()!.Contains("Jane")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UserProfileUpdatedEvent_ShouldComplete()
    {
        // Arrange
        var userProfileUpdatedEvent = new UserProfileUpdatedEvent(
            userId: Guid.NewGuid(),
            email: "test@example.com",
            previousName: "John",
            newName: "Jane");
        var cancellationToken = CancellationToken.None;

        // Act & Assert - Should not throw
        await _handler.Handle(userProfileUpdatedEvent, cancellationToken);
    }
}
