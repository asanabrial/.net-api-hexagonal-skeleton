using Xunit;
using Moq;
using FluentValidation;
using FluentAssertions;
using HexagonalSkeleton.Application.Query;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Application.Exceptions;
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
    }    [Fact]
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
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.FirstName.Should().Be(user.FullName.FirstName);
        result.LastName.Should().Be(user.FullName.LastName);
        result.Email.Should().Be(user.Email.Value);

        _mockUserReadRepository.Verify(r => r.GetByIdAsync(userId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidQuery_ShouldThrowValidationException()
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
        var act = async () => await _handler.Handle(query, cancellationToken);        // Assert
        await act.Should().ThrowAsync<HexagonalSkeleton.Application.Exceptions.ValidationException>();
        _mockUserReadRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowNotFoundException()
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
        var act = async () => await _handler.Handle(query, cancellationToken);        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User with identifier '999' was not found");

        _mockUserReadRepository.Verify(r => r.GetByIdAsync(userId, cancellationToken), Times.Once);
    }
}
