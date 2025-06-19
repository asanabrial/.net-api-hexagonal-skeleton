using Xunit;
using Moq;
using FluentValidation;
using MediatR;
using HexagonalSkeleton.Application.Command;
using HexagonalSkeleton.Application.Event;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Test.Unit.User.Application.Command;

public class RegisterUserCommandHandlerTest
{
    private readonly Mock<IValidator<RegisterUserCommand>> _mockValidator;
    private readonly Mock<IPublisher> _mockPublisher;
    private readonly Mock<IUserWriteRepository> _mockUserWriteRepository;
    private readonly Mock<IUserReadRepository> _mockUserReadRepository;
    private readonly Mock<IAuthenticationService> _mockAuthenticationService;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTest()
    {
        _mockValidator = new Mock<IValidator<RegisterUserCommand>>();
        _mockPublisher = new Mock<IPublisher>();
        _mockUserWriteRepository = new Mock<IUserWriteRepository>();
        _mockUserReadRepository = new Mock<IUserReadRepository>();
        _mockAuthenticationService = new Mock<IAuthenticationService>();

        _handler = new RegisterUserCommandHandler(
            _mockValidator.Object,
            _mockPublisher.Object,
            _mockUserWriteRepository.Object,
            _mockUserReadRepository.Object,
            _mockAuthenticationService.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand();
        var cancellationToken = CancellationToken.None;
        var salt = "salt";
        var hash = "hash";
        var userId = 1;
        var jwtToken = "jwt-token";

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<RegisterUserCommand>(), cancellationToken))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _mockUserReadRepository
            .Setup(r => r.ExistsByEmailAsync(command.Email, cancellationToken))
            .ReturnsAsync(false);

        _mockUserReadRepository
            .Setup(r => r.ExistsByPhoneNumberAsync(command.PhoneNumber, cancellationToken))
            .ReturnsAsync(false);

        _mockAuthenticationService
            .Setup(a => a.GenerateSalt())
            .Returns(salt);

        _mockAuthenticationService
            .Setup(a => a.HashPassword(command.Password, salt))
            .Returns(hash);

        _mockUserWriteRepository
            .Setup(r => r.CreateAsync(It.IsAny<HexagonalSkeleton.Domain.User>(), cancellationToken))
            .ReturnsAsync(userId);

        _mockAuthenticationService
            .Setup(a => a.GenerateJwtTokenAsync(userId, cancellationToken))
            .ReturnsAsync(jwtToken);        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid);
        Assert.Equal(jwtToken, result.AccessToken);

        _mockUserWriteRepository.Verify(r => r.CreateAsync(
            It.Is<HexagonalSkeleton.Domain.User>(u => u.Email.Value == command.Email), 
            cancellationToken), Times.Once);

        _mockPublisher.Verify(p => p.Publish(
            It.IsAny<LoginEvent>(), 
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidCommand_ShouldReturnValidationErrors()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand();
        var cancellationToken = CancellationToken.None;
        var validationResult = new FluentValidation.Results.ValidationResult();
        validationResult.Errors.Add(new FluentValidation.Results.ValidationFailure("Email", "Email is required"));

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<RegisterUserCommand>(), cancellationToken))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains("Email", result.Errors.Keys);

        _mockUserWriteRepository.Verify(r => r.CreateAsync(
            It.IsAny<HexagonalSkeleton.Domain.User>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmailAlreadyExists_ShouldReturnError()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand();
        var cancellationToken = CancellationToken.None;

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<RegisterUserCommand>(), cancellationToken))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _mockUserReadRepository
            .Setup(r => r.ExistsByEmailAsync(command.Email, cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);

        _mockUserWriteRepository.Verify(r => r.CreateAsync(
            It.IsAny<HexagonalSkeleton.Domain.User>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PhoneNumberAlreadyExists_ShouldReturnError()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand();
        var cancellationToken = CancellationToken.None;

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<RegisterUserCommand>(), cancellationToken))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _mockUserReadRepository
            .Setup(r => r.ExistsByEmailAsync(command.Email, cancellationToken))
            .ReturnsAsync(false);

        _mockUserReadRepository
            .Setup(r => r.ExistsByPhoneNumberAsync(command.PhoneNumber, cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);

        _mockUserWriteRepository.Verify(r => r.CreateAsync(
            It.IsAny<HexagonalSkeleton.Domain.User>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WeakPassword_ShouldReturnError()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand(password: "weak");
        var cancellationToken = CancellationToken.None;

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<RegisterUserCommand>(), cancellationToken))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _mockUserReadRepository
            .Setup(r => r.ExistsByEmailAsync(command.Email, cancellationToken))
            .ReturnsAsync(false);

        _mockUserReadRepository
            .Setup(r => r.ExistsByPhoneNumberAsync(command.PhoneNumber, cancellationToken))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);

        _mockUserWriteRepository.Verify(r => r.CreateAsync(
            It.IsAny<HexagonalSkeleton.Domain.User>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
