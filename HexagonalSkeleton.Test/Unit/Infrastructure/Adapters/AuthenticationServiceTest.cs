using FluentAssertions;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain.ValueObjects;
using HexagonalSkeleton.Infrastructure.Adapters;
using Moq;
using Xunit;
using DomainUser = HexagonalSkeleton.Domain.User;

namespace HexagonalSkeleton.Test.Unit.Infrastructure.Adapters
{
    public class AuthenticationServiceTest
    {
        private readonly Mock<IApplicationSettings> _mockAppSettings;
        private readonly Mock<IUserReadRepository> _mockUserReadRepository;
        private readonly AuthenticationService _authenticationService;

        public AuthenticationServiceTest()
        {
            _mockAppSettings = new Mock<IApplicationSettings>();
            _mockUserReadRepository = new Mock<IUserReadRepository>();
            _authenticationService = new AuthenticationService(_mockAppSettings.Object, _mockUserReadRepository.Object);

            // Setup common app settings
            _mockAppSettings.Setup(x => x.Secret).Returns("MyVeryLongSecretKeyThatIsAtLeast32Characters");
            _mockAppSettings.Setup(x => x.Issuer).Returns("TestIssuer");
            _mockAppSettings.Setup(x => x.Audience).Returns("TestAudience");
            _mockAppSettings.Setup(x => x.Pepper).Returns("TestPepper");
        }

        [Fact]
        public void Constructor_WithNullAppSettings_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                new AuthenticationService(null!, _mockUserReadRepository.Object));
            
