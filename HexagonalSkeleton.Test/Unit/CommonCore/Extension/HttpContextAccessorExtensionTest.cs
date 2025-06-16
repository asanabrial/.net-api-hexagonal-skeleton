using FluentAssertions;
using HexagonalSkeleton.CommonCore.Extension;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace HexagonalSkeleton.Test.Unit.CommonCore.Extension
{
    public class HttpContextAccessorExtensionTest
    {
        [Fact]
        public void GetApiBaseUrl_WithNullAccessor_ShouldReturnNull()
        {
            // Arrange
            IHttpContextAccessor? nullAccessor = null;

            // Act
            var result = nullAccessor!.GetApiBaseUrl();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetApiBaseUrl_WithNullHttpContext_ShouldReturnNull()
        {
            // Arrange
            var mockAccessor = new Mock<IHttpContextAccessor>();
            mockAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

            // Act
            var result = mockAccessor.Object.GetApiBaseUrl();

            // Assert
            result.Should().BeNull();
        }        [Fact]
        public void GetApiBaseUrl_WithStandardHttpsPort_ShouldIncludePort()
        {
            // Arrange
            var mockAccessor = new Mock<IHttpContextAccessor>();
            var mockContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();

            mockRequest.Setup(x => x.Scheme).Returns("https");
            mockRequest.Setup(x => x.Host).Returns(new HostString("api.example.com")); // No port specified
            mockRequest.Setup(x => x.PathBase).Returns(new PathString("/api/v1"));

            mockContext.Setup(x => x.Request).Returns(mockRequest.Object);
            mockAccessor.Setup(x => x.HttpContext).Returns(mockContext.Object);

            // Act
            var result = mockAccessor.Object.GetApiBaseUrl();

            // Assert
            result.Should().Be("https://api.example.com/api/v1");
        }

        [Fact]
        public void GetApiBaseUrl_WithValidHttpContext_ShouldReturnCorrectUrl()
        {
            // Arrange
            var mockAccessor = new Mock<IHttpContextAccessor>();
            var mockContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();

            mockRequest.Setup(x => x.Scheme).Returns("https");
            mockRequest.Setup(x => x.Host).Returns(new HostString("api.example.com", 443));
            mockRequest.Setup(x => x.PathBase).Returns(new PathString("/api/v1"));

            mockContext.Setup(x => x.Request).Returns(mockRequest.Object);
            mockAccessor.Setup(x => x.HttpContext).Returns(mockContext.Object);

            // Act
            var result = mockAccessor.Object.GetApiBaseUrl();

            // Assert
            result.Should().Be("https://api.example.com:443/api/v1");
        }

        [Fact]
        public void GetApiBaseUrl_WithHttpScheme_ShouldReturnHttpUrl()
        {
            // Arrange
            var mockAccessor = new Mock<IHttpContextAccessor>();
            var mockContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();

            mockRequest.Setup(x => x.Scheme).Returns("http");
            mockRequest.Setup(x => x.Host).Returns(new HostString("localhost", 5000));
            mockRequest.Setup(x => x.PathBase).Returns(new PathString(""));

            mockContext.Setup(x => x.Request).Returns(mockRequest.Object);
            mockAccessor.Setup(x => x.HttpContext).Returns(mockContext.Object);

            // Act
            var result = mockAccessor.Object.GetApiBaseUrl();

            // Assert
            result.Should().Be("http://localhost:5000");
        }

        [Fact]
        public void GetApiBaseUrl_WithEmptyPathBase_ShouldNotIncludePathBase()
        {
            // Arrange
            var mockAccessor = new Mock<IHttpContextAccessor>();
            var mockContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();

            mockRequest.Setup(x => x.Scheme).Returns("https");
            mockRequest.Setup(x => x.Host).Returns(new HostString("api.example.com"));
            mockRequest.Setup(x => x.PathBase).Returns(PathString.Empty);

            mockContext.Setup(x => x.Request).Returns(mockRequest.Object);
            mockAccessor.Setup(x => x.HttpContext).Returns(mockContext.Object);

            // Act
            var result = mockAccessor.Object.GetApiBaseUrl();

            // Assert
            result.Should().Be("https://api.example.com");
        }

        [Fact]
        public void GetApiBaseUrl_WithDifferentPortNumbers_ShouldIncludePort()
        {
            // Arrange
            var mockAccessor = new Mock<IHttpContextAccessor>();
            var mockContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();

            mockRequest.Setup(x => x.Scheme).Returns("http");
            mockRequest.Setup(x => x.Host).Returns(new HostString("localhost", 8080));
            mockRequest.Setup(x => x.PathBase).Returns(new PathString("/app"));

            mockContext.Setup(x => x.Request).Returns(mockRequest.Object);
            mockAccessor.Setup(x => x.HttpContext).Returns(mockContext.Object);

            // Act
            var result = mockAccessor.Object.GetApiBaseUrl();

            // Assert
            result.Should().Be("http://localhost:8080/app");
        }
    }
}
