using Xunit;
using HexagonalSkeleton.Domain.ValueObjects;

namespace HexagonalSkeleton.Test.Unit.User.Domain.ValueObjects;

public class FullNameTest
{
    [Theory]
    [InlineData("John", "Doe")]
    [InlineData("María", "González")]
    [InlineData("Jean-Pierre", "De La Cruz")]
    [InlineData("A", "B")]
    public void Constructor_ValidNames_ShouldCreateFullNameSuccessfully(string firstName, string lastName)
    {
        // Act
        var fullName = new FullName(firstName, lastName);

        // Assert
        Assert.NotNull(fullName);
        Assert.Equal(firstName.Trim(), fullName.FirstName);
        Assert.Equal(lastName.Trim(), fullName.LastName);
    }

    [Theory]
    [InlineData("", "Doe")]
    [InlineData(" ", "Doe")]
    [InlineData("John", "")]
    [InlineData("John", " ")]
    public void Constructor_InvalidNames_ShouldThrowArgumentException(string firstName, string lastName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new FullName(firstName, lastName));
    }

    [Fact]
    public void Constructor_NullFirstName_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new FullName(null!, "Doe"));
    }

    [Fact]
    public void Constructor_NullLastName_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new FullName("John", null!));
    }

    [Theory]
    [InlineData("John", "Doe", "John Doe")]
    [InlineData("María José", "García López", "María José García López")]
    public void GetFullName_ShouldReturnFormattedName(string firstName, string lastName, string expected)
    {
        // Arrange
        var fullName = new FullName(firstName, lastName);

        // Act
        var result = fullName.GetFullName();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("John", "Doe", "JD")]
    [InlineData("maría", "gonzález", "MG")]
    [InlineData("jean-pierre", "de la cruz", "JD")]
    public void GetInitials_ShouldReturnUppercaseInitials(string firstName, string lastName, string expected)
    {
        // Arrange
        var fullName = new FullName(firstName, lastName);

        // Act
        var result = fullName.GetInitials();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Constructor_TrimWhitespace_ShouldTrimNamesCorrectly()
    {
        // Arrange
        var firstNameWithSpaces = "  John  ";
        var lastNameWithSpaces = "  Doe  ";

        // Act
        var fullName = new FullName(firstNameWithSpaces, lastNameWithSpaces);

        // Assert
        Assert.Equal("John", fullName.FirstName);
        Assert.Equal("Doe", fullName.LastName);
    }

    [Fact]
    public void Constructor_NamesExceedLength_ShouldThrowArgumentException()
    {
        // Arrange
        var longName = new string('A', 101);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new FullName(longName, "Doe"));
        Assert.Throws<ArgumentException>(() => new FullName("John", longName));
    }

    [Fact]
    public void Equals_SameFullName_ShouldReturnTrue()
    {
        // Arrange
        var fullName1 = new FullName("John", "Doe");
        var fullName2 = new FullName("John", "Doe");

        // Act & Assert
        Assert.True(fullName1.Equals(fullName2));
        Assert.Equal(fullName1, fullName2);
        Assert.Equal(fullName1.GetHashCode(), fullName2.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentFullName_ShouldReturnFalse()
    {
        // Arrange
        var fullName1 = new FullName("John", "Doe");
        var fullName2 = new FullName("Jane", "Doe");

        // Act & Assert
        Assert.False(fullName1.Equals(fullName2));
        Assert.NotEqual(fullName1, fullName2);
    }

    [Fact]
    public void ToString_ShouldReturnFullName()
    {
        // Arrange
        var fullName = new FullName("John", "Doe");

        // Act
        var result = fullName.ToString();

        // Assert
        Assert.Equal("John Doe", result);
    }
}
