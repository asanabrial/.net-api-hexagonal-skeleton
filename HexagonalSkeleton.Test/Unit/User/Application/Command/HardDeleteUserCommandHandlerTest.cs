using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using HexagonalSkeleton.Application.Command;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Test.Unit.User.Domain;
using Moq;
using Xunit;
using DomainUser = HexagonalSkeleton.Domain.User;

namespace HexagonalSkeleton.Test.Unit.User.Application.Command
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
            var command = new HardDeleteUserCommand(1);
            var user = TestHelper.CreateTestUser(1);

            _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockUserReadRepository.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockUserWriteRepository.Setup(x => x.DeleteAsync(command.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
            _mockUserWriteRepository.Verify(x => x.DeleteAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);
        }        [Fact]
        public async Task Handle_WithInvalidCommand_ShouldThrowValidationException()
        {
            // Arrange
            var command = new HardDeleteUserCommand(0);
            var validationErrors = new List<ValidationFailure>
            {
                new ValidationFailure("Id", "Id must be greater than 0")
            };
            var validationResult = new ValidationResult(validationErrors);

            _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);            // Act & Assert
            var exception = await Assert.ThrowsAsync<HexagonalSkeleton.Application.Exceptions.ValidationException>(() =>
                _handler.Handle(command, CancellationToken.None));

            exception.Errors.Should().ContainKey("Id");
            exception.Errors["Id"].Should().Contain("Id must be greater than 0");

            _mockUserReadRepository.Verify(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockUserWriteRepository.Verify(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }        [Fact]
        public async Task Handle_WithNonExistentUser_ShouldThrowNotFoundException()
        {
            // Arrange
            var command = new HardDeleteUserCommand(999);

            _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());            _mockUserReadRepository.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DomainUser?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Contain("User");
            exception.Message.Should().Contain("999");

            _mockUserWriteRepository.Verify(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldCallRepositoriesInCorrectOrder()
        {
            // Arrange
            var command = new HardDeleteUserCommand(1);
            var user = TestHelper.CreateTestUser(1);

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
        }        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public async Task Handle_WithDifferentValidIds_ShouldSucceed(int userId)
        {
            // Arrange
            var command = new HardDeleteUserCommand(userId);
            var user = TestHelper.CreateTestUser(userId);

            _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockUserReadRepository.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockUserWriteRepository.Setup(x => x.DeleteAsync(command.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
            _mockUserWriteRepository.Verify(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var command = new HardDeleteUserCommand(1);
            var user = TestHelper.CreateTestUser(1);

            _mockValidator.Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockUserReadRepository.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockUserWriteRepository.Setup(x => x.DeleteAsync(command.Id, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database connection error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Be("Database connection error");
        }
    }
}
