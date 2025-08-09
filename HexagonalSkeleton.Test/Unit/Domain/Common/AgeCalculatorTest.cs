using HexagonalSkeleton.Domain.Common;

namespace HexagonalSkeleton.Test.Unit.Domain.Common;

/// <summary>
/// Unit tests for AgeCalculator utility class
/// Validates age calculation logic for various scenarios
/// </summary>
public class AgeCalculatorTest
{
    [Fact]
    public void CalculateAge_WithBirthdayAlreadyPassed_ShouldReturnCorrectAge()
    {
        // Arrange
        var birthdate = new DateTime(1990, 1, 1);
        var referenceDate = new DateTime(2023, 6, 15);

        // Act
        var age = AgeCalculator.CalculateAge(birthdate, referenceDate);

        // Assert
        Assert.Equal(33, age);
    }

    [Fact]
    public void CalculateAge_WithBirthdayNotYetPassed_ShouldReturnCorrectAge()
    {
        // Arrange
        var birthdate = new DateTime(1990, 12, 25);
        var referenceDate = new DateTime(2023, 6, 15);

        // Act
        var age = AgeCalculator.CalculateAge(birthdate, referenceDate);

        // Assert
        Assert.Equal(32, age);
    }

    [Fact]
    public void CalculateAge_OnExactBirthday_ShouldReturnCorrectAge()
    {
        // Arrange
        var birthdate = new DateTime(1990, 6, 15);
        var referenceDate = new DateTime(2023, 6, 15);

        // Act
        var age = AgeCalculator.CalculateAge(birthdate, referenceDate);

        // Assert
        Assert.Equal(33, age);
    }

    [Fact]
    public void CalculateAge_WithoutReferenceDate_ShouldUseToday()
    {
        // Arrange
        var birthdate = DateTime.Today.AddYears(-25);

        // Act
        var age = AgeCalculator.CalculateAge(birthdate);

        // Assert
        Assert.Equal(25, age);
    }

    [Fact]
    public void IsAtLeastAge_WhenPersonIsOldEnough_ShouldReturnTrue()
    {
        // Arrange
        var birthdate = new DateTime(2000, 1, 1);
        var referenceDate = new DateTime(2023, 6, 15);

        // Act
        var result = AgeCalculator.IsAtLeastAge(birthdate, 18, referenceDate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAtLeastAge_WhenPersonIsNotOldEnough_ShouldReturnFalse()
    {
        // Arrange
        var birthdate = new DateTime(2010, 1, 1);
        var referenceDate = new DateTime(2023, 6, 15);

        // Act
        var result = AgeCalculator.IsAtLeastAge(birthdate, 18, referenceDate);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsWithinAgeRange_WhenPersonIsWithinRange_ShouldReturnTrue()
    {
        // Arrange
        var birthdate = new DateTime(1995, 1, 1);
        var referenceDate = new DateTime(2023, 6, 15);

        // Act
        var result = AgeCalculator.IsWithinAgeRange(birthdate, 18, 35, referenceDate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsWithinAgeRange_WhenPersonIsBelowRange_ShouldReturnFalse()
    {
        // Arrange
        var birthdate = new DateTime(2010, 1, 1);
        var referenceDate = new DateTime(2023, 6, 15);

        // Act
        var result = AgeCalculator.IsWithinAgeRange(birthdate, 18, 35, referenceDate);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsWithinAgeRange_WhenPersonIsAboveRange_ShouldReturnFalse()
    {
        // Arrange
        var birthdate = new DateTime(1980, 1, 1);
        var referenceDate = new DateTime(2023, 6, 15);

        // Act
        var result = AgeCalculator.IsWithinAgeRange(birthdate, 18, 35, referenceDate);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAtLeastAge_ForMinimumValidationAge_ShouldReturnCorrectResult()
    {
        // Arrange - Test for 13 years old minimum requirement
        var birthdate = new DateTime(2010, 6, 15);
        var referenceDate = new DateTime(2023, 6, 16); // One day after 13th birthday

        // Act
        var result = AgeCalculator.IsAtLeastAge(birthdate, 13, referenceDate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAtLeastAge_OneDayBeforeRequiredAge_ShouldReturnFalse()
    {
        // Arrange - Test for 13 years old minimum requirement
        var birthdate = new DateTime(2010, 6, 15);
        var referenceDate = new DateTime(2023, 6, 14); // One day before 13th birthday

        // Act
        var result = AgeCalculator.IsAtLeastAge(birthdate, 13, referenceDate);

        // Assert
        Assert.False(result);
    }
}
