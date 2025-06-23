using FluentValidation;
using FluentValidation.Results;
using HexagonalSkeleton.Application.Command;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Test.Unit.User.Domain;
using Moq;
using Xunit;
using AutoMapper;
using DomainUser = HexagonalSkeleton.Domain.User;

namespace HexagonalSkeleton.Test.Unit.User.Application.Command
{    public class UpdateProfileUserCommandHandlerTest
    {
        private readonly Mock<IValidator<UpdateProfileUserCommand>> _mockValidator;
        private readonly Mock<IUserReadRepository> _mockUserReadRepository;
        private readonly Mock<IUserWriteRepository> _mockUserWriteRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly UpdateProfileUserCommandHandler _handler;

        public UpdateProfileUserCommandHandlerTest()
        {
            _mockValidator = new Mock<IValidator<UpdateProfileUserCommand>>();
            _mockUserReadRepository = new Mock<IUserReadRepository>();
            _mockUserWriteRepository = new Mock<IUserWriteRepository>();
            _mockMapper = new Mock<IMapper>();
            _handler = new UpdateProfileUserCommandHandler(
                _mockValidator.Object,
                _mockUserReadRepository.Object,
                _mockUserWriteRepository.Object,
                _mockMapper.Object);
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
                .ReturnsAsync(user);            _mockUserWriteRepository.Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);            var expectedResult = new UserDto
            {
                Id = user.Id,
                FirstName = "Jane",
                LastName = "Smith",
                Birthdate = new DateTime(1985, 5, 15),
                Email = user.Email.Value,
                LastLogin = user.LastLogin
            };

            _mockMapper.Setup(x => x.Map<UserDto>(It.IsAny<DomainUser>()))
                .Returns(expectedResult);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);

            // Verify the user profile was updated
            Assert.Equal("Jane", user.FullName.FirstName);
            Assert.Equal("Smith", user.FullName.LastName);
            Assert.Equal(new DateTime(1985, 5, 15), user.Birthdate);
            Assert.Equal("Updated about me", user.AboutMe);

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
                .ReturnsAsync(validationResult);            // Act & Assert
            var exception = await Assert.ThrowsAsync<HexagonalSkeleton.Application.Exceptions.ValidationException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.True(exception.Errors.ContainsKey("Id"));
            Assert.True(exception.Errors.ContainsKey("FirstName"));
            Assert.True(exception.Errors.ContainsKey("LastName"));

            _mockUserReadRepository.Verify(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockUserWriteRepository.Verify(x => x.UpdateAsync(It.IsAny<DomainUser>(), It.IsAny<CancellationToken>()), Times.Never);
        }        [Fact]
        public async Task Handle_WithNonExistentUser_ShouldThrowNotFoundException()
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

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.Contains("User", exception.Message);
            Assert.Contains("999", exception.Message);

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
                .ReturnsAsync(user);            _mockUserWriteRepository.Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);            var expectedResult = new UserDto
            {
                Id = user.Id,
                FirstName = firstName,
                LastName = lastName,
                Birthdate = new DateTime(1990, 1, 1),
                Email = user.Email.Value,
                LastLogin = user.LastLogin
            };

            _mockMapper.Setup(x => x.Map<UserDto>(It.IsAny<DomainUser>()))
                .Returns(expectedResult);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);            // Assert
            Assert.NotNull(result);
            Assert.Equal(firstName, user.FullName.FirstName);
            Assert.Equal(lastName, user.FullName.LastName);
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

            Assert.Equal("Database connection error", exception.Message);
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
            Assert.Equal("Jane", user.FullName.FirstName);
            Assert.Equal("Smith", user.FullName.LastName);
            Assert.Equal(new DateTime(1985, 5, 15), user.Birthdate);
            Assert.Equal("Updated about me", user.AboutMe);            Assert.NotEqual(initialUpdatedAt, user.UpdatedAt);
            Assert.True(user.UpdatedAt.HasValue);
            Assert.True(DateTime.UtcNow.Subtract(user.UpdatedAt.Value) < TimeSpan.FromSeconds(1));
        }
    }
}
