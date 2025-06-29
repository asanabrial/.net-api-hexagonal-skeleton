using FluentValidation;
using FluentValidation.Results;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Application.Features.UserManagement.Commands;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Test.Unit.User.Domain;
using Moq;
using Xunit;
using DomainUser = HexagonalSkeleton.Domain.User;

namespace HexagonalSkeleton.Test.Application.Features.UserManagement.Commands
{
    public class HardDeleteUserCommandHandlerTest
    {
        private readonly Mock<IValidator<HardDeleteUserCommand>> _mockValidator;
        private readonly Mock<IUserReadRepository> _mockUserReadRepository;
        private readonly Mock<IUserWriteRepository> _mockUserWriteRepository;
        private readonly HardDeleteUserCommandHandler _handler;

        public HardDeleteUserCommandHandlerTest()
        {
            _mockValidator = new Mock<IValidator<HardDeleteUserCommand>>();
            _mockUserReadRepository = new Mock<IUserReadRepository>();
            _mockUserWriteRepository = new Mock<IUserWriteRepository>();
            _handler = new HardDeleteUserCommandHandler(
                _mockValidator.Object,
                _mockUserReadRepository.Object,
                _mockUserWriteRepository.Object);
        }        [Fact]
        public async Task Handle_WithValidCommand_ShouldCompleteSuccessfully()
        {
            // Arrange
            var command = new HardDeleteUserCommand(Guid.NewGuid());
            var user = TestHelper.CreateTestUser(Guid.NewGuid());

            _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockUserReadRepository.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockUserWriteRepository.Setup(x => x.DeleteAsync(command.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);            // Assert
            var exception = await Record.ExceptionAsync(act);
            Assert.Null(exception);
            _mockUserWriteRepository.Verify(x => x.DeleteAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);
        }        [Fact]
        public async Task Handle_WithInvalidCommand_ShouldThrowValidationException()
        {
            // Arrange
            var command = new HardDeleteUserCommand(Guid.Empty);
            var validationErrors = new List<ValidationFailure>
            {
                new ValidationFailure("Id", "Id cannot be empty")
            };
            var validationResult = new ValidationResult(validationErrors);

            _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);            // Act & Assert
            var exception = await Assert.ThrowsAsync<HexagonalSkeleton.Application.Exceptions.ValidationException>(() =>
                _handler.Handle(command, CancellationToken.None));            Assert.True(exception.Errors.ContainsKey("Id"));
            Assert.Contains("Id cannot be empty", exception.Errors["Id"]);

            _mockUserReadRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockUserWriteRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }        [Fact]
        public async Task Handle_WithNonExistentUser_ShouldThrowNotFoundException()
        {
            // Arrange
            var command = new HardDeleteUserCommand(Guid.NewGuid());

            _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());            _mockUserReadRepository.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DomainUser?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.Contains("User", exception.Message);
            Assert.Contains(command.Id.ToString(), exception.Message);

            _mockUserWriteRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldCallRepositoriesInCorrectOrder()
        {
            // Arrange
            var command = new HardDeleteUserCommand(Guid.NewGuid());
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
                .Setup(x => x.DeleteAsync(command.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }        [Fact]
        public async Task Handle_WithDifferentValidIds_ShouldSucceed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new HardDeleteUserCommand(userId);
            var user = TestHelper.CreateTestUser(userId);

            _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockUserReadRepository.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockUserWriteRepository.Setup(x => x.DeleteAsync(command.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);            // Assert
            var exception = await Record.ExceptionAsync(act);
            Assert.Null(exception);
            _mockUserWriteRepository.Verify(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var command = new HardDeleteUserCommand(Guid.NewGuid());
            var user = TestHelper.CreateTestUser(Guid.NewGuid());

            _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockUserReadRepository.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockUserWriteRepository.Setup(x => x.DeleteAsync(command.Id, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database connection error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.Equal("Database connection error", exception.Message);
        }
    }
}

