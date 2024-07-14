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
    public class RegisterUserCommandHandlerTest(UnitTestFixture fixture) : IClassFixture<UnitTestFixture>
    {

        [Theory, AutoData]
        public async Task RegisterUserCommand_Should_Return_Ok_When_User_Created(CancellationTokenSource cts)
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var user = new RegisterUserCommand(
                "test@test.com",
                "Pa$$w0rd",
                "Pa$$w0rd",
                "Test",
                "Test",
                DateTime.Now);
            unitOfWorkMock.Setup(s => s.Users.CreateAsync(user.ToDomainEntity(), cts.Token)).Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(s => s.SaveChangesAsync(cts.Token)).ReturnsAsync(true);

            Mock<RegisterUserCommandHandler> registerUserCommandHandlerMock = new(
                new RegisterUserCommandValidator(),
                mediator.Object,
                unitOfWorkMock.Object,
                fixture.Settings);

            // Act
            var resultResponse =
                await registerUserCommandHandlerMock.Object.Handle(
                    user,
                    cts.Token);
            var result = resultResponse as Ok<RegisterUserCommandResult>;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);
            result!.Value.Should().NotBeNull();
            result!.Value!.AccessToken.Should().NotBeNull();
            fixture.ValidateToken(result.Value.AccessToken!).Should().BeTrue();
            mediator.Verify(x => x.Publish(It.IsAny<LoginEvent>(), cts.Token));
        }
    }
}
