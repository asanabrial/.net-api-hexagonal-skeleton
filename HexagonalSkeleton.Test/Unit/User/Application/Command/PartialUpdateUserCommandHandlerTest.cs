using AutoFixture.Xunit2;
using FluentAssertions;
using HexagonalSkeleton.Application.Command;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.Ports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace HexagonalSkeleton.Test.Unit.User.Application.Command
{
    public class PartialUpdateUserCommandHandlerTest
    {

        [Theory, AutoData]
        public async Task PartialUpdateUserCommand_Should_Return_Ok_When_User_Updated(CancellationTokenSource cts)
        {
            // Arrange
            const int userId = 1;
            var userReadRepositoryMock = new Mock<IUserReadRepository>();
            var userWriteRepositoryMock = new Mock<IUserWriteRepository>();
            var user = new User { Id = userId };
            userReadRepositoryMock.Setup(s => s.GetUserByIdAsync(userId, cts.Token)).ReturnsAsync(user);
            userWriteRepositoryMock.Setup(s => s.UpdateUser(user)).Returns(Task.CompletedTask);

            var partialUpdateUserCommandHandler = new UpdateProfileUserCommandHandler(
                new UpdateProfileUserCommandValidator(),
                userReadRepositoryMock.Object,
                userWriteRepositoryMock.Object);

            // Act
            var resultResponse =
                await partialUpdateUserCommandHandler.Handle(
                    new UpdateProfileUserCommand(
                        userId,
                        "test@test.com",
                        "Test",
                        "Test",
                        DateTime.Now),
                    cts.Token);
            var result = resultResponse as Ok<bool>;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Value.Should().BeTrue();
        }
    }
}
