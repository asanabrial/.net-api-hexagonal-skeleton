using Xunit;
using HexagonalSkeleton.Test.TestHelpers;
using Moq;
using FluentValidation;
using HexagonalSkeleton.Application.Features.UserManagement.Dto;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Application.Features.UserManagement.Queries;

namespace HexagonalSkeleton.Test.Application.Features.UserManagement.Queries;

public class GetUserManagementQueryHandlerTest
{
    private readonly Mock<IValidator<GetUserManagementQuery>> _mockValidator;
    private readonly Mock<IUserReadRepository> _mockUserReadRepository;
    private readonly GetUserManagementQueryHandler _handler;

    public GetUserManagementQueryHandlerTest()
    {
        _mockValidator = new Mock<IValidator<GetUserManagementQuery>>();
        _mockUserReadRepository = new Mock<IUserReadRepository>();

        _handler = new GetUserManagementQueryHandler(
            _mockValidator.Object,
            _mockUserReadRepository.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnGetUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserManagementQuery(userId);
        var cancellationToken = CancellationToken.None;
        var user = UserTestDataBuilder.CreateTestUser();

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetUserManagementQuery>(), cancellationToken))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _mockUserReadRepository
            .Setup(r => r.GetByIdUnfilteredAsync(userId, cancellationToken))
            .ReturnsAsync(user);

        // Act - the handler now runs the real mapping
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.FullName.FirstName, result.FirstName);
        Assert.Equal(user.FullName.LastName, result.LastName);
        Assert.Equal(user.Email.Value, result.Email);
        Assert.Equal(user.IsDeleted, result.IsDeleted);
        Assert.Equal(user.DeletedAt, result.DeletedAt);

        _mockUserReadRepository.Verify(r => r.GetByIdUnfilteredAsync(userId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserManagementQuery(userId);
        var cancellationToken = CancellationToken.None;

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetUserManagementQuery>(), cancellationToken))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _mockUserReadRepository
            .Setup(r => r.GetByIdUnfilteredAsync(userId, cancellationToken))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal($"User with identifier '{userId}' was not found", exception.Message);

        _mockUserReadRepository.Verify(r => r.GetByIdUnfilteredAsync(userId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_DeletedUser_ShouldReturnUserWithDeletionInfo()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserManagementQuery(userId);
        var cancellationToken = CancellationToken.None;
        var deletedUser = UserTestDataBuilder.CreateTestUser();
        
        // Mark user as deleted
        deletedUser.Delete();

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetUserManagementQuery>(), cancellationToken))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _mockUserReadRepository
            .Setup(r => r.GetByIdUnfilteredAsync(userId, cancellationToken))
            .ReturnsAsync(deletedUser);

        // Act - the handler now runs the real mapping
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(deletedUser.Id, result.Id);
        Assert.True(result.IsDeleted);
        Assert.NotNull(result.DeletedAt);

        _mockUserReadRepository.Verify(r => r.GetByIdUnfilteredAsync(userId, cancellationToken), Times.Once);
    }
}
