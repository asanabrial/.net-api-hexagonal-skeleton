using HexagonalSkeleton.API.Handler.ExceptionMapping;
using HexagonalSkeleton.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace HexagonalSkeleton.Test.Unit.API.Handler.ExceptionMapping
{
    public class DomainExceptionMapperTest
    {
        private readonly DomainExceptionMapper _mapper;

        public DomainExceptionMapperTest()
        {
            _mapper = new DomainExceptionMapper();
        }        [Fact]
        public void CanHandle_Should_Return_True_For_DomainExceptions()
        {
            // Arrange
            var userDataNotUniqueException = new UserDataNotUniqueException("test@example.com", "+1234567890");
            var weakPasswordException = new WeakPasswordException();
            var insufficientPermissionException = new InsufficientPermissionException("user123", "delete", "document456");

            // Act & Assert
            Assert.True(_mapper.CanHandle(userDataNotUniqueException));
            Assert.True(_mapper.CanHandle(weakPasswordException));
            Assert.True(_mapper.CanHandle(insufficientPermissionException));
        }

        [Fact]
        public void CanHandle_Should_Return_False_For_NonDomainExceptions()
        {
            // Arrange
            var argumentException = new ArgumentException("Not a domain exception");
            var invalidOperationException = new InvalidOperationException("Not a domain exception");

            // Act & Assert
            Assert.False(_mapper.CanHandle(argumentException));
            Assert.False(_mapper.CanHandle(invalidOperationException));
        }

        [Fact]
        public void MapToProblemDetails_Should_Map_UserDataNotUniqueException_To_409_Conflict()
        {
            // Arrange
            var exception = new UserDataNotUniqueException("test@example.com", "+1234567890");
            var requestPath = "/api/user";

            // Act
            var result = _mapper.MapToProblemDetails(exception, requestPath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status409Conflict, result.Status);
            Assert.Equal("Data conflict", result.Title);
            Assert.Equal(exception.Message, result.Detail);
            Assert.Equal(requestPath, result.Instance);
            Assert.True(result.Extensions.ContainsKey("email"));
            Assert.Equal("test@example.com", result.Extensions["email"]);
        }

        [Fact]
        public void MapToProblemDetails_Should_Map_WeakPasswordException_To_400_BadRequest()
        {
            // Arrange
            var exception = new WeakPasswordException();
            var requestPath = "/api/user/password";

            // Act
            var result = _mapper.MapToProblemDetails(exception, requestPath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.Status);
            Assert.Equal("Invalid password", result.Title);
            Assert.Equal("Password does not meet strength requirements", result.Detail);
            Assert.Equal(requestPath, result.Instance);
        }        [Fact]
        public void MapToProblemDetails_Should_Map_GenericDomainException_To_400_BadRequest()
        {
            // Arrange - Use a concrete domain exception
            var exception = new WeakPasswordException();
            var requestPath = "/api/business-operation";

            // Act
            var result = _mapper.MapToProblemDetails(exception, requestPath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.Status);
            Assert.Equal("Invalid password", result.Title);
            Assert.Equal("Password does not meet strength requirements", result.Detail);
            Assert.Equal(requestPath, result.Instance);
        }

        [Fact]
        public void MapToProblemDetails_Should_Throw_ArgumentException_For_Non_Domain_Exception()
        {
            // Arrange
            var exception = new ArgumentException("Not a domain exception");
            var requestPath = "/api/test";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _mapper.MapToProblemDetails(exception, requestPath));
            Assert.Contains("Cannot handle exception of type", ex.Message);
        }

        [Fact]
        public void MapToProblemDetails_Should_Map_InsufficientPermissionException_To_403_Forbidden()
        {
            // Arrange
            var exception = new InsufficientPermissionException("user123", "delete", "document456");
            var requestPath = "/api/documents/456";

            // Act
            var result = _mapper.MapToProblemDetails(exception, requestPath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status403Forbidden, result.Status);
            Assert.Equal("Insufficient permissions", result.Title);
            Assert.Equal(exception.Message, result.Detail);
            Assert.Equal(requestPath, result.Instance);
            Assert.True(result.Extensions.ContainsKey("userId"));
            Assert.Equal("user123", result.Extensions["userId"]);
            Assert.True(result.Extensions.ContainsKey("action"));
            Assert.Equal("delete", result.Extensions["action"]);
            Assert.True(result.Extensions.ContainsKey("resource"));
            Assert.Equal("document456", result.Extensions["resource"]);
        }
    }
}
