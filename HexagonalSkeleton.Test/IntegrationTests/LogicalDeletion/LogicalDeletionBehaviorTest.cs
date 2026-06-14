using FluentValidation;
using HexagonalSkeleton.Test.TestHelpers;
using FluentValidation.Results;
using HexagonalSkeleton.Application.Features.UserProfile.Commands;
using HexagonalSkeleton.Domain.Ports;
using Moq;
using Xunit;
using DomainUser = HexagonalSkeleton.Domain.User;

namespace HexagonalSkeleton.Test.Integration.LogicalDeletion
{
    // Este es realmente un test unitario con mocks - NO necesita Integration Collection
    public class LogicalDeletionBehaviorTest
    {
        private readonly Mock<IValidator<UpdateProfileUserCommand>> _mockUpdateValidator;
        private readonly Mock<IUserWriteRepository> _mockUserWriteRepository;

        private readonly UpdateProfileUserCommandHandler _updateHandler;

        public LogicalDeletionBehaviorTest()
        {
            _mockUpdateValidator = new Mock<IValidator<UpdateProfileUserCommand>>();
            _mockUserWriteRepository = new Mock<IUserWriteRepository>();

            _updateHandler = new UpdateProfileUserCommandHandler(
                _mockUpdateValidator.Object,
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
    }
}
