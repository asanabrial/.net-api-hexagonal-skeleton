using FluentValidation;
using HexagonalSkeleton.Test.TestHelpers;
using FluentValidation.Results;
using HexagonalSkeleton.Application.Features.UserProfile.Commands;
using HexagonalSkeleton.Application.Features.UserManagement.Commands;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Test.Unit.User.Domain;
using Moq;
using Xunit;
using DomainUser = HexagonalSkeleton.Domain.User;

namespace HexagonalSkeleton.Test.Integration.LogicalDeletion
{
    // Este es realmente un test unitario con mocks - NO necesita Integration Collection
    public class LogicalDeletionBehaviorTest
    {
        private readonly Mock<IValidator<UpdateProfileUserCommand>> _mockUpdateValidator;
        private readonly Mock<IValidator<SoftDeleteUserManagementCommand>> _mockDeleteValidator;
        private readonly Mock<IUserWriteRepository> _mockUserWriteRepository;

        private readonly UpdateProfileUserCommandHandler _updateHandler;
        private readonly SoftDeleteUserManagementCommandHandler _deleteHandler;

        public LogicalDeletionBehaviorTest()
        {
            _mockUpdateValidator = new Mock<IValidator<UpdateProfileUserCommand>>();
            _mockDeleteValidator = new Mock<IValidator<SoftDeleteUserManagementCommand>>();
            _mockUserWriteRepository = new Mock<IUserWriteRepository>();

            _updateHandler = new UpdateProfileUserCommandHandler(
                _mockUpdateValidator.Object,
                _mockUserWriteRepository.Object);
                
            _deleteHandler = new SoftDeleteUserManagementCommandHandler(
                _mockDeleteValidator.Object,
                _mockUserWriteRepository.Object);
        }

        [Fact]
        public async Task UpdateProfile_OnDeletedUser_ShouldThrowDomainException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = UserTestDataBuilder.CreateTestUser(userId);
            
            // Simulate that the user is logically deleted
            user.Delete(); // Esto marca IsDeleted = true
            
            var updateCommand = new UpdateProfileUserCommand(
                id: userId,
                aboutMe: "Updated about me",
                firstName: "Jane",
                lastName: "Smith",
                phoneNumber: "+1234567890",
                birthdate: new DateTime(1985, 5, 15));

            _mockUpdateValidator.Setup(x => x.ValidateAsync(updateCommand, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockUserWriteRepository.Setup(x => x.GetByIdUnfilteredAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HexagonalSkeleton.Domain.Exceptions.UserDomainException>(() =>
                _updateHandler.Handle(updateCommand, CancellationToken.None));

            Assert.Contains("UpdateProfile", exception.Message);
            
            // Verify that the repository was never called to update
            _mockUserWriteRepository.Verify(x => x.UpdateAsync(It.IsAny<DomainUser>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task SoftDelete_OnAlreadyDeletedUser_ShouldStillSucceed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = UserTestDataBuilder.CreateTestUser(userId);
            
            // User is already logically deleted
            user.Delete();
            var initialDeletedAt = user.DeletedAt;
            
            var deleteCommand = new SoftDeleteUserManagementCommand(userId);

            _mockDeleteValidator.Setup(x => x.ValidateAsync(deleteCommand, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockUserWriteRepository.Setup(x => x.GetByIdUnfilteredAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockUserWriteRepository.Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _deleteHandler.Handle(deleteCommand, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(user.IsDeleted);
            
            // The deletion timestamp may have been updated
            Assert.True(user.DeletedAt >= initialDeletedAt);
            
            _mockUserWriteRepository.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProfile_OnActiveUser_ShouldSucceed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = UserTestDataBuilder.CreateTestUser(userId);
            
            // Active user (not deleted)
            Assert.False(user.IsDeleted);
            
            var updateCommand = new UpdateProfileUserCommand(
                id: userId,
                aboutMe: "Updated about me",
                firstName: "Jane",
                lastName: "Smith",
                phoneNumber: "+1234567890",
                birthdate: new DateTime(1985, 5, 15));

            _mockUpdateValidator.Setup(x => x.ValidateAsync(updateCommand, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockUserWriteRepository.Setup(x => x.GetByIdUnfilteredAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockUserWriteRepository.Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act - the handler now runs the real mapping
            var result = await _updateHandler.Handle(updateCommand, CancellationToken.None);

            // Assert - real mapping reflects the updated user state
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal("Jane", result.FirstName);
            Assert.Equal("Smith", result.LastName);
            Assert.Equal("Jane", user.FullName.FirstName);
            Assert.Equal("Smith", user.FullName.LastName);
            Assert.False(user.IsDeleted); // Still active
            
            _mockUserWriteRepository.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerifyCommandRepository_RetrievesDeletedUsers()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = UserTestDataBuilder.CreateTestUser(userId);
            user.Delete(); // Mark as logically deleted
            
            _mockUserWriteRepository.Setup(x => x.GetByIdUnfilteredAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var retrievedUser = await _mockUserWriteRepository.Object.GetByIdUnfilteredAsync(userId, CancellationToken.None);

            // Assert
            Assert.NotNull(retrievedUser);
            Assert.True(retrievedUser.IsDeleted);
            Assert.NotNull(retrievedUser.DeletedAt);
            
            _mockUserWriteRepository.Verify(x => x.GetByIdUnfilteredAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
