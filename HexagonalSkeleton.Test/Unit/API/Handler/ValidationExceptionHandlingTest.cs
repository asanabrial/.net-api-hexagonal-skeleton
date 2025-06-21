using FluentValidation.Results;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.API.Handler;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace HexagonalSkeleton.Test.Unit.API.Handler
{
    public class ValidationExceptionHandlingTest
    {
        [Fact]
        public async Task MinimalExceptionHandler_WithValidationException_ShouldReturnValidationProblemDetails()
        {
            // Arrange
            var logger = Mock.Of<ILogger<MinimalExceptionHandler>>();
            var handler = new MinimalExceptionHandler(logger);
            
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/api/user";
            httpContext.Response.Body = new MemoryStream();
            
            var validationErrors = new Dictionary<string, string[]>
            {
                { "Email", new[] { "Email is required", "Email format is invalid" } },
                { "Password", new[] { "Password is too short" } }
            };
            
            var validationException = new ValidationException(validationErrors);

            // Act
            var result = await handler.TryHandleAsync(httpContext, validationException, CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Equal(400, httpContext.Response.StatusCode);
            Assert.Equal("application/problem+json", httpContext.Response.ContentType);
              // Read the response body
            httpContext.Response.Body.Position = 0;
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            
            Assert.NotNull(responseBody); Assert.NotEmpty(responseBody);
            
            // Debug: Let's see what's actually in the response
            System.Console.WriteLine($"Response body: {responseBody}");            // Deserialize and verify the structure
            var response = JsonSerializer.Deserialize<JsonElement>(responseBody, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            
            Assert.Equal(400, response.GetProperty("status").GetInt32());
            Assert.Equal("Validation failed", response.GetProperty("title").GetString());
            Assert.Equal("/api/user", response.GetProperty("instance").GetString());
            
            var errorsProperty = response.GetProperty("errors");            Assert.Equal(JsonValueKind.Object, errorsProperty.ValueKind);
            
            Assert.True(errorsProperty.TryGetProperty("Email", out var emailErrors));
            var emailErrorsArray = emailErrors.EnumerateArray().Select(e => e.GetString()).ToArray();
            Assert.Contains("Email is required", emailErrorsArray);
            Assert.Contains("Email format is invalid", emailErrorsArray);
            
            Assert.True(errorsProperty.TryGetProperty("Password", out var passwordErrors));
            var passwordErrorsArray = passwordErrors.EnumerateArray().Select(e => e.GetString()).ToArray();
            Assert.Contains("Password is too short", passwordErrorsArray);
        }

        [Fact]
        public async Task MinimalExceptionHandler_WithSingleFieldValidationException_ShouldReturnFormattedErrors()
        {
            // Arrange
            var logger = Mock.Of<ILogger<MinimalExceptionHandler>>();
            var handler = new MinimalExceptionHandler(logger);
            
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/api/user";
            httpContext.Response.Body = new MemoryStream();
            
            var validationException = new ValidationException("password", "Password does not meet strength requirements");

            // Act
            var result = await handler.TryHandleAsync(httpContext, validationException, CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Equal(400, httpContext.Response.StatusCode);
            
            // Read the response body
            httpContext.Response.Body.Position = 0;
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();            // Deserialize and verify the structure
            var response = JsonSerializer.Deserialize<JsonElement>(responseBody, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            
            Assert.Equal(400, response.GetProperty("status").GetInt32());
            Assert.Equal("Validation failed", response.GetProperty("title").GetString());
            
            var errorsProperty = response.GetProperty("errors");
            Assert.True(errorsProperty.TryGetProperty("password", out var passwordErrors));
            var passwordErrorsArray = passwordErrors.EnumerateArray().Select(e => e.GetString()).ToArray();
            Assert.Contains("Password does not meet strength requirements", passwordErrorsArray);
        }
    }
}
