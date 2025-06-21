using Xunit;
using Moq;
using HexagonalSkeleton.Application.Query;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Test.Unit.User.Application.Query;

public class GetAllUsersQueryHandlerTest
{
    private readonly Mock<IUserReadRepository> _mockUserReadRepository;
    private readonly GetAllUsersQueryHandler _handler;

    public GetAllUsersQueryHandlerTest()
    {
        _mockUserReadRepository = new Mock<IUserReadRepository>();
        _handler = new GetAllUsersQueryHandler(_mockUserReadRepository.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnAllUsers()
    {
        // Arrange
        var query = new GetAllUsersQuery();
        var cancellationToken = CancellationToken.None;        var users = new List<HexagonalSkeleton.Domain.User>
        {
            TestHelper.CreateTestUser(),
            TestHelper.CreateTestUser(id: 2, email: "user2@example.com", phoneNumber: "+1234567891")
        };

        _mockUserReadRepository
            .Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(users);        // Act
        var result = await _handler.Handle(query, cancellationToken);        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Users);
        Assert.Equal(2, result.Users.Count);
        
        var firstUser = result.Users[0];
        Assert.Equal(users[0].Id, firstUser.Id);
        Assert.Equal(users[0].FullName.FirstName, firstUser.FirstName);
        Assert.Equal(users[0].FullName.LastName, firstUser.LastName);
        Assert.Equal(users[0].Email.Value, firstUser.Email);

        _mockUserReadRepository.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_NoUsers_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllUsersQuery();
        var cancellationToken = CancellationToken.None;
        var users = new List<HexagonalSkeleton.Domain.User>();        _mockUserReadRepository
            .Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(users);

        // Act
        var result = await _handler.Handle(query, cancellationToken);        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Users);
        Assert.Empty(result.Users);

        _mockUserReadRepository.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
    }
}
