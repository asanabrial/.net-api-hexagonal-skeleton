using HexagonalSkeleton.Application.Exceptions;
using Xunit;

namespace HexagonalSkeleton.Test.Unit.Application.Exceptions
{    /// <summary>
    /// Unit tests for custom exceptions to verify they are properly constructed
    /// </summary>
    public class CustomExceptionsTest
    {
        [Fact]
        public void NotFoundException_WithEntityAndId_ContainsCorrectMessage()
        {
            // Arrange
            const string entityName = "User";
            const int entityId = 123;

            // Act
            var exception = new NotFoundException(entityName, entityId);            // Assert
            Assert.Contains("User", exception.Message);
            Assert.Contains("123", exception.Message);
            Assert.Contains("not found", exception.Message.ToLower());
        }

        [Fact]
        public void ValidationException_WithMultipleErrors_ContainsAllErrors()
        {
            // Arrange
            var errors = new Dictionary<string, string[]>
            {
                { "email", new[] { "Email is required", "Email format is invalid" } },
                { "password", new[] { "Password is too short" } }
            };

            // Act
            var exception = new ValidationException(errors);

            // Assert
            Assert.Equal(2, exception.Errors.Count);
            Assert.Equal(2, exception.Errors["email"].Length);
            Assert.Single(exception.Errors["password"]);
        }

        [Fact]
        public void AuthenticationException_WithMessage_InheritsFromInvalidCredentialException()
        {
            // Arrange
            const string message = "Invalid credentials";

            // Act
            var exception = new AuthenticationException(message);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.IsAssignableFrom<System.Security.Authentication.InvalidCredentialException>(exception);
        }

        [Fact]
        public void BusinessRuleViolationException_InheritsFromBusinessException()
        {
            // Arrange
            const string message = "Business rule violated";

            // Act
            var exception = new BusinessRuleViolationException(message);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.IsAssignableFrom<BusinessException>(exception);
        }
    }
}
