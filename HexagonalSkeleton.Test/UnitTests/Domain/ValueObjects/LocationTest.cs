using Xunit;
using HexagonalSkeleton.Domain.ValueObjects;

namespace HexagonalSkeleton.Test.Unit.User.Domain.ValueObjects;

public class LocationTest
{
    [Theory]
    [InlineData(40.7128, -74.0060)] // New York
    [InlineData(51.5074, -0.1278)]  // London
    [InlineData(-33.8688, 151.2093)] // Sydney
    [InlineData(0, 0)] // Equator and Prime Meridian
    [InlineData(90, 180)] // North Pole, max longitude
    [InlineData(-90, -180)] // South Pole, min longitude
    public void Constructor_ValidCoordinates_ShouldCreateLocationSuccessfully(double latitude, double longitude)
    {
        // Act
        var location = new Location(latitude, longitude);

        // Assert
        Assert.NotNull(location);
        Assert.Equal(latitude, location.Latitude);
        Assert.Equal(longitude, location.Longitude);
    }

    [Theory]
    [InlineData(91, 0)] // Latitude too high
    [InlineData(-91, 0)] // Latitude too low
    [InlineData(0, 181)] // Longitude too high
    [InlineData(0, -181)] // Longitude too low
    [InlineData(100, 200)] // Both out of range
    public void Constructor_InvalidCoordinates_ShouldThrowArgumentException(double latitude, double longitude)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Location(latitude, longitude));
    }

    [Fact]
    public void CalculateDistanceTo_SameLocation_ShouldReturnZero()
    {
        // Arrange
        var location1 = new Location(40.7128, -74.0060);
        var location2 = new Location(40.7128, -74.0060);

        // Act
        var distance = location1.CalculateDistanceTo(location2);

        // Assert
        Assert.Equal(0, distance, 1); // Allow 1km tolerance for floating point precision
    }

    [Fact]
    public void CalculateDistanceTo_DifferentLocations_ShouldReturnCorrectDistance()
    {
        // Arrange
        var newYork = new Location(40.7128, -74.0060);
        var london = new Location(51.5074, -0.1278);

        // Act
        var distance = newYork.CalculateDistanceTo(london);

        // Assert
        // The distance between NYC and London is approximately 5545 km
        Assert.True(distance > 5500 && distance < 5600, $"Expected distance around 5545km, but got {distance}km");
    }

    [Fact]
    public void CalculateDistanceTo_NullLocation_ShouldThrowArgumentNullException()
    {
        // Arrange
        var location = new Location(40.7128, -74.0060);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => location.CalculateDistanceTo(null!));
    }

    [Fact]
    public void Equals_SameCoordinates_ShouldReturnTrue()
    {
        // Arrange
        var location1 = new Location(40.7128, -74.0060);
        var location2 = new Location(40.7128, -74.0060);

        // Act & Assert
        Assert.True(location1.Equals(location2));
        Assert.Equal(location1, location2);
        Assert.Equal(location1.GetHashCode(), location2.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentCoordinates_ShouldReturnFalse()
    {
        // Arrange
        var location1 = new Location(40.7128, -74.0060);
        var location2 = new Location(51.5074, -0.1278);

        // Act & Assert
        Assert.False(location1.Equals(location2));
        Assert.NotEqual(location1, location2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedCoordinates()
    {
        // Arrange
        var location = new Location(40.7128, -74.0060);

        // Act
        var result = location.ToString();

        // Assert
        Assert.Contains("40.7128", result);
        Assert.Contains("-74.0060", result);
    }
}
