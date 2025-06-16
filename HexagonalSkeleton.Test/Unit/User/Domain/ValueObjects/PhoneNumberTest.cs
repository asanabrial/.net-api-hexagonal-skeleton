using Xunit;
using HexagonalSkeleton.Domain.ValueObjects;

namespace HexagonalSkeleton.Test.Unit.User.Domain.ValueObjects;

public class PhoneNumberTest
{
    [Theory]
    [InlineData("+1234567890")]
    [InlineData("1234567890")]
    [InlineData("+44 20 7946 0958")]
    [InlineData("(555) 123-4567")]
    [InlineData("+34 91 234 56 78")]
    public void Constructor_ValidPhoneNumber_ShouldCreatePhoneNumberSuccessfully(string validPhoneNumber)
    {
        // Act
        var phoneNumber = new PhoneNumber(validPhoneNumber);

        // Assert
        Assert.NotNull(phoneNumber);
        Assert.NotNull(phoneNumber.Value);
        Assert.True(phoneNumber.Value.Length >= 7);
    }    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("123")]
    [InlineData("abc")]
    [InlineData("+")]
    [InlineData("12345678901234567")] // Too long
    public void Constructor_InvalidPhoneNumber_ShouldThrowArgumentException(string invalidPhoneNumber)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PhoneNumber(invalidPhoneNumber));
    }

    [Fact]
    public void Constructor_NullPhoneNumber_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PhoneNumber(null!));
    }

    [Fact]
    public void Constructor_PhoneNumberWithSpecialCharacters_ShouldCleanAndValidate()
    {
        // Arrange
        var phoneWithFormatting = "+1 (555) 123-4567";

        // Act
        var phoneNumber = new PhoneNumber(phoneWithFormatting);

        // Assert
        Assert.NotNull(phoneNumber);
        Assert.Equal("+15551234567", phoneNumber.Value);
    }

    [Fact]
    public void Equals_SamePhoneNumber_ShouldReturnTrue()
    {
        // Arrange
        var phone1 = new PhoneNumber("+1234567890");
        var phone2 = new PhoneNumber("+1234567890");

        // Act & Assert
        Assert.True(phone1.Equals(phone2));
        Assert.Equal(phone1, phone2);
        Assert.Equal(phone1.GetHashCode(), phone2.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentPhoneNumber_ShouldReturnFalse()
    {
        // Arrange
        var phone1 = new PhoneNumber("+1234567890");
        var phone2 = new PhoneNumber("+1234567891");

        // Act & Assert
        Assert.False(phone1.Equals(phone2));
        Assert.NotEqual(phone1, phone2);
    }

    [Fact]
    public void ImplicitConversion_FromString_ShouldWork()
    {
        // Arrange
        string phoneString = "+1234567890";

        // Act
        PhoneNumber phoneNumber = phoneString;

        // Assert
        Assert.NotNull(phoneNumber);
        Assert.Equal(phoneString, phoneNumber.Value);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+1234567890");

        // Act
        string phoneString = phoneNumber;

        // Assert
        Assert.Equal("+1234567890", phoneString);
    }

    [Fact]
    public void ToString_ShouldReturnPhoneNumberValue()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+1234567890");

        // Act
        var result = phoneNumber.ToString();

        // Assert
        Assert.Equal("+1234567890", result);
    }
}