            exception.ParamName.Should().Be("appSettings");
        }

        [Fact]
        public void Constructor_WithNullUserReadRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                new AuthenticationService(_mockAppSettings.Object, null!));
            
            exception.ParamName.Should().Be("userReadRepository");
        }

        [Fact]
        public async Task GenerateJwtTokenAsync_WithValidUserId_ShouldReturnToken()
        {
            // Arrange
            var userId = 1;
            var user = CreateTestUser(userId);
            _mockUserReadRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var token = await _authenticationService.GenerateJwtTokenAsync(userId);

            // Assert
            token.Should().NotBeNullOrEmpty();
            token.Should().Contain(".");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GenerateJwtTokenAsync_WithInvalidUserId_ShouldThrowArgumentException(int userId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _authenticationService.GenerateJwtTokenAsync(userId));
            
            exception.ParamName.Should().Be("userId");
            exception.Message.Should().Contain("User ID must be greater than zero");
        }        [Fact]
        public async Task GenerateJwtTokenAsync_WithNonExistentUser_ShouldThrowArgumentException()
        {
            // Arrange
            var userId = 999;
            _mockUserReadRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DomainUser?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _authenticationService.GenerateJwtTokenAsync(userId));
            
            exception.ParamName.Should().Be("userId");
            exception.Message.Should().Contain("User not found");
        }

        [Fact]
        public async Task GenerateJwtTokenAsync_WithDeletedUser_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var userId = 1;
            var user = CreateTestUser(userId);
            user.Delete(); // Mark as deleted
            
            _mockUserReadRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _authenticationService.GenerateJwtTokenAsync(userId));
            
            exception.Message.Should().Contain("Cannot generate token for deleted user");
        }

        [Fact]
        public async Task ValidateCredentialsAsync_WithValidCredentials_ShouldReturnTrue()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password123";
            var salt = _authenticationService.GenerateSalt();
            var hashedPassword = _authenticationService.HashPassword(password, salt);
            
            var user = CreateTestUser(1);            // Use reflection to set password properties since they're protected
            var passwordHashProperty = typeof(DomainUser).GetProperty("PasswordHash");
            var passwordSaltProperty = typeof(DomainUser).GetProperty("PasswordSalt");
            passwordHashProperty?.SetValue(user, hashedPassword);
            passwordSaltProperty?.SetValue(user, salt);

            _mockUserReadRepository.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _authenticationService.ValidateCredentialsAsync(email, password);

            // Assert
            result.Should().BeTrue();
        }        [Fact]
        public async Task ValidateCredentialsAsync_WithInvalidPassword_ShouldReturnFalse()
        {
            // Arrange
            var email = "test@example.com";
            var password = "wrongpassword";
            var salt = _authenticationService.GenerateSalt();
            var hashedPassword = _authenticationService.HashPassword("correctpassword", salt);
            
            var user = CreateTestUser(1);
            var passwordHashProperty = typeof(DomainUser).GetProperty("PasswordHash");
            var passwordSaltProperty = typeof(DomainUser).GetProperty("PasswordSalt");
            passwordHashProperty?.SetValue(user, hashedPassword);
            passwordSaltProperty?.SetValue(user, salt);

            _mockUserReadRepository.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _authenticationService.ValidateCredentialsAsync(email, password);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateCredentialsAsync_WithNonExistentUser_ShouldReturnFalse()
        {
            // Arrange
            var email = "nonexistent@example.com";
            var password = "password123";            _mockUserReadRepository.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DomainUser?)null);

            // Act
            var result = await _authenticationService.ValidateCredentialsAsync(email, password);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateCredentialsAsync_WithDeletedUser_ShouldReturnFalse()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password123";
            
            var user = CreateTestUser(1);
            user.Delete(); // Mark as deleted

            _mockUserReadRepository.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _authenticationService.ValidateCredentialsAsync(email, password);

            // Assert
            result.Should().BeFalse();
        }        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ValidateCredentialsAsync_WithInvalidEmail_ShouldThrowArgumentException(string email)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _authenticationService.ValidateCredentialsAsync(email, "password"));
            
            exception.ParamName.Should().Be("email");
            exception.Message.Should().Contain("Email cannot be null or empty");
        }

        [Fact]
        public async Task ValidateCredentialsAsync_WithNullEmail_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _authenticationService.ValidateCredentialsAsync(null!, "password"));
            
            exception.ParamName.Should().Be("email");
            exception.Message.Should().Contain("Email cannot be null or empty");
        }        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ValidateCredentialsAsync_WithInvalidPassword_ShouldThrowArgumentException(string password)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _authenticationService.ValidateCredentialsAsync("test@example.com", password));
            
            exception.ParamName.Should().Be("password");
            exception.Message.Should().Contain("Password cannot be null or empty");
        }

        [Fact]
        public async Task ValidateCredentialsAsync_WithNullPassword_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _authenticationService.ValidateCredentialsAsync("test@example.com", null!));
            
            exception.ParamName.Should().Be("password");
            exception.Message.Should().Contain("Password cannot be null or empty");
        }

        [Fact]
        public void HashPassword_WithValidInputs_ShouldReturnHashedPassword()
        {
            // Arrange
            var password = "testPassword";
            var salt = "testSalt";

            // Act
            var hashedPassword = _authenticationService.HashPassword(password, salt);

            // Assert
            hashedPassword.Should().NotBeNullOrEmpty();
            hashedPassword.Should().NotBe(password);
        }        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void HashPassword_WithInvalidPassword_ShouldThrowArgumentException(string password)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _authenticationService.HashPassword(password, "salt"));
            
            exception.ParamName.Should().Be("password");
            exception.Message.Should().Contain("Password cannot be null or empty");
        }

        [Fact]
        public void HashPassword_WithNullPassword_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _authenticationService.HashPassword(null!, "salt"));
            
            exception.ParamName.Should().Be("password");
            exception.Message.Should().Contain("Password cannot be null or empty");
        }        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void HashPassword_WithInvalidSalt_ShouldThrowArgumentException(string salt)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _authenticationService.HashPassword("password", salt));
            
            exception.ParamName.Should().Be("salt");
            exception.Message.Should().Contain("Salt cannot be null or empty");
        }

        [Fact]
        public void HashPassword_WithNullSalt_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _authenticationService.HashPassword("password", null!));
            
            exception.ParamName.Should().Be("salt");
            exception.Message.Should().Contain("Salt cannot be null or empty");
        }        [Fact]
        public void HashPassword_WithNullPepper_ShouldThrowInvalidOperationException()
        {
            // Arrange
            string? nullPepper = null;
            _mockAppSettings.Setup(x => x.Pepper).Returns(nullPepper!);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                _authenticationService.HashPassword("password", "salt"));
            
            exception.Message.Should().Contain("Pepper configuration is required for security");
        }

        [Fact]
        public void GenerateSalt_ShouldReturnNonEmptyString()
        {
            // Act
            var salt = _authenticationService.GenerateSalt();

            // Assert
            salt.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void GenerateSalt_ShouldReturnDifferentValuesOnMultipleCalls()
        {
            // Act
            var salt1 = _authenticationService.GenerateSalt();
            var salt2 = _authenticationService.GenerateSalt();

            // Assert
            salt1.Should().NotBe(salt2);
        }

        [Fact]
        public void HashPassword_WithSameInputs_ShouldReturnSameHash()
        {
            // Arrange
            var password = "testPassword";
            var salt = "testSalt";

            // Act
            var hash1 = _authenticationService.HashPassword(password, salt);
            var hash2 = _authenticationService.HashPassword(password, salt);

            // Assert
            hash1.Should().Be(hash2);
        }        private static DomainUser CreateTestUser(int id)
        {
            return DomainUser.Reconstitute(
                id: id,
                email: "test@example.com",
                firstName: "John",
                lastName: "Doe",
                birthdate: new DateTime(1990, 1, 1),
                phoneNumber: "+1234567890",
                latitude: 40.7128,
                longitude: -74.0060,
                aboutMe: "Test user",
                passwordSalt: "testSalt",
                passwordHash: "testHash",
                lastLogin: DateTime.UtcNow,
                createdAt: DateTime.UtcNow,
                updatedAt: null,
                deletedAt: null,
                isDeleted: false
            );
        }
    }
}
