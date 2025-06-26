using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using HexagonalSkeleton.Application.Events;
using HexagonalSkeleton.Domain.Ports;

namespace HexagonalSkeleton.Test.Application.Events;

public class LoginEventHandlerTest
{
    private readonly Mock<ILogger<LoginEventHandler>> _mockLogger;
    private readonly Mock<IUserWriteRepository> _mockUserWriteRepository;
    private readonly LoginEventHandler _handler;

    public LoginEventHandlerTest()
    {
        _mockLogger = new Mock<ILogger<LoginEventHandler>>();
        _mockUserWriteRepository = new Mock<IUserWriteRepository>();
        _handler = new LoginEventHandler(_mockLogger.Object, _mockUserWriteRepository.Object);
    }

    [Fact]
    public async Task Handle_LoginEvent_ShouldUpdateLastLoginAndLog()
    {
        // Arrange
        var userId = 123;
        var loginEvent = new LoginEvent(userId);
        var cancellationToken = CancellationToken.None;

        _mockUserWriteRepository
            .Setup(r => r.SetLastLoginAsync(userId, cancellationToken))
            .Returns(Task.CompletedTask);        // Act
        await _handler.Handle(loginEvent, cancellationToken);

        // Assert
        _mockUserWriteRepository.Verify(r => r.SetLastLoginAsync(userId, cancellationToken), Times.Once);
        
        // Verify processing started log message
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Processing login event for user {userId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
            
        // Verify success log message
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Login event processed successfully for user {userId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_LoginEvent_ShouldHandleRepositoryException()
    {
        // Arrange
        var userId = 123;
        var loginEvent = new LoginEvent(userId);
        var cancellationToken = CancellationToken.None;

        _mockUserWriteRepository
            .Setup(r => r.SetLastLoginAsync(userId, cancellationToken))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(loginEvent, cancellationToken));

        _mockUserWriteRepository.Verify(r => r.SetLastLoginAsync(userId, cancellationToken), Times.Once);
    }
}
