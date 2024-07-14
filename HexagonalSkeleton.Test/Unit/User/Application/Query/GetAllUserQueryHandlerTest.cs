using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using HexagonalSkeleton.API.Data;
using HexagonalSkeleton.API.Features.User.Application.Query;
using HexagonalSkeleton.API.Features.User.Domain;
using HexagonalSkeleton.Test.Integration.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using static HexagonalSkeleton.Test.Integration.User.UnitOfWorkFixture;

namespace HexagonalSkeleton.Test.Unit.User.Application.Query
{
    public class GetAllUserQueryHandlerTest
    {
        [Theory, AutoData]
        public async Task GetAllUsers_Should_Return_All_Entities_Without_Deleted_Ones(GetAllUsersQuery query, CancellationTokenSource cts)
        {
            // Arrange
            int count = 10;
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var users = new Fixture().CreateMany<UserEntity>(count).ToList();
            unitOfWorkMock.Setup(s => s.Users.FindAllAsync(cts.Token)).ReturnsAsync(users);

            Mock<GetAllUsersQueryHandler> getAllUsersQueryHandlerMock = new(unitOfWorkMock.Object);

            // Act
            var resultResponse = await getAllUsersQueryHandlerMock.Object.Handle(query, cts.Token);
            var result = resultResponse as Ok<IEnumerable<GetAllUsersQueryResult>>;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);
            result!.Value.Should().HaveCount(count);
        }
    }
}
