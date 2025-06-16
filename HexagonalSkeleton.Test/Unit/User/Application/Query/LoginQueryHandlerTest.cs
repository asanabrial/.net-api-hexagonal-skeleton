using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using HexagonalSkeleton.Application.Query;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Test.Unit.User.Domain;
using Moq;
using Xunit;
using DomainUser = HexagonalSkeleton.Domain.User;

namespace HexagonalSkeleton.Test.Unit.User.Application.Query
{    public class LoginQueryHandlerTest
    {
        private readonly Mock<IValidator<LoginQuery>> _mockValidator;
        private readonly Mock<IUserReadRepository> _mockUserReadRepository;
        private readonly Mock<IAuthenticationService> _mockAuthenticationService;
        private readonly LoginQueryHandler _handler;

        public LoginQueryHandlerTest()
        {
            _mockValidator = new Mock<IValidator<LoginQuery>>();
            _mockUserReadRepository = new Mock<IUserReadRepository>();
            _mockAuthenticationService = new Mock<IAuthenticationService>();
            _handler = new LoginQueryHandler(
                _mockValidator.Object,
                _mockUserReadRepository.Object,
                _mockAuthenticationService.Object);
        }        [Fact]
        public async Task Handle_WithValidCredentials_ShouldReturnSuccessResultWithToken()
        {
            // Arrange
            var query = new LoginQuery("test@example.com", "password123");
            var user = TestHelper.CreateTestUser(1);
            var expectedToken = "jwt.token.here";

            _mockValidator.Setup(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockAuthenticationService.Setup(x => x.ValidateCredentialsAsync(query.Email, query.Password, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockUserReadRepository.Setup(x => x.GetByEmailAsync(query.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockAuthenticationService.Setup(x => x.GenerateJwtTokenAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedToken);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
            
            var loginResult = result.Data as LoginQueryResult;
            loginResult.Should().NotBeNull();
            loginResult!.AccessToken.Should().Be(expectedToken);

            _mockAuthenticationService.Verify(x => x.ValidateCredentialsAsync(query.Email, query.Password, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithInvalidCredentials_ShouldReturnUnauthorizedError()
        {
            // Arrange
            var query = new LoginQuery("test@example.com", "wrongpassword");

            _mockValidator.Setup(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockAuthenticationService.Setup(x => x.ValidateCredentialsAsync(query.Email, query.Password, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();            result.Errors.Should().ContainKey("Error");
            result.Errors["Error"].Should().Contain("Invalid email or password");

            _mockAuthenticationService.Verify(x => x.GenerateJwtTokenAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithInvalidQuery_ShouldReturnValidationErrors()
        {
            // Arrange
            var query = new LoginQuery("", "");
            var validationErrors = new List<ValidationFailure>
            {
                new ValidationFailure("Email", "Email is required"),
                new ValidationFailure("Password", "Password is required")
            };
            var validationResult = new ValidationResult(validationErrors);

            _mockValidator.Setup(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainKey("Email");
            result.Errors.Should().ContainKey("Password");

            _mockAuthenticationService.Verify(x => x.ValidateCredentialsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }        [Theory]
        [InlineData("test@example.com", "password123")]
        [InlineData("user@domain.org", "mySecretPass")]
        [InlineData("admin@company.co.uk", "AdminPass2023")]
        public async Task Handle_WithDifferentValidCredentials_ShouldSucceed(string email, string password)
        {
            // Arrange
            var query = new LoginQuery(email, password);
            var user = TestHelper.CreateTestUser(1);
            var expectedToken = $"jwt.token.for.{email}";

            _mockValidator.Setup(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockAuthenticationService.Setup(x => x.ValidateCredentialsAsync(email, password, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockUserReadRepository.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockAuthenticationService.Setup(x => x.GenerateJwtTokenAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedToken);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            
            var loginResult = result.Data as LoginQueryResult;
            loginResult.Should().NotBeNull();
            loginResult!.AccessToken.Should().Be(expectedToken);
        }

        [Fact]
        public async Task Handle_WhenAuthServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var query = new LoginQuery("test@example.com", "password123");

            _mockValidator.Setup(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockAuthenticationService.Setup(x => x.ValidateCredentialsAsync(query.Email, query.Password, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Authentication service error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.Handle(query, CancellationToken.None));

            exception.Message.Should().Be("Authentication service error");
        }        [Fact]
        public async Task Handle_ShouldCallServicesInCorrectOrder()
        {
            // Arrange
            var query = new LoginQuery("test@example.com", "password123");
            var user = TestHelper.CreateTestUser(1);
            var expectedToken = "jwt.token.here";

            _mockValidator.Setup(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockAuthenticationService.Setup(x => x.ValidateCredentialsAsync(query.Email, query.Password, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockUserReadRepository.Setup(x => x.GetByEmailAsync(query.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockAuthenticationService.Setup(x => x.GenerateJwtTokenAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedToken);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            var sequence = new MockSequence();
            _mockValidator.InSequence(sequence)
                .Setup(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            
            _mockAuthenticationService.InSequence(sequence)
                .Setup(x => x.ValidateCredentialsAsync(query.Email, query.Password, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            
            _mockUserReadRepository.InSequence(sequence)
                .Setup(x => x.GetByEmailAsync(query.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            
            _mockAuthenticationService.InSequence(sequence)
                .Setup(x => x.GenerateJwtTokenAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedToken);
        }
    }
}
