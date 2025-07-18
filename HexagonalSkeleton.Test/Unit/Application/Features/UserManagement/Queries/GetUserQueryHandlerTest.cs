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

public class GetUserQueryHandlerTest
{
    private readonly Mock<IValidator<GetUserQuery>> _mockValidator;
    private readonly Mock<IUserReadRepository> _mockUserReadRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetUserQueryHandler _handler;

    public GetUserQueryHandlerTest()
    {
        _mockValidator = new Mock<IValidator<GetUserQuery>>();
        _mockUserReadRepository = new Mock<IUserReadRepository>();
        _mockMapper = new Mock<IMapper>();

        _handler = new GetUserQueryHandler(
            _mockValidator.Object,
            _mockUserReadRepository.Object,
            _mockMapper.Object);
    }    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserQuery(userId);
        var cancellationToken = CancellationToken.None;
        var user = TestHelper.CreateTestUser();

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetUserQuery>(), cancellationToken))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());        _mockUserReadRepository
            .Setup(r => r.GetByIdAsync(userId, cancellationToken))
            .ReturnsAsync(user);        var expectedResult = new GetUserDto
        {
            Id = user.Id,
            FirstName = user.FullName.FirstName,
            LastName = user.FullName.LastName,
            Birthdate = user.Birthdate,
            Email = user.Email.Value,
            LastLogin = user.LastLogin
        };

        _mockMapper
            .Setup(m => m.Map<GetUserDto>(It.IsAny<User>()))
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.FullName.FirstName, result.FirstName);
        Assert.Equal(user.FullName.LastName, result.LastName);
        Assert.Equal(user.Email.Value, result.Email);

        _mockUserReadRepository.Verify(r => r.GetByIdAsync(userId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidQuery_ShouldThrowValidationException()
    {
        // Arrange
        var query = new GetUserQuery(Guid.Empty); // Invalid ID
        var cancellationToken = CancellationToken.None;
        var validationErrors = new FluentValidation.Results.ValidationResult();
        validationErrors.Errors.Add(new FluentValidation.Results.ValidationFailure("Id", "Id cannot be empty"));

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetUserQuery>(), cancellationToken))
            .ReturnsAsync(validationErrors);        // Act
        var act = async () => await _handler.Handle(query, cancellationToken);        // Assert
        await Assert.ThrowsAsync<HexagonalSkeleton.Application.Exceptions.ValidationException>(act);
        _mockUserReadRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserQuery(userId);
        var cancellationToken = CancellationToken.None;

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetUserQuery>(), cancellationToken))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _mockUserReadRepository
            .Setup(r => r.GetByIdAsync(userId, cancellationToken))
            .ReturnsAsync((User?)null);        // Act
        var act = async () => await _handler.Handle(query, cancellationToken);        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal($"User with identifier '{userId}' was not found", exception.Message);

        _mockUserReadRepository.Verify(r => r.GetByIdAsync(userId, cancellationToken), Times.Once);
    }
}

