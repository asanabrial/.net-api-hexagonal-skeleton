using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using HexagonalSkeleton.Application.Command;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Test.Unit.User.Domain;
using Moq;
using Xunit;
using DomainUser = HexagonalSkeleton.Domain.User;

namespace HexagonalSkeleton.Test.Unit.User.Application.Command
{
    public class UpdateProfileUserCommandHandlerTest
    {
        private readonly Mock<IValidator<UpdateProfileUserCommand>> _mockValidator;
        private readonly Mock<IUserReadRepository> _mockUserReadRepository;
        private readonly Mock<IUserWriteRepository> _mockUserWriteRepository;
        private readonly UpdateProfileUserCommandHandler _handler;

        public UpdateProfileUserCommandHandlerTest()
        {
            _mockValidator = new Mock<IValidator<UpdateProfileUserCommand>>();
            _mockUserReadRepository = new Mock<IUserReadRepository>();
            _mockUserWriteRepository = new Mock<IUserWriteRepository>();
            _handler = new UpdateProfileUserCommandHandler(
                _mockValidator.Object,
                _mockUserReadRepository.Object,
                _mockUserWriteRepository.Object);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldReturnSuccessResult()
        {
            // Arrange
            var command = new UpdateProfileUserCommand(
                id: 1,
                aboutMe: "Updated about me",
                firstName: "Jane",
                lastName: "Smith",
                birthdate: new DateTime(1985, 5, 15));
            
            var user = TestHelper.CreateTestUser(1);

            _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockUserReadRepository.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockUserWriteRepository.Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();

            // Verify the user profile was updated
            user.FullName.FirstName.Should().Be("Jane");
            user.FullName.LastName.Should().Be("Smith");
            user.Birthdate.Should().Be(new DateTime(1985, 5, 15));
            user.AboutMe.Should().Be("Updated about me");

            _mockUserWriteRepository.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithInvalidCommand_ShouldReturnValidationErrors()
        {
            // Arrange
            var command = new UpdateProfileUserCommand(
                id: 0,
                aboutMe: "",
                firstName: "",
                lastName: "",
                birthdate: DateTime.Now.AddYears(-5)); // Too young
            
            var validationErrors = new List<ValidationFailure>
            {
                new ValidationFailure("Id", "Id must be greater than 0"),
                new ValidationFailure("FirstName", "First name is required"),
                new ValidationFailure("LastName", "Last name is required")
            };
            var validationResult = new ValidationResult(validationErrors);

            _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainKey("Id");
            result.Errors.Should().ContainKey("FirstName");
            result.Errors.Should().ContainKey("LastName");

            _mockUserReadRepository.Verify(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockUserWriteRepository.Verify(x => x.UpdateAsync(It.IsAny<DomainUser>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithNonExistentUser_ShouldReturnUserNotFoundError()
        {
            // Arrange
            var command = new UpdateProfileUserCommand(
                id: 999,
                aboutMe: "Updated about me",
                firstName: "Jane",
                lastName: "Smith",
                birthdate: new DateTime(1985, 5, 15));

            _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockUserReadRepository.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DomainUser?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainKey("User");
            result.Errors["User"].Should().Contain("User not found");

            _mockUserWriteRepository.Verify(x => x.UpdateAsync(It.IsAny<DomainUser>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData("John", "Doe")]
        [InlineData("María", "García")]
        [InlineData("Jean-Pierre", "Dubois")]
        public async Task Handle_WithDifferentNameFormats_ShouldUpdateCorrectly(string firstName, string lastName)
        {
            // Arrange
            var command = new UpdateProfileUserCommand(
                id: 1,
                aboutMe: "Test bio",
                firstName: firstName,
                lastName: lastName,
                birthdate: new DateTime(1990, 1, 1));
            
            var user = TestHelper.CreateTestUser(1);

            _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockUserReadRepository.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockUserWriteRepository.Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            user.FullName.FirstName.Should().Be(firstName);
            user.FullName.LastName.Should().Be(lastName);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var command = new UpdateProfileUserCommand(
                id: 1,
                aboutMe: "Updated about me",
                firstName: "Jane",
                lastName: "Smith",
                birthdate: new DateTime(1985, 5, 15));
            
            var user = TestHelper.CreateTestUser(1);

            _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockUserReadRepository.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockUserWriteRepository.Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database connection error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Be("Database connection error");
        }

        [Fact]
        public async Task Handle_ShouldUpdateUserProfileAndTimestamp()
        {
            // Arrange
            var command = new UpdateProfileUserCommand(
                id: 1,
                aboutMe: "Updated about me",
                firstName: "Jane",
                lastName: "Smith",
                birthdate: new DateTime(1985, 5, 15));
            
            var user = TestHelper.CreateTestUser(1);
            var initialUpdatedAt = user.UpdatedAt;

            _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockUserReadRepository.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            user.FullName.FirstName.Should().Be("Jane");
            user.FullName.LastName.Should().Be("Smith");
            user.Birthdate.Should().Be(new DateTime(1985, 5, 15));
            user.AboutMe.Should().Be("Updated about me");
            user.UpdatedAt.Should().NotBe(initialUpdatedAt);
            user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
    }
}
