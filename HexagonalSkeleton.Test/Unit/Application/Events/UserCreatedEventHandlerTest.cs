using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using HexagonalSkeleton.Domain.Events;
using HexagonalSkeleton.Application.Events;

namespace HexagonalSkeleton.Test.Application.Events;

public class UserCreatedEventHandlerTest
{
    private readonly Mock<ILogger<UserCreatedEventHandler>> _mockLogger;
    private readonly UserCreatedEventHandler _handler;

    public UserCreatedEventHandlerTest()
    {
        _mockLogger = new Mock<ILogger<UserCreatedEventHandler>>();
        _handler = new UserCreatedEventHandler(_mockLogger.Object);
    }

    [Fact]
    public async Task Handle_UserCreatedEvent_ShouldLogCreation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userCreatedEvent = new UserCreatedEvent(
            userId: userId,
            email: "test@example.com",
            name: "John",
            surname: "Doe",
            phoneNumber: "+1234567890");
        var cancellationToken = CancellationToken.None;

        // Act
        await _handler.Handle(userCreatedEvent, cancellationToken);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User created") && 
                                              v.ToString()!.Contains(userId.ToString()) &&
                                              v.ToString()!.Contains("test@example.com") &&
                                              v.ToString()!.Contains("John") &&
                                              v.ToString()!.Contains("Doe")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UserCreatedEvent_ShouldComplete()
    {
        // Arrange
        var userCreatedEvent = new UserCreatedEvent(
            userId: Guid.NewGuid(),
            email: "test@example.com",
            name: "John",
            surname: "Doe",
            phoneNumber: "+1234567890");
        var cancellationToken = CancellationToken.None;

        // Act & Assert - Should not throw
        await _handler.Handle(userCreatedEvent, cancellationToken);
    }
}
