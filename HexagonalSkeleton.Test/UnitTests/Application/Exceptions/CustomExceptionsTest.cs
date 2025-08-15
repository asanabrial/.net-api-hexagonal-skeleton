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
        public void NotFoundException_WithCustomMessage_ContainsMessage()
        {
            // Arrange
            const string message = "Custom not found message";

            // Act
            var exception = new NotFoundException(message);            // Assert
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void ValidationException_WithFieldAndMessage_ContainsErrors()
        {
            // Arrange
            const string field = "email";
            const string message = "Email is required";

            // Act
            var exception = new ValidationException(field, message);

            // Assert
            Assert.True(exception.Errors.ContainsKey(field));
            Assert.Contains(message, exception.Errors[field]);
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
        public void AuthorizationException_WithMessage_InheritsFromUnauthorizedAccessException()
        {
            // Arrange
            const string message = "Access denied";

            // Act
            var exception = new AuthorizationException(message);

            // Assert
            Assert.Equal(message, exception.Message);            Assert.IsAssignableFrom<UnauthorizedAccessException>(exception);
        }

        [Fact]
        public void ConflictException_WithMessage_ContainsMessage()
        {
            // Arrange
            const string message = "Resource already exists";

            // Act
            var exception = new ConflictException(message);

            // Assert
            Assert.Equal(message, exception.Message);
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

        [Fact]
        public void TooManyRequestsException_WithMessage_ContainsMessage()
        {
            // Arrange
            const string message = "Rate limit exceeded";

            // Act
            var exception = new TooManyRequestsException(message);

            // Assert
            Assert.Equal(message, exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Custom message")]
        public void NotFoundException_WithVariousMessages_DoesNotThrow(string message)
        {
            // Act
            var act = () => new NotFoundException(message);

            // Assert
            // Test passes if no exception is thrown
        }

        [Fact]
        public void ValidationException_WithGeneralMessage_ContainsGeneralError()
        {
            // Arrange
            const string message = "Validation failed";

            // Act
            var exception = new ValidationException(message);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.True(exception.Errors.ContainsKey("General"));
            Assert.Contains(message, exception.Errors["General"]);
        }

        [Fact]
        public void ValidationException_WithNullErrors_InitializesEmptyDictionary()
        {
            // Act
            var exception = new ValidationException((IDictionary<string, string[]>)null!);

            // Assert
            Assert.NotNull(exception.Errors);
            Assert.Empty(exception.Errors);
        }
    }
}
