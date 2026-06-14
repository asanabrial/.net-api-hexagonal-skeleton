using Xunit;
using HexagonalSkeleton.Test.TestHelpers;
using Moq;
using HexagonalSkeleton.Application.Features.UserManagement.Dto;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.ValueObjects;
using HexagonalSkeleton.Domain.Specifications;
using HexagonalSkeleton.Application.Services;
using FluentValidation;
using FluentValidation.Results;
using HexagonalSkeleton.Application.Features.UserManagement.Queries;

namespace HexagonalSkeleton.Test.Application.Features.UserManagement.Queries;

/// <summary>
/// Super simple tests - easy to understand for anyone
/// Tests the simplified search functionality
/// </summary>
public class GetAllUsersManagementQueryHandlerTest
{
    private readonly Mock<IValidator<GetAllUsersManagementQuery>> _mockValidator;
    private readonly Mock<IUserReadRepository> _mockUserReadRepository;
    private readonly Mock<IUserSpecificationService> _mockSpecificationService;
    private readonly GetAllUsersManagementQueryHandler _handler;

    public GetAllUsersManagementQueryHandlerTest()
    {
        _mockValidator = new Mock<IValidator<GetAllUsersManagementQuery>>();
        _mockUserReadRepository = new Mock<IUserReadRepository>();
        _mockSpecificationService = new Mock<IUserSpecificationService>();
        _handler = new GetAllUsersManagementQueryHandler(
            _mockValidator.Object,
            _mockUserReadRepository.Object,
            _mockSpecificationService.Object);

        // Setup validator to return valid by default
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetAllUsersManagementQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Setup specification service to return a default specification
        _mockSpecificationService
            .Setup(s => s.BuildSpecification(It.IsAny<GetAllUsersManagementQuery>()))
            .Returns(new Mock<ISpecification<User>>().Object);
    }

    [Fact]
    public async Task Handle_NoSearchTerm_ShouldGetAllUsers()
    {
        // Arrange
        var query = new GetAllUsersManagementQuery(pageNumber: 1, pageSize: 10); // No search term
        var cancellationToken = CancellationToken.None;

        var users = new List<User>
        {
            UserTestDataBuilder.CreateTestUser(),
            UserTestDataBuilder.CreateTestUser(id: Guid.NewGuid(), email: "user2@example.com")
        };

        var pagination = PaginationParams.Create(1, 10);
        var pagedResult = new PagedResult<User>(users, 2, pagination);

        _mockUserReadRepository
            .Setup(r => r.GetUsersAsync(It.IsAny<ISpecification<User>>(), It.IsAny<PaginationParams>(), cancellationToken))
            .ReturnsAsync(pagedResult);

        // Act - the handler now runs the real mapping
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
        var query = new GetAllUsersManagementQuery(pageNumber: 1, pageSize: 10, searchTerm: "john");
        var cancellationToken = CancellationToken.None;
        
        var users = new List<User>
        {
            UserTestDataBuilder.CreateTestUser()
        };

        var pagination = PaginationParams.Create(1, 10);
        var pagedResult = new PagedResult<User>(users, 2, pagination);

        _mockUserReadRepository
            .Setup(r => r.GetUsersAsync(It.IsAny<ISpecification<User>>(), It.IsAny<PaginationParams>(), cancellationToken))
            .ReturnsAsync(pagedResult);

        // Act - the handler now runs the real mapping
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Single(result.Items);
        Assert.Equal(2, result.Metadata.TotalCount);

        _mockUserReadRepository.Verify(r => r.GetUsersAsync(It.IsAny<ISpecification<User>>(), It.IsAny<PaginationParams>(), cancellationToken), Times.Once);
    }
}

