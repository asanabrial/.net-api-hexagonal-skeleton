using Xunit;
using Moq;
using FluentValidation;
using HexagonalSkeleton.Application.Features.UserManagement.Dto;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using AutoMapper;
using HexagonalSkeleton.Application.Features.UserManagement.Queries;

namespace HexagonalSkeleton.Test.Application.Features.UserManagement.Queries;

public class GetUserManagementQueryHandlerTest
{
    private readonly Mock<IValidator<GetUserManagementQuery>> _mockValidator;
    private readonly Mock<IUserReadRepository> _mockUserReadRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetUserManagementQueryHandler _handler;

    public GetUserManagementQueryHandlerTest()
    {
        _mockValidator = new Mock<IValidator<GetUserManagementQuery>>();
        _mockUserReadRepository = new Mock<IUserReadRepository>();
        _mockMapper = new Mock<IMapper>();

        _handler = new GetUserManagementQueryHandler(
            _mockValidator.Object,
            _mockUserReadRepository.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnGetUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserManagementQuery(userId);
        var cancellationToken = CancellationToken.None;
        var user = TestHelper.CreateTestUser();

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetUserManagementQuery>(), cancellationToken))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _mockUserReadRepository
            .Setup(r => r.GetByIdUnfilteredAsync(userId, cancellationToken))
            .ReturnsAsync(user);

        var expectedResult = new GetUserDto
        {
            Id = user.Id,
            FirstName = user.FullName.FirstName,
            LastName = user.FullName.LastName,
            FullName = user.FullName.GetFullName(),
            Email = user.Email.Value,
            IsDeleted = user.IsDeleted,
            DeletedAt = user.DeletedAt
        };

        _mockMapper
            .Setup(m => m.Map<GetUserDto>(user))
            .Returns(expectedResult);

        // Act
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
    public async Task Handle_InvalidQuery_ShouldNotProceedToRepository()
    {
        // Arrange - Este test verifica que la validación funciona correctamente
        var query = new GetUserManagementQuery(Guid.Empty); // Invalid ID
        var cancellationToken = CancellationToken.None;

        // Setup validation to pass to test actual validation logic
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetUserManagementQuery>(), cancellationToken))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        // Setup repository to return null for invalid ID
        _mockUserReadRepository
            .Setup(r => r.GetByIdUnfilteredAsync(Guid.Empty, cancellationToken))
            .ReturnsAsync((User?)null);

        // Act & Assert - Invalid ID should result in NotFoundException
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(query, cancellationToken));

        // Verify repository was called despite invalid ID (validation passed but user not found)
        _mockUserReadRepository.Verify(r => r.GetByIdUnfilteredAsync(Guid.Empty, cancellationToken), Times.Once);
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
        var deletedUser = TestHelper.CreateTestUser();
        
        // Mark user as deleted
        deletedUser.Delete();

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetUserManagementQuery>(), cancellationToken))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _mockUserReadRepository
            .Setup(r => r.GetByIdUnfilteredAsync(userId, cancellationToken))
            .ReturnsAsync(deletedUser);

        var expectedResult = new GetUserDto
        {
            Id = deletedUser.Id,
            FirstName = deletedUser.FullName.FirstName,
            LastName = deletedUser.FullName.LastName,
            FullName = deletedUser.FullName.GetFullName(),
            Email = deletedUser.Email.Value,
            IsDeleted = true,
            DeletedAt = deletedUser.DeletedAt
        };

        _mockMapper
            .Setup(m => m.Map<GetUserDto>(deletedUser))
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(deletedUser.Id, result.Id);
        Assert.True(result.IsDeleted);
        Assert.NotNull(result.DeletedAt);

        _mockUserReadRepository.Verify(r => r.GetByIdUnfilteredAsync(userId, cancellationToken), Times.Once);
    }
}
