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
    public class UpdateUserCommandHandlerTest
    {

        [Theory, AutoData]
        public async Task UpdateUserCommand_Should_Return_Ok_When_User_Updated(CancellationTokenSource cts)
        {
            // Arrange
            const int userId = 1;
            var userWriteRepositoryMock = new Mock<IUserWriteRepository>();
            var user = new User { Id = userId };
            userWriteRepositoryMock.Setup(s => s.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);

            var updateUserCommandHandler = new UpdateUserCommandHandler(
                new UpdateUserCommandValidator(),
                userWriteRepositoryMock.Object);

            // Act
            var resultResponse =
                await updateUserCommandHandler.Handle(
                    new UpdateUserCommand()
                    {
                        Id = userId,
                        Email = "test@test.com",
                        Name = "Test",
                        Surname = "Test",
                        Birthdate = DateTime.Now,
                        Password = "Pa$$w0rd",
                        PasswordHash = "Pa$$w0rd",
                        PasswordSalt = "Pa$$w0rd",
                        UpdatedAt = DateTime.Now,
                        IsDeleted = false,
                        CreatedAt = DateTime.Now,
                        LastLogin = DateTime.Now,
                        DeletedAt = DateTime.Now
                    },
                    cts.Token);
            var result = resultResponse as Ok<bool>;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Value.Should().BeTrue();
            userWriteRepositoryMock.Verify(x => x.UpdateUser(It.IsAny<User>()), Times.Once);
        }
    }
}
