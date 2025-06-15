using System.Security.Claims;
using FluentAssertions;
using HexagonalSkeleton.Application.Command;
using HexagonalSkeleton.Application.Query;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HexagonalSkeleton.Test.Unit.User.Infrastructure
{
    public class UserControllerTest
    {
        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            Mock<IMediator> mediator = new();
            Mock<UserController> userController = new(mediator.Object);
            LoginQuery query = new("test@test.com", "Pa$$w0rd");
            var expectedResult = Results.Ok(new LoginQueryResult("token"));
            mediator.Setup(s => s.Send(query, It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            // Act
            var result = await userController.Object.Login(query) as Ok<LoginQueryResult>;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);
            mediator.Verify(s => s.Send(query, It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            Mock<IMediator> mediator = new();
            Mock<UserController> userController = new(mediator.Object);
            LoginQuery query = new("test@test.com", "Pa$$w0rd");
            var expectedResult = Results.Unauthorized();
            mediator.Setup(s => s.Send(query, It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            // Act
            var result = await userController.Object.Login(query) as UnauthorizedHttpResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            mediator.Verify(s => s.Send(query, It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsToken()
        {
            // Arrange
            Mock<IMediator> mediator = new();
            Mock<UserController> userController = new(mediator.Object);
            RegisterUserCommand command = new("test@test.com", "Pa$$w0rd", "Pa$$w0rd", "Test", "Test", DateTime.Now, "123456789", 0, 0, "Test");
            var expectedResult = Results.Ok(new RegisterUserCommandResult("token"));
            mediator.Setup(s => s.Send(command, It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            // Act
            var result = await userController.Object.Post(command) as Ok<RegisterUserCommandResult>;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);
            mediator.Verify(s => s.Send(command, It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task Register_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            Mock<IMediator> mediator = new();
            Mock<UserController> userController = new(mediator.Object);
            RegisterUserCommand command = new("test@test.com", "Pa$$w0rd", "Pa$$w0rd", "Test", "Test", DateTime.Now, "123456789", 0, 0, "Test");
            var expectedResult = Results.BadRequest();
            mediator.Setup(s => s.Send(command, It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            // Act
            var result = await userController.Object.Post(command) as BadRequest;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            mediator.Verify(s => s.Send(command, It.IsAny<CancellationToken>()));
        }
        [Fact]
        public async Task GetAllUsers_ReturnsAllUsers()
        {
            // Arrange
            Mock<IMediator> mediator = new();
            Mock<UserController> userController = new(mediator.Object);
            var expectedResult = Results.Ok(new List<GetAllUsersQueryResult>());
            mediator.Setup(s => s.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            // Act
            var resultResponse = await userController.Object.GetAll();
            var result = resultResponse as Ok<List<GetAllUsersQueryResult>>;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);
            mediator.Verify(s => s.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()));
        }
        [Fact]
        public async Task GetAllUsers_ReturnsUnauthorized()
        {
            // Arrange
            Mock<IMediator> mediator = new();
            Mock<UserController> userController = new(mediator.Object);
            var expectedResult = Results.Unauthorized();
            mediator.Setup(s => s.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            // Act
            var resultResponse = await userController.Object.GetAll();
            var result = resultResponse as UnauthorizedHttpResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            mediator.Verify(s => s.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task GetUser_ReturnsUser()
        {
            // Arrange
            Mock<IMediator> mediator = new();
            Mock<UserController> userController = new(mediator.Object);
            var expectedResult = Results.Ok(new GetUserQueryResult(new UserEntity()));

            mediator.Setup(s => s.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var resultResponse = await userController.Object.Get(1);
            var result = resultResponse as Ok<GetUserQueryResult>;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);
            mediator.Verify(s => s.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()));
        }
        [Fact]
        public async Task GetUser_ReturnsNotFound()
        {
            // Arrange
            Mock<IMediator> mediator = new();
            Mock<UserController> userController = new(mediator.Object);
            var expectedResult = Results.NotFound();

            mediator.Setup(s => s.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var resultResponse = await userController.Object.Get(1);
            var result = resultResponse as NotFound;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            mediator.Verify(s => s.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()));
        }
        [Fact]
        public async Task GetUser_ReturnsUnauthorized()
        {
            // Arrange
            Mock<IMediator> mediator = new();
            Mock<UserController> userController = new(mediator.Object);
            var expectedResult = Results.Unauthorized();

            mediator.Setup(s => s.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var resultResponse = await userController.Object.Get(1);
            var result = resultResponse as UnauthorizedHttpResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            mediator.Verify(s => s.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()));
        }
        [Fact]
        public async Task HardDeleteUser_ReturnsOk()
        {
            // Arrange
            Mock<IMediator> mediator = new();
            Mock<UserController> userController = new(mediator.Object);
            var expectedResult = Results.Ok(true);

            mediator.Setup(s => s.Send(It.IsAny<HardDeleteUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var resultResponse = await userController.Object.HardDelete(1);
            var result = resultResponse as Ok<bool>;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);
            mediator.Verify(s => s.Send(It.IsAny<HardDeleteUserCommand>(), It.IsAny<CancellationToken>()));
        }
        [Fact]
        public async Task HardDeleteUser_ReturnsNotFound()
        {
            // Arrange
            Mock<IMediator> mediator = new();
            Mock<UserController> userController = new(mediator.Object);
            var expectedResult = Results.NotFound();

            mediator.Setup(s => s.Send(It.IsAny<HardDeleteUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var resultResponse = await userController.Object.HardDelete(1);
            var result = resultResponse as NotFound;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            mediator.Verify(s => s.Send(It.IsAny<HardDeleteUserCommand>(), It.IsAny<CancellationToken>()));
        }
        [Fact]
        public async Task HardDeleteUser_ReturnsUnauthorized()
        {
            // Arrange
            Mock<IMediator> mediator = new();
            Mock<UserController> userController = new(mediator.Object);
            var expectedResult = Results.Unauthorized();

            mediator.Setup(s => s.Send(It.IsAny<HardDeleteUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var resultResponse = await userController.Object.HardDelete(1);
            var result = resultResponse as UnauthorizedHttpResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            mediator.Verify(s => s.Send(It.IsAny<HardDeleteUserCommand>(), It.IsAny<CancellationToken>()));
        }
        [Fact]
        public async Task SoftDeleteUser_ReturnsOk()
        {
            // Arrange
            Mock<IMediator> mediator = new();
            Mock<UserController> userController = new(mediator.Object);
            var expectedResult = Results.Ok(true);

            mediator.Setup(s => s.Send(It.IsAny<SoftDeleteUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var resultResponse = await userController.Object.SoftDelete(1);
            var result = resultResponse as Ok<bool>;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);
            mediator.Verify(s => s.Send(It.IsAny<SoftDeleteUserCommand>(), It.IsAny<CancellationToken>()));
        }
        [Fact]
        public async Task SoftDeleteUser_ReturnsNotFound()
        {
            // Arrange
            Mock<IMediator> mediator = new();
            Mock<UserController> userController = new(mediator.Object);
            var expectedResult = Results.NotFound();

            mediator.Setup(s => s.Send(It.IsAny<SoftDeleteUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var resultResponse = await userController.Object.SoftDelete(1);
            var result = resultResponse as NotFound;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            mediator.Verify(s => s.Send(It.IsAny<SoftDeleteUserCommand>(), It.IsAny<CancellationToken>()));
        }
        [Fact]
        public async Task SoftDeleteUser_ReturnsUnauthorized()
        {
            // Arrange
            Mock<IMediator> mediator = new();
            Mock<UserController> userController = new(mediator.Object);
            var expectedResult = Results.Unauthorized();

            mediator.Setup(s => s.Send(It.IsAny<SoftDeleteUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var resultResponse = await userController.Object.SoftDelete(1);
            var result = resultResponse as UnauthorizedHttpResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            mediator.Verify(s => s.Send(It.IsAny<SoftDeleteUserCommand>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task UpdateUser_ReturnsOk()
        {
            // Arrange
            Mock<IMediator> mediator = new();
            Mock<UserController> userController = new(mediator.Object);
            UpdateUserCommand command = new()
            {
                Birthdate = DateTime.Now,
                Email = "",
                Name = "",
                Surname = "",
                Id = 1,
                Password = "",
                PasswordSalt = "",
                PasswordHash = "",
                LastLogin = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsDeleted = false,
                DeletedAt = DateTime.Now
            };
            var expectedResult = Results.Ok(true);

            mediator.Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var resultResponse = await userController.Object.Put(command) as Ok<bool>;

            // Assert
            resultResponse.Should().NotBeNull();
            resultResponse!.StatusCode.Should().Be(StatusCodes.Status200OK);
            mediator.Verify(s => s.Send(command, It.IsAny<CancellationToken>()));
        }
        [Fact]
        public async Task UpdateUser_ReturnsNotFound()
        {
            // Arrange
            Mock<IMediator> mediator = new();
            Mock<UserController> userController = new(mediator.Object);
            UpdateUserCommand command = new()
            {
                Birthdate = DateTime.Now,
                Email = "",
                Name = "",
                Surname = "",
                Id = 1,
                Password = "",
                PasswordSalt = "",
                PasswordHash = "",
                LastLogin = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsDeleted = false,
                DeletedAt = DateTime.Now
            };
            var expectedResult = Results.NotFound();

            mediator.Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var resultResponse = await userController.Object.Put(command) as NotFound;

            // Assert
            resultResponse.Should().NotBeNull();
            resultResponse!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            mediator.Verify(s => s.Send(command, It.IsAny<CancellationToken>()));
        }
        [Fact]
        public async Task UpdateUser_ReturnsUnauthorized()
        {
            // Arrange
            Mock<IMediator> mediator = new();
            Mock<UserController> userController = new(mediator.Object);
            UpdateUserCommand command = new()
            {
                Birthdate = DateTime.Now,
                Email = "",
                Name = "",
                Surname = "",
                Id = 1,
                Password = "",
                PasswordSalt = "",
                PasswordHash = "",
                LastLogin = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsDeleted = false,
                DeletedAt = DateTime.Now
            };
            var expectedResult = Results.Unauthorized();

            mediator.Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var resultResponse = await userController.Object.Put(command) as UnauthorizedHttpResult;

            // Assert
            resultResponse.Should().NotBeNull();
            resultResponse!.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            mediator.Verify(s => s.Send(command, It.IsAny<CancellationToken>()));
        }
        [Fact]
        public async Task UpdateUser_ReturnsBadRequest()
        {
            // Arrange
            Mock<IMediator> mediator = new();
            Mock<UserController> userController = new(mediator.Object);
            UpdateUserCommand command = new()
            {
                Birthdate = DateTime.Now,
                Email = "",
                Name = "",
                Surname = "",
                Id = 1,
                Password = "",
                PasswordSalt = "",
                PasswordHash = "",
                LastLogin = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsDeleted = false,
                DeletedAt = DateTime.Now
            };
            var expectedResult = Results.BadRequest();

            mediator.Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var resultResponse = await userController.Object.Put(command) as BadRequest;

            // Assert
            resultResponse.Should().NotBeNull();
            resultResponse!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            mediator.Verify(s => s.Send(command, It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task PartialUpdate_Returns_Ok()
        {

            // Arrange
            Mock<IMediator> mediator = new();
            Mock<UserController> userController = new(mediator.Object);
            UpdateProfileUserCommand command = new(1, "", "", "", DateTime.Now);
            var expectedResult = Results.Ok(true);

            mediator.Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var resultResponse = await userController.Object.Patch(command) as Ok<bool>;

            // Assert
            resultResponse.Should().NotBeNull();
            resultResponse!.StatusCode.Should().Be(StatusCodes.Status200OK);
            mediator.Verify(s => s.Send(command, It.IsAny<CancellationToken>()));
        }
        [Fact]
        public async Task PartialUpdate_Returns_NotFound()
        {

            // Arrange
            Mock<IMediator> mediator = new();
            Mock<UserController> userController = new(mediator.Object);
            UpdateProfileUserCommand command = new(1, "", "", "", DateTime.Now);
            var expectedResult = Results.NotFound();

            mediator.Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var resultResponse = await userController.Object.Patch(command) as NotFound;

            // Assert
            resultResponse.Should().NotBeNull();
            resultResponse!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            mediator.Verify(s => s.Send(command, It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task PartialUpdate_Returns_Unauthorized()
        {

            // Arrange
            Mock<IMediator> mediator = new();
            Mock<UserController> userController = new(mediator.Object);
            UpdateProfileUserCommand command = new(1, "", "", "", DateTime.Now);
            var expectedResult = Results.Unauthorized();

            mediator.Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var resultResponse = await userController.Object.Patch(command) as UnauthorizedHttpResult;

            // Assert
            resultResponse.Should().NotBeNull();
            resultResponse!.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            mediator.Verify(s => s.Send(command, It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task Me_Returns_Ok()
        {

            // Arrange
            Mock<ISender> mediator = new();
            Mock<UserController> userController = new (mediator.Object)
            {
                Object =
                {
                    ControllerContext = new ControllerContext()
                    {
                        HttpContext = new DefaultHttpContext
                        {
                            User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()
                            {
                                new (ClaimTypes.NameIdentifier, "1"),
                            }))

                        }
                    }
                }
            };

            var expectedResult = Results.Ok(new GetUserQueryResult(new UserEntity()));

            mediator.Setup(s => s.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var resultResponse = await userController.Object.Get();
            var result = resultResponse as Ok<GetUserQueryResult>;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);
            mediator.Verify(s => s.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()));
        }
    }
}
