using FluentAssertions;
using HexagonalSkeleton.CommonCore.Auth;
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
            hash.Should().NotBeNullOrEmpty();
            hash.Should().NotBe(password);
            hash.Should().NotBe(salt);
            hash.Should().NotBe(pepper);
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
            hash1.Should().Be(hash2);
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
            hash1.Should().NotBe(hash2);
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
            hash1.Should().NotBe(hash2);
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
            hash1.Should().NotBe(hash2);
        }

        [Theory]
        [InlineData("", "salt", "pepper")]
        [InlineData("password", "", "pepper")]
        [InlineData("password", "salt", "")]
        public void ComputeHash_WithEmptyInputs_ShouldStillReturnHash(string password, string salt, string pepper)
        {
            // Act
            var hash = PasswordHasher.ComputeHash(password, salt, pepper);

            // Assert
            hash.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void ComputeHash_WithSpecialCharacters_ShouldWork()
        {
            // Arrange
            var password = "p@ssw0rd!#$%";
            var salt = "s@lt&*()";
            var pepper = "p3pp3r<>?";

            // Act
            var hash = PasswordHasher.ComputeHash(password, salt, pepper);

            // Assert
            hash.Should().NotBeNullOrEmpty();
            hash.Should().NotContain(password);
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
            hash.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void GenerateSalt_ShouldReturnNonEmptyString()
        {
            // Act
            var salt = PasswordHasher.GenerateSalt();

            // Assert
            salt.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void GenerateSalt_ShouldReturnDifferentValuesOnMultipleCalls()
        {
            // Act
            var salt1 = PasswordHasher.GenerateSalt();
            var salt2 = PasswordHasher.GenerateSalt();
            var salt3 = PasswordHasher.GenerateSalt();

            // Assert
            salt1.Should().NotBe(salt2);
            salt2.Should().NotBe(salt3);
            salt1.Should().NotBe(salt3);
        }

        [Fact]
        public void GenerateSalt_ShouldReturnBase64EncodedString()
        {
            // Act
            var salt = PasswordHasher.GenerateSalt();

            // Assert
            salt.Should().NotBeNullOrEmpty();
            // Should be able to convert back from Base64 without throwing
            var action = () => Convert.FromBase64String(salt);
            action.Should().NotThrow();
        }

        [Fact]
        public void GenerateSalt_ShouldReturnConsistentLength()
        {
            // Act
            var salt1 = PasswordHasher.GenerateSalt();
            var salt2 = PasswordHasher.GenerateSalt();
            var salt3 = PasswordHasher.GenerateSalt();

            // Assert
            salt1.Length.Should().Be(salt2.Length);
            salt2.Length.Should().Be(salt3.Length);
            // Base64 encoding of 16 bytes should be 24 characters (including padding)
            salt1.Length.Should().Be(24);
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
            hash.Should().NotBeNullOrEmpty();
            // Should be able to convert back from Base64 without throwing
            var action = () => Convert.FromBase64String(hash!);
            action.Should().NotThrow();
        }
    }
}
