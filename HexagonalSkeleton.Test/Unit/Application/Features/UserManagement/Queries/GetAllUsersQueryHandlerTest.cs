using Xunit;
using Moq;
using HexagonalSkeleton.Application.Features.UserManagement.Dto;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.ValueObjects;
using HexagonalSkeleton.Domain.Specifications;
using HexagonalSkeleton.Application.Services;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using HexagonalSkeleton.Application.Features.UserManagement.Queries;
using HexagonalSkeleton.Test.TestInfrastructure.Helpers;

namespace HexagonalSkeleton.Test.Application.Features.UserManagement.Queries;

/// <summary>
/// Super simple tests - easy to understand for anyone
/// Tests the simplified search functionality
/// </summary>
public class GetAllUsersQueryHandlerTest
{
    private readonly Mock<IValidator<GetAllUsersQuery>> _mockValidator;
    private readonly Mock<IUserReadRepository> _mockUserReadRepository;
    private readonly Mock<IUserSpecificationService> _mockSpecificationService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetAllUsersQueryHandler _handler;

    public GetAllUsersQueryHandlerTest()
    {
        _mockValidator = new Mock<IValidator<GetAllUsersQuery>>();
        _mockUserReadRepository = new Mock<IUserReadRepository>();
        _mockSpecificationService = new Mock<IUserSpecificationService>();
        _mockMapper = new Mock<IMapper>();
        _handler = new GetAllUsersQueryHandler(
            _mockValidator.Object, 
            _mockUserReadRepository.Object, 
            _mockSpecificationService.Object,
            _mockMapper.Object);

        // Setup validator to return valid by default
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Setup specification service to return a default specification
        _mockSpecificationService
            .Setup(s => s.BuildSpecification(It.IsAny<GetAllUsersQuery>()))
            .Returns(new Mock<ISpecification<User>>().Object);
    }

    [Fact]
    public async Task Handle_NoSearchTerm_ShouldGetAllUsers()
    {
        // Arrange
        var query = new GetAllUsersQuery(pageNumber: 1, pageSize: 10); // No search term
        var cancellationToken = CancellationToken.None;

        var users = new List<User>
        {
            TestHelper.CreateTestUser(),
            TestHelper.CreateTestUser(id: Guid.NewGuid(), email: "user2@example.com")
        };

        var pagination = PaginationParams.Create(1, 10);
        var pagedResult = new PagedResult<User>(users, 2, pagination);

        _mockUserReadRepository
            .Setup(r => r.GetUsersAsync(It.IsAny<ISpecification<User>>(), It.IsAny<PaginationParams>(), cancellationToken))
            .ReturnsAsync(pagedResult);

        var expectedUserDtos = new List<GetAllUsersDto>
        {
            new GetAllUsersDto { Id = users[0].Id },
            new GetAllUsersDto { Id = users[1].Id }
        };

        _mockMapper
            .Setup(m => m.Map<List<GetAllUsersDto>>(It.IsAny<IReadOnlyList<User>>()))
            .Returns(expectedUserDtos);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.Metadata.TotalCount);

