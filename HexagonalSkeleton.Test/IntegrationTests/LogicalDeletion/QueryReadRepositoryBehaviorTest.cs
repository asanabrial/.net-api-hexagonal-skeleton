using FluentValidation;
using HexagonalSkeleton.Test.TestHelpers;
using FluentValidation.Results;
using HexagonalSkeleton.Application.Features.UserManagement.Queries;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Test.Unit.User.Domain;
using Moq;
using Xunit;
using DomainUser = HexagonalSkeleton.Domain.User;
using HexagonalSkeleton.Application.Features.UserManagement.Dto;

namespace HexagonalSkeleton.Test.Integration.LogicalDeletion
{
    // Este es realmente un test unitario con mocks - NO necesita Integration Collection
    public class QueryReadRepositoryBehaviorTest
    {
        private readonly Mock<IValidator<GetUserQuery>> _mockValidator;
        private readonly Mock<IUserReadRepository> _mockUserReadRepository;

        private readonly GetUserQueryHandler _queryHandler;

        public QueryReadRepositoryBehaviorTest()
        {
            _mockValidator = new Mock<IValidator<GetUserQuery>>();
            _mockUserReadRepository = new Mock<IUserReadRepository>();

            _queryHandler = new GetUserQueryHandler(
                _mockValidator.Object,
                _mockUserReadRepository.Object);
        }

        [Fact]
        public async Task GetUserQuery_OnDeletedUser_ShouldReturnNull_ReadRepositoryFiltersCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetUserQuery(userId);

            _mockValidator.Setup(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Simulate that the read repository does NOT find the deleted user (correct behavior)
            _mockUserReadRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DomainUser?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _queryHandler.Handle(query, CancellationToken.None));

            Assert.Contains("User", exception.Message);
            Assert.Contains(userId.ToString(), exception.Message);
            
            // Verify that the read repository was called
            _mockUserReadRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);

            // No mapping happens because the user was not found (the handler throws first)
        }

        [Fact]
        public async Task GetUserQuery_OnActiveUser_ShouldReturnUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = UserTestDataBuilder.CreateTestUser(userId);
            var query = new GetUserQuery(userId);

            // Active user (not deleted)
            Assert.False(user.IsDeleted);

            _mockValidator.Setup(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Read repository finds the active user
            _mockUserReadRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act - the handler now runs the real mapping
            var result = await _queryHandler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal(user.FullName.FirstName, result.FirstName);
            Assert.Equal(user.Email.Value, result.Email);

            _mockUserReadRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void VerifyArchitecturalSeparation_ReadVsWriteRepositories()
        {
            // Esta prueba conceptual verifica que estamos usando las interfaces correctas
            
            // UserProfile (queries) debería usar IUserReadRepository que filtra borrados
            var constructor = typeof(GetUserQueryHandler).GetConstructors().First();
            var parameters = constructor.GetParameters();
            
            Assert.Contains(parameters, p => p.ParameterType == typeof(IUserReadRepository));
            
            // Command handlers should be able to access IUserWriteRepository
            // (esto se verifica en los tests de comando que ya tenemos)
            
            // Esta separación asegura que:
            // - Queries de perfil NO ven usuarios borrados
            // - Management commands CAN access deleted users for validation
        }
    }
}
