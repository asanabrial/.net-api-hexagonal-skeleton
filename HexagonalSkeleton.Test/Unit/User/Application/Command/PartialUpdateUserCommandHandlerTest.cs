using AutoFixture.Xunit2;
using FluentAssertions;
using HexagonalSkeleton.API.Data;
using HexagonalSkeleton.API.Features.User.Application.Command;
using HexagonalSkeleton.API.Features.User.Application.Event;
using HexagonalSkeleton.API.Features.User.Application.Query;
using HexagonalSkeleton.API.Features.User.Domain;
using HexagonalSkeleton.Test.Integration.User;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexagonalSkeleton.Test.Unit.User.Application.Command
{
    public class PartialUpdateUserCommandHandlerTest
    {

        [Theory, AutoData]
        public async Task PartialUpdateUserCommand_Should_Return_Ok_When_User_Updated(CancellationTokenSource cts)
        {
            // Arrange
            int userId = 1;
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var user = new UserEntity();
            unitOfWorkMock.Setup(s => s.Users.FindOneAsync(userId, cts.Token)).ReturnsAsync(user);
            unitOfWorkMock.Setup(s => s.Users.Update(user)).Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(s => s.SaveChangesAsync(cts.Token)).ReturnsAsync(true);

            Mock<PartialUpdateUserCommandHandler> partialUpdateUserCommandHandlerMock = new(
                new PartialUpdateUserCommandValidator(),
                unitOfWorkMock.Object);

            // Act
            var resultResponse =
                await partialUpdateUserCommandHandlerMock.Object.Handle(
                    new PartialUpdateUserCommand(
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
            result!.Value.Should().BeTrue();
        }
    }
}
