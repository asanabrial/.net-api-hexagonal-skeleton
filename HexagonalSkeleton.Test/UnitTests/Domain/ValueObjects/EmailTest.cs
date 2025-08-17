using Xunit;
using HexagonalSkeleton.Domain.ValueObjects;

namespace HexagonalSkeleton.Test.Unit.User.Domain.ValueObjects;

public class EmailTest
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("first+last@example.org")]
    [InlineData("123@example.com")]
    public void Constructor_ValidEmail_ShouldCreateEmailSuccessfully(string validEmail)
    {
        // Act
        var email = new Email(validEmail);

        // Assert
        Assert.NotNull(email);
        Assert.Equal(validEmail.ToLowerInvariant(), email.Value);
    }    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-email")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user@@domain.com")]
    public void Constructor_InvalidEmail_ShouldThrowArgumentException(string invalidEmail)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Email(invalidEmail));
    }

    [Fact]
    public void Constructor_NullEmail_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Email(null!));
    }

    [Fact]
    public void Equals_SameEmail_ShouldReturnTrue()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("TEST@EXAMPLE.COM");

        // Act & Assert
        Assert.True(email1.Equals(email2));
        Assert.Equal(email1, email2);
        Assert.Equal(email1.GetHashCode(), email2.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentEmail_ShouldReturnFalse()
    {
        // Arrange
        var email1 = new Email("test1@example.com");
        var email2 = new Email("test2@example.com");

        // Act & Assert
        Assert.False(email1.Equals(email2));
        Assert.NotEqual(email1, email2);
    }

    [Fact]
    public void ToString_ShouldReturnEmailValue()
    {
        // Arrange
        var email = new Email("test@example.com");

        // Act
        var result = email.ToString();

        // Assert
        Assert.Equal("test@example.com", result);
    }
}
