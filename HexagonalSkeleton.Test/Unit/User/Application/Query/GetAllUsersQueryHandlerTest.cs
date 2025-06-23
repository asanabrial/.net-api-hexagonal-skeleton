using Xunit;
using Moq;
using HexagonalSkeleton.Application.Query;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.ValueObjects;
using HexagonalSkeleton.Domain.Specifications;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;

namespace HexagonalSkeleton.Test.Unit.User.Application.Query;

public class GetAllUsersQueryHandlerTest
{
    private readonly Mock<IValidator<GetAllUsersQuery>> _mockValidator;
    private readonly Mock<IUserReadRepository> _mockUserReadRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetAllUsersQueryHandler _handler;

    public GetAllUsersQueryHandlerTest()
    {
        _mockValidator = new Mock<IValidator<GetAllUsersQuery>>();
        _mockUserReadRepository = new Mock<IUserReadRepository>();
        _mockMapper = new Mock<IMapper>();
        _handler = new GetAllUsersQueryHandler(_mockValidator.Object, _mockUserReadRepository.Object, _mockMapper.Object);

        // Setup validator to return valid by default
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnPaginatedUsers()
    {
        // Arrange
        var query = new GetAllUsersQuery(pageNumber: 1, pageSize: 10);
        var cancellationToken = CancellationToken.None;

        var users = new List<HexagonalSkeleton.Domain.User>
        {
            TestHelper.CreateTestUser(),
            TestHelper.CreateTestUser(id: 2, email: "user2@example.com", phoneNumber: "+1234567891")
        };

        var pagination = PaginationParams.Create(1, 10);
        var pagedResult = new PagedResult<HexagonalSkeleton.Domain.User>(users, 2, pagination);

        _mockUserReadRepository
            .Setup(r => r.GetPagedAsync(It.Is<PaginationParams>(p => p.PageNumber == 1 && p.PageSize == 10), null, cancellationToken))
            .ReturnsAsync(pagedResult);

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
            .Setup(m => m.Map<List<UserDto>>(It.IsAny<IReadOnlyList<HexagonalSkeleton.Domain.User>>()))
            .Returns(expectedUserDtos);        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert - Updated for simplified PagedQueryResult<UserDto>
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.Metadata.TotalCount);
        Assert.Equal(1, result.Metadata.PageNumber);
        Assert.Equal(10, result.Metadata.PageSize);
        Assert.Equal(1, result.Metadata.TotalPages);
        Assert.False(result.Metadata.HasNextPage);
        Assert.False(result.Metadata.HasPreviousPage);
        
        var firstUser = result.Items[0];
        Assert.Equal(users[0].Id, firstUser.Id);
        Assert.Equal(users[0].FullName.FirstName, firstUser.FirstName);
        Assert.Equal(users[0].FullName.LastName, firstUser.LastName);
        Assert.Equal(users[0].Email.Value, firstUser.Email);

        _mockUserReadRepository.Verify(r => r.GetPagedAsync(It.Is<PaginationParams>(p => p.PageNumber == 1 && p.PageSize == 10), null, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldReturnFilteredUsers()
    {
        // Arrange
        var query = new GetAllUsersQuery(pageNumber: 1, pageSize: 10, searchTerm: "john");
        var cancellationToken = CancellationToken.None;
        var users = new List<HexagonalSkeleton.Domain.User>
        {
            TestHelper.CreateTestUser()
        };

        var pagination = PaginationParams.Create(1, 10);
        var pagedResult = new PagedResult<HexagonalSkeleton.Domain.User>(users, 1, pagination);

        _mockUserReadRepository
            .Setup(r => r.GetPagedAsync(It.Is<PaginationParams>(p => p.PageNumber == 1 && p.PageSize == 10), It.IsAny<Specification<HexagonalSkeleton.Domain.User>>(), cancellationToken))
            .ReturnsAsync(pagedResult);

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
            }
        };

        _mockMapper
            .Setup(m => m.Map<List<UserDto>>(It.IsAny<IReadOnlyList<HexagonalSkeleton.Domain.User>>()))
            .Returns(expectedUserDtos);

        // Act
        var result = await _handler.Handle(query, cancellationToken);        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Single(result.Items);
        Assert.Equal(1, result.Metadata.TotalCount);

        _mockUserReadRepository.Verify(r => r.GetPagedAsync(It.Is<PaginationParams>(p => p.PageNumber == 1 && p.PageSize == 10), It.IsAny<Specification<HexagonalSkeleton.Domain.User>>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_NoUsers_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllUsersQuery();
        var cancellationToken = CancellationToken.None;

        var pagination = PaginationParams.Create(1, 10);
        var pagedResult = PagedResult<HexagonalSkeleton.Domain.User>.Empty(pagination);

        _mockUserReadRepository
            .Setup(r => r.GetPagedAsync(It.Is<PaginationParams>(p => p.PageNumber == 1 && p.PageSize == 10), null, cancellationToken))
            .ReturnsAsync(pagedResult);

        _mockMapper
            .Setup(m => m.Map<List<UserDto>>(It.IsAny<IReadOnlyList<HexagonalSkeleton.Domain.User>>()))
            .Returns(new List<UserDto>());

        // Act
        var result = await _handler.Handle(query, cancellationToken);        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.Metadata.TotalCount);
        Assert.Equal(1, result.Metadata.PageNumber);
        Assert.Equal(10, result.Metadata.PageSize);

        _mockUserReadRepository.Verify(r => r.GetPagedAsync(It.Is<PaginationParams>(p => p.PageNumber == 1 && p.PageSize == 10), null, cancellationToken), Times.Once);
    }
}
