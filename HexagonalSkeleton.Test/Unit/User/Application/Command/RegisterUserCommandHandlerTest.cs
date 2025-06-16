using AutoFixture.Xunit2;
using FluentAssertions;
using HexagonalSkeleton.Application.Command;
using HexagonalSkeleton.Application.Event;
using HexagonalSkeleton.Domain.Ports;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
            var userWriteRepositoryMock = new Mock<IUserWriteRepository>();
            var authenticationServiceMock = new Mock<IAuthenticationService>();
            var user = new RegisterUserCommand(
                "test@test.com",
                "Pa$$w0rd",
                "Pa$$w0rd",
                "Test",
                "Test",
                DateTime.Now,
                "123456789",
                0,
                0,
                "Test");
            userWriteRepositoryMock.Setup(s => s.CreateUserAsync(It.IsAny<Domain.User>(), cts.Token)).Returns(Task.CompletedTask);
            authenticationServiceMock.Setup(s => s.GenerateJwtToken(It.IsAny<Domain.User>())).Returns("fake-token");

            var registerUserCommandHandler = new RegisterUserCommandHandler(
                new RegisterUserCommandValidator(),
                mediator.Object,
                userWriteRepositoryMock.Object,
                authenticationServiceMock.Object,
                fixture.Settings);

            // Act
            var resultResponse =
                await registerUserCommandHandler.Handle(
                    user,
                    cts.Token);
            var result = resultResponse as Ok<RegisterUserCommandResult>;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Value.Should().NotBeNull();
            result.Value!.AccessToken.Should().NotBeNull();
            result.Value.AccessToken.Should().Be("fake-token");
            mediator.Verify(x => x.Publish(It.IsAny<LoginEvent>(), cts.Token));
        }
    }
}
