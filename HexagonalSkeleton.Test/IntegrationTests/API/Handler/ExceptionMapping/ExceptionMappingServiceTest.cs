using HexagonalSkeleton.API.Handler.ExceptionMapping;
using HexagonalSkeleton.Domain.Exceptions;
using HexagonalSkeleton.Application.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HexagonalSkeleton.Test.Unit.API.Handler.ExceptionMapping
{
    public class ExceptionMappingServiceTest
    {
        private readonly Mock<ILogger<ExceptionMappingService>> _mockLogger;
        private readonly ExceptionMappingService _service;
        private readonly List<IExceptionMapper> _mappers;

        public ExceptionMappingServiceTest()
        {
            _mockLogger = new Mock<ILogger<ExceptionMappingService>>();
            _mappers = new List<IExceptionMapper>
            {
                new DomainExceptionMapper(),
                new ApplicationExceptionMapper(),
                new ValidationExceptionMapper()
            };
            _service = new ExceptionMappingService(_mappers, _mockLogger.Object);
        }

        [Fact]
        public void TryMapToProblemDetails_Should_Return_Null_For_Unhandled_Exception()
        {
            // Arrange
            var exception = new InvalidOperationException("Unhandled exception");
            var requestPath = "/api/test";

            // Act
            var result = _service.TryMapToProblemDetails(exception, requestPath);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void TryMapToProblemDetails_Should_Use_Correct_Mapper_For_Domain_Exception()
        {
            // Arrange
            var exception = new UserDataNotUniqueException("test@example.com", "+1234567890");
            var requestPath = "/api/user";

            // Act
            var result = _service.TryMapToProblemDetails(exception, requestPath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(409, result.Status);
            Assert.Equal("Data conflict", result.Title);
        }

        [Fact]
        public void TryMapToProblemDetails_Should_Use_Correct_Mapper_For_Application_Exception()
        {
            // Arrange
            var exception = new NotFoundException("Resource not found");
            var requestPath = "/api/resource/123";

            // Act
            var result = _service.TryMapToProblemDetails(exception, requestPath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.Status);
            Assert.Equal("Resource not found", result.Title);
        }

        [Fact]
        public void TryMapToProblemDetails_Should_Use_First_Matching_Mapper()
        {
            // Arrange - Create a service with duplicate mappers to test priority
            var duplicateMappers = new List<IExceptionMapper>
            {
                new DomainExceptionMapper(),
                new DomainExceptionMapper() // Duplicate to test first-match behavior
            };
            var serviceWithDuplicates = new ExceptionMappingService(duplicateMappers, _mockLogger.Object);
            var exception = new UserDataNotUniqueException("test@example.com", "+1234567890");
            var requestPath = "/api/user";

            // Act
            var result = serviceWithDuplicates.TryMapToProblemDetails(exception, requestPath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(409, result.Status);
        }

        [Fact]
        public void CanHandle_Should_Return_True_When_Any_Mapper_Can_Handle()
        {
            // Arrange
            var domainException = new UserDataNotUniqueException("test@example.com", "+1234567890");
            var applicationException = new NotFoundException("Not found");

            // Act & Assert
            Assert.True(_service.CanHandle(domainException));
            Assert.True(_service.CanHandle(applicationException));
        }

        [Fact]
        public void CanHandle_Should_Return_False_When_No_Mapper_Can_Handle()
        {
            // Arrange
            var unhandledException = new InvalidOperationException("Unhandled");

            // Act & Assert
            Assert.False(_service.CanHandle(unhandledException));
        }
    }
}
