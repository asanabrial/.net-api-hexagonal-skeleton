using Xunit;
using Moq;
using FluentValidation;
using MediatR;
using HexagonalSkeleton.Application.Command;
using HexagonalSkeleton.Application.Events;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.ValueObjects;
using AutoMapper;

namespace HexagonalSkeleton.Test.Unit.User.Application.Command
{    public class RegisterUserCommandHandlerCompleteDataTest
    {
        private readonly Mock<IValidator<RegisterUserCommand>> _mockValidator;
        private readonly Mock<IPublisher> _mockPublisher;
        private readonly Mock<IUserWriteRepository> _mockUserWriteRepository;
        private readonly Mock<IUserReadRepository> _mockUserReadRepository;
        private readonly Mock<IAuthenticationService> _mockAuthenticationService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly RegisterUserCommandHandler _handler;        public RegisterUserCommandHandlerCompleteDataTest()
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
        public async Task Handle_ValidCommand_ShouldReturnCompleteUserData()
        {
            // Arrange
            var command = TestHelper.CreateRegisterUserCommand(
                email: "complete.test@example.com",
                name: "John",
                surname: "Doe",
                phoneNumber: "+1234567890",
                latitude: 40.7128,
                longitude: -74.0060
            );
            var cancellationToken = CancellationToken.None;
            var salt = "test-salt";
            var hash = "test-hash";
            var userId = 42;
            var jwtToken = "jwt-token-123";

            // Setup mocks
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

            // Create a complete user with all data
            var createdUser = TestHelper.CreateTestUser(
                id: userId,
                email: command.Email,
                firstName: command.FirstName,
                lastName: command.LastName,
                phoneNumber: command.PhoneNumber,
                latitude: command.Latitude,
                longitude: command.Longitude,
                birthdate: command.Birthdate
            );

            _mockUserReadRepository                .Setup(r => r.GetByIdAsync(userId, cancellationToken))
                .ReturnsAsync(createdUser);            
                  _mockAuthenticationService
                .Setup(a => a.GenerateJwtTokenAsync(userId, cancellationToken))
                .ReturnsAsync(new TokenInfo(jwtToken, DateTime.UtcNow.AddDays(7)));            // Setup AutoMapper mock to return a properly mapped result with nested structure
            _mockMapper
                .Setup(m => m.Map<UserDto>(It.IsAny<HexagonalSkeleton.Domain.User>()))
                .Returns((HexagonalSkeleton.Domain.User user) => new UserDto
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
            var result = await _handler.Handle(command, cancellationToken);            // Assert - Verify ALL user data is returned
            Assert.NotNull(result);
            Assert.Equal(jwtToken, result.AccessToken);
            Assert.NotNull(result.User);
            
            // Verify user identification
            Assert.Equal(userId, result.User.Id);
            
            // Verify names
            Assert.Equal("John", result.User.FirstName);
            Assert.Equal("Doe", result.User.LastName);
            Assert.Equal("John Doe", result.User.FullName);
            
            // Verify contact information
            Assert.Equal("complete.test@example.com", result.User.Email);
            Assert.Equal("+1234567890", result.User.PhoneNumber);
            
            // Verify personal information
            Assert.Equal(command.Birthdate, result.User.Birthdate);
            Assert.Equal("Test user", result.User.AboutMe);
            
            // Verify location
            Assert.Equal(40.7128, result.User.Latitude);
            Assert.Equal(-74.0060, result.User.Longitude);
            
            // Verify timestamps
            Assert.True(result.User.CreatedAt > DateTime.MinValue);
            
            // Verify repository calls were made correctly
            _mockUserWriteRepository.Verify(r => r.CreateAsync(
                It.Is<HexagonalSkeleton.Domain.User>(u => 
                    u.Email.Value == command.Email &&
                    u.FullName.FirstName == command.FirstName &&
                    u.FullName.LastName == command.LastName
                ), 
                cancellationToken), Times.Once);

            _mockUserReadRepository.Verify(r => r.GetByIdAsync(userId, cancellationToken), Times.Once);
            
            _mockPublisher.Verify(p => p.Publish(
                It.IsAny<LoginEvent>(), 
                cancellationToken), Times.Once);
        }
    }
}
