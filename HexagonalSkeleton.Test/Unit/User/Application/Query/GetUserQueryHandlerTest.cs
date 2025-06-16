using Xunit;
using Moq;
using FluentValidation;
using HexagonalSkeleton.Application.Query;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Test.Unit.User.Application.Query;

public class GetUserQueryHandlerTest
{
    private readonly Mock<IValidator<GetUserQuery>> _mockValidator;
    private readonly Mock<IUserReadRepository> _mockUserReadRepository;
    private readonly GetUserQueryHandler _handler;

    public GetUserQueryHandlerTest()
    {
        _mockValidator = new Mock<IValidator<GetUserQuery>>();
        _mockUserReadRepository = new Mock<IUserReadRepository>();

        _handler = new GetUserQueryHandler(
            _mockValidator.Object,
            _mockUserReadRepository.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnUser()
    {
        // Arrange
        var userId = 1;
        var query = new GetUserQuery(userId);
        var cancellationToken = CancellationToken.None;
        var user = TestHelper.CreateTestUser();

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetUserQuery>(), cancellationToken))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _mockUserReadRepository
            .Setup(r => r.GetByIdAsync(userId, cancellationToken))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        
        var userData = result.Data as GetUserQueryResult;
        Assert.NotNull(userData);
        Assert.Equal(user.Id, userData.Id);
        Assert.Equal(user.FullName.FirstName, userData.FirstName);
        Assert.Equal(user.FullName.LastName, userData.LastName);
        Assert.Equal(user.Email.Value, userData.Email);

        _mockUserReadRepository.Verify(r => r.GetByIdAsync(userId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidQuery_ShouldReturnValidationErrors()
    {
        // Arrange
        var query = new GetUserQuery(0); // Invalid ID
        var cancellationToken = CancellationToken.None;
        var validationErrors = new FluentValidation.Results.ValidationResult();
        validationErrors.Errors.Add(new FluentValidation.Results.ValidationFailure("Id", "Id must be greater than 0"));

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetUserQuery>(), cancellationToken))
            .ReturnsAsync(validationErrors);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);

        _mockUserReadRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnError()
    {
        // Arrange
        var userId = 999;
        var query = new GetUserQuery(userId);
        var cancellationToken = CancellationToken.None;

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetUserQuery>(), cancellationToken))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _mockUserReadRepository
            .Setup(r => r.GetByIdAsync(userId, cancellationToken))
            .ReturnsAsync((HexagonalSkeleton.Domain.User?)null);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains("User not found", result.Errors.Values.SelectMany(x => x));

        _mockUserReadRepository.Verify(r => r.GetByIdAsync(userId, cancellationToken), Times.Once);
    }
}
