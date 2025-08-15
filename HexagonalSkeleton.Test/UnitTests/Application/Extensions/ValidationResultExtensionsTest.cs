using FluentValidation.Results;
using HexagonalSkeleton.Application.Extensions;
using Xunit;

namespace HexagonalSkeleton.Test.Unit.Application.Extensions
{
    public class ValidationResultExtensionsTest
    {
        [Fact]
        public void ToDictionary_WithMultipleErrors_ShouldGroupByPropertyName()
        {
            // Arrange
            var validationResult = new ValidationResult();
            validationResult.Errors.Add(new ValidationFailure("Email", "Email is required"));
            validationResult.Errors.Add(new ValidationFailure("Email", "Email format is invalid"));
            validationResult.Errors.Add(new ValidationFailure("Password", "Password is too short"));
            validationResult.Errors.Add(new ValidationFailure("Name", "Name is required"));

            // Act
            var result = validationResult.ToDictionary();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.True(result.ContainsKey("Email"));
            Assert.True(result.ContainsKey("Password"));
            Assert.True(result.ContainsKey("Name"));

            Assert.Equal(2, result["Email"].Length);
            Assert.Contains("Email is required", result["Email"]);
            Assert.Contains("Email format is invalid", result["Email"]);

            Assert.Single(result["Password"]);
            Assert.Contains("Password is too short", result["Password"]);

            Assert.Single(result["Name"]);
            Assert.Contains("Name is required", result["Name"]);
        }        [Fact]
        public void ToDictionary_WithEmptyValidationResult_ShouldReturnEmptyDictionary()
        {
            // Arrange
            var validationResult = new ValidationResult();

            // Act
            var result = validationResult.ToDictionary();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void ToDictionary_WithSingleError_ShouldReturnSingleKeyValuePair()
        {
            // Arrange
            var validationResult = new ValidationResult();
            validationResult.Errors.Add(new ValidationFailure("TestField", "Test error message"));

            // Act
            var result = validationResult.ToDictionary();

            // Assert
            Assert.Single(result);
            Assert.True(result.ContainsKey("TestField"));
            Assert.Single(result["TestField"]);
            Assert.Contains("Test error message", result["TestField"]);
        }
    }
}
