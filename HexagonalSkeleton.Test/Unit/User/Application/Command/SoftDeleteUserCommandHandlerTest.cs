using AutoFixture.Xunit2;
using FluentAssertions;
using HexagonalSkeleton.Application.Command;
using HexagonalSkeleton.Domain.Ports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace HexagonalSkeleton.Test.Unit.User.Application.Command
{
    public class SoftDeleteUserCommandHandlerTest
    {

        [Theory, AutoData]
        public async Task SoftDeleteUserCommand_Should_Return_Ok_When_User_Deleted(CancellationTokenSource cts)
        {
            // Arrange
            const int userId = 1;
            var userWriteRepositoryMock = new Mock<IUserWriteRepository>();
            userWriteRepositoryMock.Setup(s => s.SoftDeleteUser(userId)).Returns(Task.CompletedTask);

            var softDeleteUserCommandHandler = new SoftDeleteUserCommandHandler(
                new SoftDeleteUserCommandValidator(),
                userWriteRepositoryMock.Object);

            // Act
            var resultResponse = await softDeleteUserCommandHandler.Handle(new SoftDeleteUserCommand(userId), cts.Token);
            var result = resultResponse as Ok<bool>;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Value.Should().BeTrue();
            userWriteRepositoryMock.Verify(x => x.SoftDeleteUser(userId), Times.Once);
        }
    }
}
