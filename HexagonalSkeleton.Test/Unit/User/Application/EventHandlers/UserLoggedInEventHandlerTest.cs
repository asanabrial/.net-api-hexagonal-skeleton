using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using HexagonalSkeleton.Application.EventHandlers;
using HexagonalSkeleton.Domain.Events;

namespace HexagonalSkeleton.Test.Unit.User.Application.EventHandlers;

public class UserLoggedInEventHandlerTest
{
    private readonly Mock<ILogger<UserLoggedInEventHandler>> _mockLogger;
    private readonly UserLoggedInEventHandler _handler;

    public UserLoggedInEventHandlerTest()
    {
        _mockLogger = new Mock<ILogger<UserLoggedInEventHandler>>();
        _handler = new UserLoggedInEventHandler(_mockLogger.Object);
    }

    [Fact]
    public async Task Handle_UserLoggedInEvent_ShouldLogLogin()
    {
        // Arrange
        var loginTime = DateTime.UtcNow;
        var userLoggedInEvent = new UserLoggedInEvent(
            userId: 123,
            email: "test@example.com",
            loginTime: loginTime);
        var cancellationToken = CancellationToken.None;

        // Act
        await _handler.Handle(userLoggedInEvent, cancellationToken);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User logged in") && 
                                              v.ToString()!.Contains("123") &&
                                              v.ToString()!.Contains("test@example.com")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UserLoggedInEvent_ShouldComplete()
    {
        // Arrange
        var loginTime = DateTime.UtcNow;
        var userLoggedInEvent = new UserLoggedInEvent(
            userId: 123,
            email: "test@example.com",
            loginTime: loginTime);
        var cancellationToken = CancellationToken.None;

        // Act & Assert - Should not throw
        await _handler.Handle(userLoggedInEvent, cancellationToken);
    }
}
