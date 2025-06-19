using HexagonalSkeleton.API.Handler;
using HexagonalSkeleton.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Moq;
using System.Text.Json;
using Xunit;

namespace HexagonalSkeleton.Test.Unit.API.Handler
{
    /// <summary>
    /// Unit tests for MinimalExceptionHandler to verify correct HTTP status code mapping
    /// </summary>
    public class MinimalExceptionHandlerTest : IDisposable
    {
        private readonly Mock<ILogger<MinimalExceptionHandler>> _mockLogger;
        private readonly MinimalExceptionHandler _handler;
        private readonly DefaultHttpContext _httpContext;

        public MinimalExceptionHandlerTest()
        {
            _mockLogger = new Mock<ILogger<MinimalExceptionHandler>>();
            _handler = new MinimalExceptionHandler(_mockLogger.Object);
            _httpContext = new DefaultHttpContext();
            _httpContext.Response.Body = new MemoryStream();
        }

        public void Dispose()
        {
            _httpContext.Response.Body?.Dispose();
        }

        [Fact]
        public async Task TryHandleAsync_ValidationException_Returns400BadRequest()
        {
            // Arrange
            var exception = new ValidationException("email", "Email is required");

            // Act
            var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);            // Assert
            result.Should().BeTrue();
            _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            _httpContext.Response.ContentType.Should().Be("application/problem+json");
        }

        [Fact]
        public async Task TryHandleAsync_NotFoundException_Returns404NotFound()
        {
            // Arrange
            var exception = new NotFoundException("User", 123);

            // Act
            var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);            // Assert
            result.Should().BeTrue();
            _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            _httpContext.Response.ContentType.Should().Be("application/problem+json");
        }

        [Fact]
        public async Task TryHandleAsync_AuthenticationException_Returns401Unauthorized()
        {
            // Arrange
            var exception = new AuthenticationException("Invalid credentials");

            // Act
            var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);            // Assert
            result.Should().BeTrue();
            _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            _httpContext.Response.ContentType.Should().Be("application/problem+json");
        }

        [Fact]
        public async Task TryHandleAsync_AuthorizationException_Returns403Forbidden()
        {
            // Arrange
            var exception = new AuthorizationException("You don't have permission");

            // Act
            var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);            // Assert
            result.Should().BeTrue();
            _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
            _httpContext.Response.ContentType.Should().Be("application/problem+json");
        }

        [Fact]
        public async Task TryHandleAsync_ConflictException_Returns409Conflict()
        {
            // Arrange
            var exception = new ConflictException("User already exists");

            // Act
            var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);            // Assert
            result.Should().BeTrue();
            _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
            _httpContext.Response.ContentType.Should().Be("application/problem+json");
        }

        [Fact]
        public async Task TryHandleAsync_BusinessException_Returns422UnprocessableEntity()
        {
            // Arrange
            var exception = new BusinessRuleViolationException("Business rule violated");

            // Act
            var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);            // Assert
            result.Should().BeTrue();
            _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
            _httpContext.Response.ContentType.Should().Be("application/problem+json");
        }

        [Fact]
        public async Task TryHandleAsync_TooManyRequestsException_Returns429TooManyRequests()
        {
            // Arrange
            var exception = new TooManyRequestsException("Rate limit exceeded");

            // Act
            var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);            // Assert
            result.Should().BeTrue();
            _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
            _httpContext.Response.ContentType.Should().Be("application/problem+json");
        }

        [Fact]
        public async Task TryHandleAsync_UnknownException_ReturnsFalse()
        {
            // Arrange
            var exception = new InvalidOperationException("Some other exception");

            // Act
            var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);            // Assert
            result.Should().BeFalse(); // Should let ASP.NET Core handle it
        }

        [Fact]
        public async Task TryHandleAsync_ValidationException_ContainsCorrectProblemDetails()
        {
            // Arrange
            var exception = new ValidationException("email", "Email is required");
            _httpContext.Request.Path = "/api/test";

            // Act
            await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);            // Assert
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseText);
            
            problemDetails.GetProperty("title").GetString().Should().Be("Validation failed");
            problemDetails.GetProperty("status").GetInt32().Should().Be(400);
            problemDetails.GetProperty("instance").GetString().Should().Be("/api/test");
        }
    }
}
