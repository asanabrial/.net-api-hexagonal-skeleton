using Xunit;
using Moq;
using HexagonalSkeleton.Application.Query;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using AutoMapper;

namespace HexagonalSkeleton.Test.Unit.User.Application.Query;

public class GetAllUsersQueryHandlerTest
{
    private readonly Mock<IUserReadRepository> _mockUserReadRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetAllUsersQueryHandler _handler;

    public GetAllUsersQueryHandlerTest()
    {
        _mockUserReadRepository = new Mock<IUserReadRepository>();
        _mockMapper = new Mock<IMapper>();
        _handler = new GetAllUsersQueryHandler(_mockUserReadRepository.Object, _mockMapper.Object);
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
        };        _mockUserReadRepository
            .Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(users);

        var expectedUserDtos = new List<UserDto>
        {
            new UserDto
            {
                Id = users[0].Id,
                FirstName = users[0].FullName.FirstName,
                LastName = users[0].FullName.LastName,
                Birthdate = users[0].Birthdate,
                Email = users[0].Email.Value,
                LastLogin = users[0].LastLogin
            },
            new UserDto
            {
                Id = users[1].Id,
                FirstName = users[1].FullName.FirstName,
                LastName = users[1].FullName.LastName,
                Birthdate = users[1].Birthdate,
                Email = users[1].Email.Value,
                LastLogin = users[1].LastLogin
            }
        };

        _mockMapper
            .Setup(m => m.Map<IList<UserDto>>(It.IsAny<IEnumerable<HexagonalSkeleton.Domain.User>>()))
            .Returns(expectedUserDtos);

        // Act
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

        _mockMapper
            .Setup(m => m.Map<IList<UserDto>>(It.IsAny<IEnumerable<HexagonalSkeleton.Domain.User>>()))
            .Returns(new List<UserDto>());

        // Act
        var result = await _handler.Handle(query, cancellationToken);        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Users);
        Assert.Empty(result.Users);

        _mockUserReadRepository.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
    }
}
