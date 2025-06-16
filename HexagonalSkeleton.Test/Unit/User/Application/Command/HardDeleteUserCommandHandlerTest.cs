using AutoFixture.Xunit2;
using FluentAssertions;
using HexagonalSkeleton.Application.Command;
using HexagonalSkeleton.Domain.Ports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace HexagonalSkeleton.Test.Unit.User.Application.Command
{
    public class HardDeleteUserCommandHandlerTest
    {
        [Theory, AutoData]
        public async Task HardDeleteUserCommand_Should_Return_Ok_When_User_Deleted(CancellationTokenSource cts)
        {
            // Arrange
            const int userId = 1;
            var userWriteRepositoryMock = new Mock<IUserWriteRepository>();
            userWriteRepositoryMock.Setup(s => s.HardDeleteUser(userId)).Returns(Task.CompletedTask);
            
            var hardDeleteUserCommandHandler = new HardDeleteUserCommandHandler(
                new HardDeleteUserCommandValidator(),
                userWriteRepositoryMock.Object);

            // Act
            var resultResponse = await hardDeleteUserCommandHandler.Handle(new HardDeleteUserCommand(userId), cts.Token);
            var result = resultResponse as Ok<bool>;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Value.Should().BeTrue();
            userWriteRepositoryMock.Verify(x => x.HardDeleteUser(userId), Times.Once);
        }
    }
}