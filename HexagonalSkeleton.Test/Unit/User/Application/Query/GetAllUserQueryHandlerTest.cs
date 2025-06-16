using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using HexagonalSkeleton.Application.Query;
using HexagonalSkeleton.Domain.Ports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using DomainUser = HexagonalSkeleton.Domain.User;

namespace HexagonalSkeleton.Test.Unit.User.Application.Query
{
    public class GetAllUserQueryHandlerTest
    {
        [Theory, AutoData]
        public async Task GetAllUsers_Should_Return_All_Entities_Without_Deleted_Ones(GetAllUsersQuery query, CancellationTokenSource cts)
        {
            // Arrange
            const int count = 10;
            var userReadRepositoryMock = new Mock<IUserReadRepository>();
            var fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));

            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var users = fixture.CreateMany<DomainUser>(count).ToList();
            userReadRepositoryMock.Setup(s => s.GetAllUsersAsync(cts.Token)).ReturnsAsync(users);
            var getAllUsersQueryHandler = new GetAllUsersQueryHandler(userReadRepositoryMock.Object);

            // Act
            var resultResponse = await getAllUsersQueryHandler.Handle(query, cts.Token);
            var result = resultResponse as Ok<IEnumerable<GetAllUsersQueryResult>>;            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Value.Should().HaveCount(count);
        }
    }
}