        _mockUserReadRepository.Verify(r => r.GetUsersAsync(It.IsAny<ISpecification<User>>(), It.IsAny<PaginationParams>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldSearchUsers()
    {
        // Arrange
        var query = new GetAllUsersQuery(pageNumber: 1, pageSize: 10, searchTerm: "john");
        var cancellationToken = CancellationToken.None;
        
        var users = new List<User>
        {
            TestHelper.CreateTestUser()
        };

        var pagination = PaginationParams.Create(1, 10);
        var pagedResult = new PagedResult<User>(users, 2, pagination);

        _mockUserReadRepository
            .Setup(r => r.GetUsersAsync(It.IsAny<ISpecification<User>>(), It.IsAny<PaginationParams>(), cancellationToken))
            .ReturnsAsync(pagedResult);

        var expectedUserDtos = new List<GetAllUsersDto>
        {
            new GetAllUsersDto { Id = users[0].Id }
        };

        _mockMapper
            .Setup(m => m.Map<List<GetAllUsersDto>>(It.IsAny<IReadOnlyList<User>>()))
            .Returns(expectedUserDtos);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Single(result.Items);
        Assert.Equal(2, result.Metadata.TotalCount);

        _mockUserReadRepository.Verify(r => r.GetUsersAsync(It.IsAny<ISpecification<User>>(), It.IsAny<PaginationParams>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptySearchTerm_ShouldGetAllUsers()
    {
        // Arrange
        var query = new GetAllUsersQuery(pageNumber: 1, pageSize: 10, searchTerm: "   "); // Empty/whitespace
        var cancellationToken = CancellationToken.None;

        var users = new List<User>();
        var pagination = PaginationParams.Create(1, 10);
        var pagedResult = new PagedResult<User>(users, 0, pagination);

        _mockUserReadRepository
            .Setup(r => r.GetUsersAsync(It.IsAny<ISpecification<User>>(), It.IsAny<PaginationParams>(), cancellationToken))
            .ReturnsAsync(pagedResult);

        _mockMapper
            .Setup(m => m.Map<List<GetAllUsersDto>>(It.IsAny<IReadOnlyList<User>>()))
            .Returns(new List<GetAllUsersDto>());

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.Metadata.TotalCount);

        _mockUserReadRepository.Verify(r => r.GetUsersAsync(It.IsAny<ISpecification<User>>(), It.IsAny<PaginationParams>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_SearchByPhone_ShouldWork()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var query = new GetAllUsersQuery(pageNumber: 1, pageSize: 10, searchTerm: phoneNumber);
        var cancellationToken = CancellationToken.None;

        var users = new List<User>
        {
            TestHelper.CreateTestUser(phoneNumber: phoneNumber)
        };

        var pagination = PaginationParams.Create(1, 10);
        var pagedResult = new PagedResult<User>(users, 1, pagination);

        _mockUserReadRepository
            .Setup(r => r.GetUsersAsync(It.IsAny<ISpecification<User>>(), It.IsAny<PaginationParams>(), cancellationToken))
            .ReturnsAsync(pagedResult);

        var userDtos = new List<GetAllUsersDto>
        {
            new GetAllUsersDto { Id = Guid.NewGuid(), PhoneNumber = phoneNumber }
        };

        _mockMapper
            .Setup(m => m.Map<List<GetAllUsersDto>>(It.IsAny<IReadOnlyList<User>>()))
            .Returns(userDtos);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(phoneNumber, result.Items[0].PhoneNumber);

        _mockUserReadRepository.Verify(r => r.GetUsersAsync(It.IsAny<ISpecification<User>>(), It.IsAny<PaginationParams>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_SearchByEmail_ShouldWork()
    {
        // Arrange
        var email = "test@example.com";
        var query = new GetAllUsersQuery(pageNumber: 1, pageSize: 10, searchTerm: email);
        var cancellationToken = CancellationToken.None;

        var users = new List<User>
        {
            TestHelper.CreateTestUser(email: email)
        };

        var pagination = PaginationParams.Create(1, 10);
        var pagedResult = new PagedResult<User>(users, 1, pagination);

        _mockUserReadRepository
            .Setup(r => r.GetUsersAsync(It.IsAny<ISpecification<User>>(), It.IsAny<PaginationParams>(), cancellationToken))
            .ReturnsAsync(pagedResult);

        var userDtos = new List<GetAllUsersDto>
        {
            new GetAllUsersDto { Id = Guid.NewGuid(), Email = email }
        };

        _mockMapper
            .Setup(m => m.Map<List<GetAllUsersDto>>(It.IsAny<IReadOnlyList<User>>()))
            .Returns(userDtos);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(email, result.Items[0].Email);

        _mockUserReadRepository.Verify(r => r.GetUsersAsync(It.IsAny<ISpecification<User>>(), It.IsAny<PaginationParams>(), cancellationToken), Times.Once);
    }
}

