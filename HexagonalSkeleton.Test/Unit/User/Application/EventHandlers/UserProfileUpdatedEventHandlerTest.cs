using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using HexagonalSkeleton.Application.EventHandlers;
using HexagonalSkeleton.Domain.Events;

namespace HexagonalSkeleton.Test.Unit.User.Application.EventHandlers;

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
        var userProfileUpdatedEvent = new UserProfileUpdatedEvent(
            userId: 123,
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
                                              v.ToString()!.Contains("123") &&
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
            userId: 123,
            email: "test@example.com",
            previousName: "John",
            newName: "Jane");
        var cancellationToken = CancellationToken.None;

        // Act & Assert - Should not throw
        await _handler.Handle(userProfileUpdatedEvent, cancellationToken);
    }
}
