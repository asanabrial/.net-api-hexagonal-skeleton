using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using HexagonalSkeleton.API.Data;
using HexagonalSkeleton.Application.Query;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Test.Integration.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace HexagonalSkeleton.Test.Unit.User.Application.Query
{
    public class GetAllUserQueryHandlerTest(UnitOfWorkFixture fixture) : IClassFixture<UnitOfWorkFixture>
    {
        [Theory, AutoData]
        public async Task GetAllUsers_Should_Return_All_Entities_Without_Deleted_Ones(GetAllUsersQuery query, CancellationTokenSource cts)
        {
            // Arrange
            const int count = 10;
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));

            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var users = fixture.CreateMany<UserEntity>(count).ToList();
            unitOfWorkMock.Setup(s => s.Users.GetAllUsersAsync(cts.Token)).ReturnsAsync(users);
            Mock<GetAllUsersQueryHandler> getAllUsersQueryHandlerMock = new(unitOfWorkMock.Object);

            // Act
            var resultResponse = await getAllUsersQueryHandlerMock.Object.Handle(query, cts.Token);
            var result = resultResponse as Ok<IEnumerable<GetAllUsersQueryResult>>;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Value.Should().HaveCount(count);
        }
    }
}
