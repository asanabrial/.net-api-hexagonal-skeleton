using AutoFixture.Xunit2;
using FluentAssertions;
using HexagonalSkeleton.API.Data;
using HexagonalSkeleton.API.Features.User.Application.Command;
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
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(s => s.Users.SoftDeleteUser(userId)).Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(s => s.SaveChangesAsync(cts.Token)).ReturnsAsync(true);

            Mock<SoftDeleteUserCommandHandler> hardDeleteUserCommandHandlerMock = new(
                new SoftDeleteUserCommandValidator(),
                unitOfWorkMock.Object);

            // Act
            var resultResponse = await hardDeleteUserCommandHandlerMock.Object.Handle(new SoftDeleteUserCommand(userId), cts.Token);
            var result = resultResponse as Ok<bool>;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Value.Should().BeTrue();
        }
    }
}
