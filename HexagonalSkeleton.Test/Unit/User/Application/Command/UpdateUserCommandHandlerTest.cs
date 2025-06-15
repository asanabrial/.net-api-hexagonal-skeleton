using AutoFixture.Xunit2;
using FluentAssertions;
using HexagonalSkeleton.API.Data;
using HexagonalSkeleton.Application.Command;
using HexagonalSkeleton.Domain;
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
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var user = new UserEntity(){Id = userId};
            unitOfWorkMock.Setup(s => s.Users.UpdateUser(user)).Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(s => s.SaveChangesAsync(cts.Token)).ReturnsAsync(true);

            Mock<UpdateUserCommandHandler> partialUpdateUserCommandHandlerMock = new(
                new UpdateUserCommandValidator(),
                unitOfWorkMock.Object);

            // Act
            var resultResponse =
                await partialUpdateUserCommandHandlerMock.Object.Handle(
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
        }
    }
}
