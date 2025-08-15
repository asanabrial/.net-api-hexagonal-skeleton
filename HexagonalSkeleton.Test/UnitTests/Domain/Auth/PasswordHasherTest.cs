using HexagonalSkeleton.Infrastructure.Auth;
using Xunit;

namespace HexagonalSkeleton.Test.Unit.CommonCore.Auth
{
    public class PasswordHasherTest
    {
        [Fact]
        public void ComputeHash_WithValidInputs_ShouldReturnHash()
        {
            // Arrange
            var password = "testPassword";
            var salt = "testSalt";
            var pepper = "testPepper";

            // Act
            var hash = PasswordHasher.ComputeHash(password, salt, pepper);

            // Assert
            Assert.NotNull(hash); Assert.NotEmpty(hash);
            Assert.NotEqual(password, hash);
            Assert.NotEqual(salt, hash);
            Assert.NotEqual(pepper, hash);
        }

        [Fact]
        public void ComputeHash_WithSameInputs_ShouldReturnSameHash()
        {
            // Arrange
            var password = "testPassword";
            var salt = "testSalt";
            var pepper = "testPepper";

            // Act
            var hash1 = PasswordHasher.ComputeHash(password, salt, pepper);
            var hash2 = PasswordHasher.ComputeHash(password, salt, pepper);

            // Assert
            Assert.Equal(hash2, hash1);
        }

        [Fact]
        public void ComputeHash_WithDifferentPasswords_ShouldReturnDifferentHashes()
        {
            // Arrange
            var salt = "testSalt";
            var pepper = "testPepper";
            var password1 = "password1";
            var password2 = "password2";

            // Act
            var hash1 = PasswordHasher.ComputeHash(password1, salt, pepper);
            var hash2 = PasswordHasher.ComputeHash(password2, salt, pepper);

            // Assert
            Assert.NotEqual(hash2, hash1);
        }

        [Fact]
        public void ComputeHash_WithDifferentSalts_ShouldReturnDifferentHashes()
        {
            // Arrange
            var password = "testPassword";
            var pepper = "testPepper";
            var salt1 = "salt1";
            var salt2 = "salt2";

            // Act
            var hash1 = PasswordHasher.ComputeHash(password, salt1, pepper);
            var hash2 = PasswordHasher.ComputeHash(password, salt2, pepper);

            // Assert
            Assert.NotEqual(hash2, hash1);
        }

        [Fact]
        public void ComputeHash_WithDifferentPeppers_ShouldReturnDifferentHashes()
        {
            // Arrange
            var password = "testPassword";
            var salt = "testSalt";
            var pepper1 = "pepper1";
            var pepper2 = "pepper2";

            // Act
            var hash1 = PasswordHasher.ComputeHash(password, salt, pepper1);
            var hash2 = PasswordHasher.ComputeHash(password, salt, pepper2);

            // Assert
            Assert.NotEqual(hash2, hash1);
        }        [Theory]
        [InlineData("", "salt", "pepper")]
        [InlineData("password", "", "pepper")]
        public void ComputeHash_WithEmptyInputs_ShouldThrowArgumentException(string password, string salt, string pepper)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => PasswordHasher.ComputeHash(password, salt, pepper));
        }

        [Theory]
        [InlineData("password", "salt", "")]
        public void ComputeHash_WithEmptyPepper_ShouldStillReturnHash(string password, string salt, string pepper)
        {
            // Act
            var hash = PasswordHasher.ComputeHash(password, salt, pepper);

            // Assert
            Assert.NotNull(hash); Assert.NotEmpty(hash);
        }

        [Fact]
        public void ComputeHash_WithSpecialCharacters_ShouldWork()
        {
            // Arrange
            var password = "p@ssw0rd!#$%";
            var salt = "s@lt&*()";
            var pepper = "p3pp3r<>?";

            // Act
            var hash = PasswordHasher.ComputeHash(password, salt, pepper);            // Assert
            Assert.NotNull(hash); Assert.NotEmpty(hash);
            Assert.DoesNotContain(password, hash);
        }

        [Fact]
        public void ComputeHash_WithUnicodeCharacters_ShouldWork()
        {
            // Arrange
            var password = "contraseña";
            var salt = "salé";
            var pepper = "pimiénta";

            // Act
            var hash = PasswordHasher.ComputeHash(password, salt, pepper);

            // Assert
            Assert.NotNull(hash); Assert.NotEmpty(hash);
        }

        [Fact]
        public void GenerateSalt_ShouldReturnNonEmptyString()
        {
            // Act
            var salt = PasswordHasher.GenerateSalt();

            // Assert
            Assert.NotNull(salt); Assert.NotEmpty(salt);
        }

        [Fact]
        public void GenerateSalt_ShouldReturnDifferentValuesOnMultipleCalls()
        {
            // Act
            var salt1 = PasswordHasher.GenerateSalt();
            var salt2 = PasswordHasher.GenerateSalt();
            var salt3 = PasswordHasher.GenerateSalt();

            // Assert
            Assert.NotEqual(salt2, salt1);
            Assert.NotEqual(salt3, salt2);
            Assert.NotEqual(salt3, salt1);
        }

        [Fact]
        public void GenerateSalt_ShouldReturnBase64EncodedString()
        {
            // Act
            var salt = PasswordHasher.GenerateSalt();

            // Assert
            Assert.NotNull(salt);
            Assert.NotEmpty(salt);
            // Should be able to convert back from Base64 without throwing
            var bytes = Record.Exception(() => Convert.FromBase64String(salt));
            Assert.Null(bytes);
        }

        [Fact]
        public void GenerateSalt_ShouldReturnConsistentLength()
        {
            // Act
            var salt1 = PasswordHasher.GenerateSalt();
            var salt2 = PasswordHasher.GenerateSalt();
            var salt3 = PasswordHasher.GenerateSalt();

            // Assert
            Assert.Equal(salt2.Length, salt1.Length);
            Assert.Equal(salt3.Length, salt2.Length);
            // Base64 encoding of 16 bytes should be 24 characters (including padding)
            Assert.Equal(24, salt1.Length);
        }

        [Fact]
        public void ComputeHash_ShouldReturnBase64EncodedString()
        {
            // Arrange
            var password = "testPassword";
            var salt = "testSalt";
            var pepper = "testPepper";

            // Act
            var hash = PasswordHasher.ComputeHash(password, salt, pepper);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
            // Should be able to convert back from Base64 without throwing
            var exception = Record.Exception(() => Convert.FromBase64String(hash!));
            Assert.Null(exception);
        }
    }
}
