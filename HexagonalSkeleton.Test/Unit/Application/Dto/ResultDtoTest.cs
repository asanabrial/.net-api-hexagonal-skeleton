using Xunit;
using HexagonalSkeleton.Application.Dto;

namespace HexagonalSkeleton.Test.Unit.Application.Dto;

public class ResultDtoTest
{
    [Fact]
    public void Constructor_WithDictionary_ShouldCreateInvalidResult()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email is required" } },
            { "Password", new[] { "Password is too short" } }
        };

        // Act
        var result = new ResultDto(errors);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains("Email", result.Errors.Keys);
        Assert.Contains("Password", result.Errors.Keys);
        Assert.Null(result.Data);
    }

    [Fact]
    public void Constructor_WithStringError_ShouldCreateInvalidResult()
    {
        // Arrange
        var errorMessage = "Something went wrong";

        // Act
        var result = new ResultDto(errorMessage);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains("Error", result.Errors.Keys);
        Assert.Contains(errorMessage, result.Errors["Error"]);
        Assert.Null(result.Data);
    }

    [Fact]
    public void Constructor_WithData_ShouldCreateValidResult()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test" };

        // Act
        var result = new ResultDto(data);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.NotNull(result.Data);
        Assert.Equal(data, result.Data);
    }

    [Fact]
    public void Constructor_WithNullData_ShouldCreateValidResult()
    {
        // Arrange & Act
        var result = new ResultDto((object?)null);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Null(result.Data);
    }

    [Fact]
    public void IsValid_EmptyErrors_ShouldReturnTrue()
    {
        // Arrange
        var result = new ResultDto(new Dictionary<string, string[]>());

        // Act & Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void IsValid_WithErrors_ShouldReturnFalse()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Field", new[] { "Error message" } }
        };
        var result = new ResultDto(errors);

        // Act & Assert
        Assert.False(result.IsValid);
    }
}
