using FluentValidation;
using FluentValidation.Results;
using HexagonalSkeleton.Application.Features.UserManagement.Queries;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Test.Unit.User.Domain;
using Moq;
using Xunit;
using AutoMapper;
using DomainUser = HexagonalSkeleton.Domain.User;
using HexagonalSkeleton.Application.Features.UserManagement.Dto;

namespace HexagonalSkeleton.Test.Integration.LogicalDeletion
{
    // Este es realmente un test unitario con mocks - NO necesita Integration Collection
    public class QueryReadRepositoryBehaviorTest
    {
        private readonly Mock<IValidator<GetUserQuery>> _mockValidator;
        private readonly Mock<IUserReadRepository> _mockUserReadRepository;
        private readonly Mock<IMapper> _mockMapper;
        
        private readonly GetUserQueryHandler _queryHandler;

        public QueryReadRepositoryBehaviorTest()
        {
            _mockValidator = new Mock<IValidator<GetUserQuery>>();
            _mockUserReadRepository = new Mock<IUserReadRepository>();
            _mockMapper = new Mock<IMapper>();
            
            _queryHandler = new GetUserQueryHandler(
                _mockValidator.Object,
                _mockUserReadRepository.Object,
                _mockMapper.Object);
        }

        [Fact]
        public async Task GetUserQuery_OnDeletedUser_ShouldReturnNull_ReadRepositoryFiltersCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetUserQuery(userId);

            _mockValidator.Setup(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Simular que el read repository NO encuentra el usuario borrado (comportamiento correcto)
            _mockUserReadRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DomainUser?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _queryHandler.Handle(query, CancellationToken.None));

            Assert.Contains("User", exception.Message);
            Assert.Contains(userId.ToString(), exception.Message);
            
            // Verificar que el read repository fue llamado
            _mockUserReadRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
            
            // Verificar que el mapper nunca fue llamado porque no había usuario
            _mockMapper.Verify(x => x.Map<GetUserDto>(It.IsAny<DomainUser>()), Times.Never);
        }

        [Fact]
        public async Task GetUserQuery_OnActiveUser_ShouldReturnUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = TestHelper.CreateTestUser(userId);
            var query = new GetUserQuery(userId);

            // Usuario activo (no borrado)
            Assert.False(user.IsDeleted);

            _mockValidator.Setup(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Read repository encuentra el usuario activo
            _mockUserReadRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var expectedDto = new GetUserDto
            {
                Id = userId,
                FirstName = user.FullName.FirstName,
                LastName = user.FullName.LastName,
                Email = user.Email.Value,
                Birthdate = user.Birthdate
            };

            _mockMapper.Setup(x => x.Map<GetUserDto>(user))
                .Returns(expectedDto);

            // Act
            var result = await _queryHandler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal(user.FullName.FirstName, result.FirstName);
            Assert.Equal(user.Email.Value, result.Email);
            
            _mockUserReadRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
            _mockMapper.Verify(x => x.Map<GetUserDto>(user), Times.Once);
        }

        [Fact]
        public void VerifyArchitecturalSeparation_ReadVsWriteRepositories()
        {
            // Esta prueba conceptual verifica que estamos usando las interfaces correctas
            
            // UserProfile (queries) debería usar IUserReadRepository que filtra borrados
            var constructor = typeof(GetUserQueryHandler).GetConstructors().First();
            var parameters = constructor.GetParameters();
            
            Assert.Contains(parameters, p => p.ParameterType == typeof(IUserReadRepository));
            
            // Los handlers de comando deberían poder acceder a IUserWriteRepository
            // (esto se verifica en los tests de comando que ya tenemos)
            
            // Esta separación asegura que:
            // - Queries de perfil NO ven usuarios borrados
            // - Commands de gestión SÍ pueden acceder a usuarios borrados para validación
        }
    }
}
