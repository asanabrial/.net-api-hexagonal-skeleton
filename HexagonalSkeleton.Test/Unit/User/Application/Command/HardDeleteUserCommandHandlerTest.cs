using AutoFixture.Xunit2;
using FluentAssertions;
using HexagonalSkeleton.API.Data;
using HexagonalSkeleton.API.Features.User.Application.Command;
using HexagonalSkeleton.API.Features.User.Application.Query;
using HexagonalSkeleton.API.Features.User.Domain;
using HexagonalSkeleton.API.Features.User.Infrastructure;
using HexagonalSkeleton.Test.Integration.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexagonalSkeleton.Test.Unit.User.Application.Command
{
    public class HardDeleteUserCommandHandlerTest
    {

        [Theory, AutoData]
        public async Task HardDeleteUserCommand_Should_Return_Ok_When_User_Deleted(CancellationTokenSource cts)
        {
            // Arrange
            int userId = 1;
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(s => s.Users.HardDelete(userId)).Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(s => s.SaveChangesAsync(cts.Token)).ReturnsAsync(true);
            
            Mock<HardDeleteUserCommandHandler> hardDeleteUserCommandHandlerMock = new(
                new HardDeleteUserCommandValidator(),
                new Mock<ILogger<HardDeleteUserCommandHandler>>().Object,
                unitOfWorkMock.Object);

            // Act
            var resultResponse = await hardDeleteUserCommandHandlerMock.Object.Handle(new HardDeleteUserCommand(userId), cts.Token);
            var result = resultResponse as Ok<bool>;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);
            result!.Value.Should().BeTrue();
        }
    }
}
