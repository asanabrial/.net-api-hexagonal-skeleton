using AutoFixture.Xunit2;
using FluentAssertions;
using HexagonalSkeleton.API.Features.User.Application.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using MediatR;
using AutoFixture;
using HexagonalSkeleton.API.Data;
using HexagonalSkeleton.API.Features.User.Application.Event;
using HexagonalSkeleton.API.Features.User.Domain;
using HexagonalSkeleton.CommonCore.Auth;

namespace HexagonalSkeleton.Test.Unit.User.Application.Query
{
    public class LoginHandlerTest(UnitTestFixture fixture) : IClassFixture<UnitTestFixture>
    {
        [Theory, AutoData]
        public async Task Login_Should_Return_All_Entities_Without_Deleted_Ones(CancellationTokenSource cts)
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var user = fixture.Fixture.Create<UserEntity>();
            var passwordRaw = Guid.NewGuid().ToString();
            user.PasswordSalt = PasswordHasher.GenerateSalt();
            user.PasswordHash = PasswordHasher.ComputeHash(passwordRaw, user.PasswordSalt, fixture.Settings.Value.Pepper);

            unitOfWorkMock.Setup(s => s.Users.GetByEmailAsync(user.Email!, cts.Token)).ReturnsAsync(user);

            var validatorLoginQuery = new LoginQueryValidator();
            var getUserQueryHandlerMock = new Mock<LoginQueryHandler>(
                validatorLoginQuery,
                mediator.Object,
                unitOfWorkMock.Object,
                fixture.Settings);

            // Act
            var resultResponse = await getUserQueryHandlerMock.Object.Handle(new LoginQuery(user.Email!, passwordRaw), cts.Token);
            var result = resultResponse as Ok<LoginQueryResult>;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Value.Should().NotBeNull();
            result.Value!.AccessToken.Should().NotBeNull();
            fixture.ValidateToken(result.Value.AccessToken!).Should().BeTrue();
            mediator.Verify(x => x.Publish(It.IsAny<LoginEvent>(), cts.Token));
        }

        [Theory, AutoData]
        public async Task Login_Should_Return_NotFound_When_User_Not_Found(CancellationTokenSource cts)
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            UserEntity? user = null;
            var email = Guid.NewGuid().ToString();
            var password = Guid.NewGuid().ToString();
            unitOfWorkMock.Setup(s => s.Users.GetByEmailAsync(email, cts.Token)).ReturnsAsync(user);

            var validatorLoginQuery = new LoginQueryValidator();
            var getUserQueryHandlerMock = new Mock<LoginQueryHandler>(
                validatorLoginQuery,
                mediator.Object,
                unitOfWorkMock.Object,
                fixture.Settings);

            // Act
            var resultResponse =
                await getUserQueryHandlerMock.Object.Handle(new LoginQuery(email, password), cts.Token);
            
            var result = resultResponse as UnauthorizedHttpResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }
        [Theory, AutoData]
        public async Task Login_Should_Return_Validation_Error_When_Email_Is_Empty(CancellationTokenSource cts)
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var getUserQueryHandlerMock = new Mock<LoginQueryHandler>(
                new LoginQueryValidator(),
                mediator.Object,
                unitOfWorkMock.Object,
                fixture.Settings);

            // Act
            var resultResponse = await getUserQueryHandlerMock.Object.Handle(new LoginQuery(string.Empty, "password"), cts.Token);
            var result = resultResponse as ProblemHttpResult;
            var problemDetails = result!.ProblemDetails as HttpValidationProblemDetails;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            problemDetails!.Errors.Should().HaveCount(1);
            problemDetails.Errors.Should().ContainKey("Email");
        }
        [Theory, AutoData]
        public async Task Login_Should_Return_Validation_Error_When_Password_Is_Empty(CancellationTokenSource cts)
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var getUserQueryHandlerMock = new Mock<LoginQueryHandler>(
                new LoginQueryValidator(),
                mediator.Object,
                unitOfWorkMock.Object,
                fixture.Settings);

            // Act
            var resultResponse = await getUserQueryHandlerMock.Object.Handle(new LoginQuery("email", string.Empty), cts.Token);
            var result = resultResponse as ProblemHttpResult;
            var problemDetails = result!.ProblemDetails as HttpValidationProblemDetails;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            problemDetails!.Errors.Should().HaveCount(1);
            problemDetails.Errors.Should().ContainKey("Password");
        }

    }
}
