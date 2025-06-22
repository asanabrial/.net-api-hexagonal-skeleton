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
            
            Assert.Equal("appSettings", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullUserReadRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                new AuthenticationService(_mockAppSettings.Object, null!));
            
            Assert.Equal("userReadRepository", exception.ParamName);
        }

        [Fact]        public async Task GenerateJwtTokenAsync_WithValidUserId_ShouldReturnToken()
        {
            // Arrange
            var userId = 1;
            var user = CreateTestUser(userId);
            _mockUserReadRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var tokenInfo = await _authenticationService.GenerateJwtTokenAsync(userId);

            // Assert
            Assert.NotNull(tokenInfo);
            Assert.NotNull(tokenInfo.Token);
            Assert.NotEmpty(tokenInfo.Token);
            Assert.Contains(".", tokenInfo.Token);
            Assert.True(tokenInfo.ExpiresIn > 0);
            Assert.True(tokenInfo.ExpiresAt > DateTime.UtcNow);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GenerateJwtTokenAsync_WithInvalidUserId_ShouldThrowArgumentException(int userId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _authenticationService.GenerateJwtTokenAsync(userId));
            
            Assert.Equal("userId", exception.ParamName);
            Assert.Contains("User ID must be greater than zero", exception.Message);
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
            
            Assert.Equal("userId", exception.ParamName);
            Assert.Contains("User not found", exception.Message);
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
            
            Assert.Contains("Cannot generate token for deleted user", exception.Message);
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
            Assert.True(result);
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
            Assert.False(result);
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
            Assert.False(result);
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
            Assert.False(result);
        }        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ValidateCredentialsAsync_WithInvalidEmail_ShouldThrowArgumentException(string email)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _authenticationService.ValidateCredentialsAsync(email, "password"));
            
            Assert.Equal("email", exception.ParamName);
            Assert.Contains("Email cannot be null or empty", exception.Message);
        }

        [Fact]
        public async Task ValidateCredentialsAsync_WithNullEmail_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _authenticationService.ValidateCredentialsAsync(null!, "password"));
            
            Assert.Equal("email", exception.ParamName);
            Assert.Contains("Email cannot be null or empty", exception.Message);
        }        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ValidateCredentialsAsync_WithInvalidPassword_ShouldThrowArgumentException(string password)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _authenticationService.ValidateCredentialsAsync("test@example.com", password));
            
            Assert.Equal("password", exception.ParamName);
            Assert.Contains("Password cannot be null or empty", exception.Message);
        }

        [Fact]
        public async Task ValidateCredentialsAsync_WithNullPassword_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _authenticationService.ValidateCredentialsAsync("test@example.com", null!));
            
            Assert.Equal("password", exception.ParamName);
            Assert.Contains("Password cannot be null or empty", exception.Message);
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
            Assert.NotNull(hashedPassword); Assert.NotEmpty(hashedPassword);
            Assert.NotEqual(password, hashedPassword);
        }        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void HashPassword_WithInvalidPassword_ShouldThrowArgumentException(string password)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _authenticationService.HashPassword(password, "salt"));
            
            Assert.Equal("password", exception.ParamName);
            Assert.Contains("Password cannot be null or empty", exception.Message);
        }

        [Fact]
        public void HashPassword_WithNullPassword_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _authenticationService.HashPassword(null!, "salt"));
            
            Assert.Equal("password", exception.ParamName);
            Assert.Contains("Password cannot be null or empty", exception.Message);
        }        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void HashPassword_WithInvalidSalt_ShouldThrowArgumentException(string salt)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _authenticationService.HashPassword("password", salt));
            
            Assert.Equal("salt", exception.ParamName);
            Assert.Contains("Salt cannot be null or empty", exception.Message);
        }

        [Fact]
        public void HashPassword_WithNullSalt_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _authenticationService.HashPassword("password", null!));
            
            Assert.Equal("salt", exception.ParamName);
            Assert.Contains("Salt cannot be null or empty", exception.Message);
        }        [Fact]
        public void HashPassword_WithNullPepper_ShouldThrowInvalidOperationException()
        {
            // Arrange
            string? nullPepper = null;
            _mockAppSettings.Setup(x => x.Pepper).Returns(nullPepper!);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                _authenticationService.HashPassword("password", "salt"));
            
            Assert.Contains("Pepper configuration is required for security", exception.Message);
        }

        [Fact]
        public void GenerateSalt_ShouldReturnNonEmptyString()
        {
            // Act
            var salt = _authenticationService.GenerateSalt();

            // Assert
            Assert.NotNull(salt); Assert.NotEmpty(salt);
        }

        [Fact]
        public void GenerateSalt_ShouldReturnDifferentValuesOnMultipleCalls()
        {
            // Act
            var salt1 = _authenticationService.GenerateSalt();
            var salt2 = _authenticationService.GenerateSalt();

            // Assert
            Assert.NotEqual(salt2, salt1);
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
            Assert.Equal(hash2, hash1);
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
