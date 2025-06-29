using FluentValidation;
using FluentValidation.Results;
using HexagonalSkeleton.Application.Features.UserManagement.Dto;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Application.Features.UserManagement.Commands;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Test.Unit.User.Domain;
using Moq;
using Xunit;
using DomainUser = HexagonalSkeleton.Domain.User;

namespace HexagonalSkeleton.Test.Application.Features.UserManagement.Commands
{
    public class SoftDeleteUserCommandHandlerTest
    {
        private readonly Mock<IValidator<SoftDeleteUserCommand>> _mockValidator;
        private readonly Mock<IUserReadRepository> _mockUserReadRepository;
        private readonly Mock<IUserWriteRepository> _mockUserWriteRepository;
        private readonly SoftDeleteUserCommandHandler _handler;

        public SoftDeleteUserCommandHandlerTest()
        {
            _mockValidator = new Mock<IValidator<SoftDeleteUserCommand>>();
            _mockUserReadRepository = new Mock<IUserReadRepository>();
            _mockUserWriteRepository = new Mock<IUserWriteRepository>();
            _handler = new SoftDeleteUserCommandHandler(
                _mockValidator.Object,
                _mockUserReadRepository.Object,
                _mockUserWriteRepository.Object);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldReturnSuccessResult()
        {
            // Arrange
            var command = new SoftDeleteUserCommand(Guid.NewGuid());
            var user = TestHelper.CreateTestUser(Guid.NewGuid());

            _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockUserReadRepository.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockUserWriteRepository.Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);            // Assert
            Assert.NotNull(result);

            // Verify the user was soft deleted
            Assert.True(user.IsDeleted);
            Assert.NotNull(user.DeletedAt);

            _mockUserWriteRepository.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        }        [Fact]
        public async Task Handle_WithInvalidCommand_ShouldThrowValidationException()
        {
            // Arrange
            var command = new SoftDeleteUserCommand(Guid.Empty);
            var validationErrors = new List<ValidationFailure>
            {
                new ValidationFailure("Id", "Id cannot be empty")
            };
            var validationResult = new ValidationResult(validationErrors);

            _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HexagonalSkeleton.Application.Exceptions.ValidationException>(() =>
                _handler.Handle(command, CancellationToken.None));            Assert.True(exception.Errors.ContainsKey("Id"));
            Assert.Contains("Id cannot be empty", exception.Errors["Id"]);

            _mockUserReadRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockUserWriteRepository.Verify(x => x.UpdateAsync(It.IsAny<DomainUser>(), It.IsAny<CancellationToken>()), Times.Never);
        }        [Fact]
        public async Task Handle_WithNonExistentUser_ShouldThrowNotFoundException()
        {
            // Arrange
            var command = new SoftDeleteUserCommand(Guid.NewGuid());

            _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockUserReadRepository.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DomainUser?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.Contains("User", exception.Message);
            Assert.Contains(command.Id.ToString(), exception.Message);

            _mockUserWriteRepository.Verify(x => x.UpdateAsync(It.IsAny<DomainUser>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldCallRepositoriesInCorrectOrder()
        {
            // Arrange
            var command = new SoftDeleteUserCommand(Guid.NewGuid());
            var user = TestHelper.CreateTestUser(Guid.NewGuid());

            _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockUserReadRepository.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            var sequence = new MockSequence();
            _mockValidator.InSequence(sequence)
                .Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            
            _mockUserReadRepository.InSequence(sequence)
                .Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            
            _mockUserWriteRepository.InSequence(sequence)
                .Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_WithDifferentValidIds_ShouldSucceed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new SoftDeleteUserCommand(userId);
            var user = TestHelper.CreateTestUser(userId);

            _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockUserReadRepository.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockUserWriteRepository.Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);            // Assert
            Assert.NotNull(result);
            Assert.True(user.IsDeleted);
            _mockUserWriteRepository.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var command = new SoftDeleteUserCommand(Guid.NewGuid());
            var user = TestHelper.CreateTestUser(Guid.NewGuid());

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
        public async Task Handle_ShouldMarkUserAsDeleted()
        {
            // Arrange
            var command = new SoftDeleteUserCommand(Guid.NewGuid());
            var user = TestHelper.CreateTestUser(Guid.NewGuid());
            var initialDeletedAt = user.DeletedAt;

            _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockUserReadRepository.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert            Assert.True(user.IsDeleted);
            Assert.NotEqual(initialDeletedAt, user.DeletedAt);
            Assert.True(user.DeletedAt.HasValue);
            Assert.True(DateTime.UtcNow.Subtract(user.DeletedAt.Value) < TimeSpan.FromSeconds(1));
            Assert.NotNull(user.UpdatedAt);
            Assert.True(user.UpdatedAt.HasValue);
            Assert.True(DateTime.UtcNow.Subtract(user.UpdatedAt.Value) < TimeSpan.FromSeconds(1));
        }
    }
}

