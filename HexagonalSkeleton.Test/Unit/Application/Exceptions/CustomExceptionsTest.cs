using HexagonalSkeleton.Application.Exceptions;
using FluentAssertions;
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
            var exception = new NotFoundException(entityName, entityId);

            // Assert
            exception.Message.Should().Contain("User");
            exception.Message.Should().Contain("123");
            exception.Message.ToLower().Should().Contain("not found");
        }

        [Fact]
        public void NotFoundException_WithCustomMessage_ContainsMessage()
        {
            // Arrange
            const string message = "Custom not found message";

            // Act
            var exception = new NotFoundException(message);            // Assert
            exception.Message.Should().Be(message);
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
            exception.Errors.Should().ContainKey(field);
            exception.Errors[field].Should().Contain(message);
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
            exception.Errors.Should().HaveCount(2);
            exception.Errors["email"].Should().HaveCount(2);            exception.Errors["password"].Should().ContainSingle();
        }

        [Fact]
        public void AuthenticationException_WithMessage_InheritsFromInvalidCredentialException()
        {
            // Arrange
            const string message = "Invalid credentials";

            // Act
            var exception = new AuthenticationException(message);

            // Assert
            exception.Message.Should().Be(message);
            exception.Should().BeAssignableTo<System.Security.Authentication.InvalidCredentialException>();
        }

        [Fact]
        public void AuthorizationException_WithMessage_InheritsFromUnauthorizedAccessException()
        {
            // Arrange
            const string message = "Access denied";

            // Act
            var exception = new AuthorizationException(message);

            // Assert
            exception.Message.Should().Be(message);            exception.Should().BeAssignableTo<UnauthorizedAccessException>();
        }

        [Fact]
        public void ConflictException_WithMessage_ContainsMessage()
        {
            // Arrange
            const string message = "Resource already exists";

            // Act
            var exception = new ConflictException(message);

            // Assert
            exception.Message.Should().Be(message);
        }

        [Fact]
        public void BusinessRuleViolationException_InheritsFromBusinessException()
        {
            // Arrange
            const string message = "Business rule violated";

            // Act
            var exception = new BusinessRuleViolationException(message);

            // Assert
            exception.Message.Should().Be(message);
            exception.Should().BeAssignableTo<BusinessException>();
        }

        [Fact]
        public void TooManyRequestsException_WithMessage_ContainsMessage()
        {
            // Arrange
            const string message = "Rate limit exceeded";

            // Act
            var exception = new TooManyRequestsException(message);

            // Assert
            exception.Message.Should().Be(message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Custom message")]
        public void NotFoundException_WithVariousMessages_DoesNotThrow(string message)
        {
            // Act
            var act = () => new NotFoundException(message);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void ValidationException_WithGeneralMessage_ContainsGeneralError()
        {
            // Arrange
            const string message = "Validation failed";

            // Act
            var exception = new ValidationException(message);

            // Assert
            exception.Message.Should().Be(message);
            exception.Errors.Should().ContainKey("General");
            exception.Errors["General"].Should().Contain(message);
        }

        [Fact]
        public void ValidationException_WithNullErrors_InitializesEmptyDictionary()
        {
            // Act
            var exception = new ValidationException((IDictionary<string, string[]>)null!);

            // Assert
            exception.Errors.Should().NotBeNull();
            exception.Errors.Should().BeEmpty();
        }
    }
}
