using Xunit;
using Moq;
using FluentValidation;
using MediatR;
using HexagonalSkeleton.Application.Features.UserAuthentication.Dto;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.ValueObjects;
using AutoMapper;
using HexagonalSkeleton.Application.Features.UserRegistration.Commands;
using HexagonalSkeleton.Application.Events;

namespace HexagonalSkeleton.Test.Application.Features.UserRegistration.Commands;

public class RegisterUserCommandHandlerTest
{    private readonly Mock<IValidator<RegisterUserCommand>> _mockValidator;
    private readonly Mock<IPublisher> _mockPublisher;
    private readonly Mock<IUserWriteRepository> _mockUserWriteRepository;
    private readonly Mock<IUserReadRepository> _mockUserReadRepository;
    private readonly Mock<IAuthenticationService> _mockAuthenticationService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly RegisterUserCommandHandler _handler;    public RegisterUserCommandHandlerTest()
    {
        _mockValidator = new Mock<IValidator<RegisterUserCommand>>();
        _mockPublisher = new Mock<IPublisher>();
        _mockUserWriteRepository = new Mock<IUserWriteRepository>();
        _mockUserReadRepository = new Mock<IUserReadRepository>();
        _mockAuthenticationService = new Mock<IAuthenticationService>();
        _mockMapper = new Mock<IMapper>();

        _handler = new RegisterUserCommandHandler(
            _mockValidator.Object,
            _mockPublisher.Object,
            _mockUserWriteRepository.Object,
            _mockUserReadRepository.Object,
            _mockAuthenticationService.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand();
        var cancellationToken = CancellationToken.None;
        var salt = "salt";
        var hash = "hash";
        var userId = Guid.NewGuid();
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
            .Returns(hash);        _mockUserWriteRepository
            .Setup(r => r.CreateAsync(It.IsAny<User>(), cancellationToken))
            .ReturnsAsync(userId);

        // Mock the GetByIdAsync to return the created user
        var createdUser = TestHelper.CreateTestUser(userId);        _mockUserReadRepository
            .Setup(r => r.GetByIdAsync(userId, cancellationToken))
            .ReturnsAsync(createdUser);        
          _mockAuthenticationService
            .Setup(a => a.GenerateJwtTokenAsync(userId, cancellationToken))
            .ReturnsAsync(new TokenInfo(jwtToken, DateTime.UtcNow.AddDays(7)));

        // Setup AutoMapper mock to return a properly mapped result with nested structure
        _mockMapper
            .Setup(m => m.Map<AuthenticatedUserDto>(It.IsAny<User>()))
            .Returns((User user) => new AuthenticatedUserDto
            {
                Id = user.Id,
                FirstName = user.FullName.FirstName,
                LastName = user.FullName.LastName,
                Email = user.Email.Value,
                PhoneNumber = user.PhoneNumber?.Value,
                Birthdate = user.Birthdate,
                Latitude = user.Location?.Latitude,
                Longitude = user.Location?.Longitude,
                AboutMe = user.AboutMe,
                CreatedAt = user.CreatedAt
            });

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(jwtToken, result.AccessToken);

        _mockUserWriteRepository.Verify(r => r.CreateAsync(
            It.Is<User>(u => u.Email.Value == command.Email), 
            cancellationToken), Times.Once);

        _mockPublisher.Verify(p => p.Publish(
            It.IsAny<LoginEvent>(), 
            cancellationToken), Times.Once);
    }    [Fact]
    public async Task Handle_InvalidCommand_ShouldThrowValidationException()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand();
        var cancellationToken = CancellationToken.None;
        var validationResult = new FluentValidation.Results.ValidationResult();
        validationResult.Errors.Add(new FluentValidation.Results.ValidationFailure("Email", "Email is required"));

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<RegisterUserCommand>(), cancellationToken))
            .ReturnsAsync(validationResult);        // Act & Assert
        var exception = await Assert.ThrowsAsync<HexagonalSkeleton.Application.Exceptions.ValidationException>(() => 
            _handler.Handle(command, cancellationToken));
          Assert.True(exception.Errors.ContainsKey("Email"));
        Assert.Contains("Email is required", exception.Errors["Email"]);

        _mockUserWriteRepository.Verify(r => r.CreateAsync(
            It.IsAny<User>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }    [Fact]
    public async Task Handle_EmailAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand();
        var cancellationToken = CancellationToken.None;

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<RegisterUserCommand>(), cancellationToken))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());        _mockUserReadRepository
            .Setup(r => r.ExistsByEmailAsync(command.Email, cancellationToken))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Domain.Exceptions.UserDataNotUniqueException>(() => 
            _handler.Handle(command, cancellationToken));
        
        Assert.Equal("test@example.com", exception.Email);
        Assert.Equal("+1234567890", exception.PhoneNumber);

        _mockUserWriteRepository.Verify(r => r.CreateAsync(
            It.IsAny<User>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }    [Fact]
    public async Task Handle_PhoneNumberAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand();
        var cancellationToken = CancellationToken.None;

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<RegisterUserCommand>(), cancellationToken))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _mockUserReadRepository
            .Setup(r => r.ExistsByEmailAsync(command.Email, cancellationToken))
            .ReturnsAsync(false);        _mockUserReadRepository
            .Setup(r => r.ExistsByPhoneNumberAsync(command.PhoneNumber, cancellationToken))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Domain.Exceptions.UserDataNotUniqueException>(() => 
            _handler.Handle(command, cancellationToken));
        
        Assert.Equal("test@example.com", exception.Email);
        Assert.Equal("+1234567890", exception.PhoneNumber);

        _mockUserWriteRepository.Verify(r => r.CreateAsync(
            It.IsAny<User>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }    [Fact]
    public async Task Handle_WeakPassword_ShouldThrowValidationException()
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
            .ReturnsAsync(false);        // Act & Assert
        var exception = await Assert.ThrowsAsync<Domain.Exceptions.WeakPasswordException>(() => 
            _handler.Handle(command, cancellationToken));
        
        Assert.Contains("Password does not meet strength requirements", exception.Message);

        _mockUserWriteRepository.Verify(r => r.CreateAsync(
            It.IsAny<User>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
