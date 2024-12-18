﻿using AutoFixture.Xunit2;
using FluentAssertions;
using HexagonalSkeleton.API.Features.User.Application.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using AutoFixture;
using HexagonalSkeleton.API.Data;
using HexagonalSkeleton.API.Features.User.Domain;

namespace HexagonalSkeleton.Test.Unit.User.Application.Query
{
    public class GetUserQueryHandlerTest
    {
        [Theory, AutoData]
        public async Task GetUserQuery_Should_Return_All_Entities_Without_Deleted_Ones(CancellationTokenSource cts)
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));

            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            var user = fixture.Create<UserEntity>();
            unitOfWorkMock.Setup(s => s.Users.GetUserByIdAsync(user.Id, cts.Token)).ReturnsAsync(user);

            var validatorGetUserQueryMock = new GetUserQueryValidator();
            var getUserQueryHandlerMock = new Mock<GetUserQueryHandler>(
                validatorGetUserQueryMock,
                unitOfWorkMock.Object);

            var expectedResult = new GetUserQueryResult(user);

            // Act
            var resultResponse = await getUserQueryHandlerMock.Object.Handle(new GetUserQuery(expectedResult.Id), cts.Token);
            var result = resultResponse as Ok<GetUserQueryResult>;

            // Assert

            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Value.Should().BeEquivalentTo(expectedResult);
        }
        [Theory, AutoData]
        public async Task GetUserQuery_Should_Return_NotFound_When_User_Not_Found(CancellationTokenSource cts)
        {
            // Arrange
            const int userId = 1;
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            UserEntity? user = null;
            unitOfWorkMock.Setup(s => s.Users.GetUserByIdAsync(userId, cts.Token)).ReturnsAsync(user);

            var validatorGetUserQueryMock = new GetUserQueryValidator();
            var getUserQueryHandlerMock = new Mock<GetUserQueryHandler>(
                validatorGetUserQueryMock,
                unitOfWorkMock.Object);

            // Act
            var resultResponse = await getUserQueryHandlerMock.Object.Handle(new GetUserQuery(userId), cts.Token);
            var result = resultResponse as NotFound;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Theory, AutoData]
        public async Task GetUserQuery_Should_Return_ValidationProblem_When_Validation_Fails(CancellationTokenSource cts)
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));

            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            var user = fixture.Create<UserEntity>();
            unitOfWorkMock.Setup(s => s.Users.GetUserByIdAsync(user.Id, cts.Token)).ReturnsAsync(user);

            var validatorGetUserQueryMock = new GetUserQueryValidator();
            var getUserQueryHandlerMock = new Mock<GetUserQueryHandler>(
                validatorGetUserQueryMock,
                unitOfWorkMock.Object);

            // Act
            var resultResponse = await getUserQueryHandlerMock.Object.Handle(new GetUserQuery(default), cts.Token);
            var result = resultResponse as ProblemHttpResult;
            var problemDetails = result!.ProblemDetails as HttpValidationProblemDetails;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            problemDetails!.Errors.Should().HaveCount(1);
        }
    }
}
