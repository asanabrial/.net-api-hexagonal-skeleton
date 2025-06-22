using FluentValidation;
using FluentValidation.Results;
using HexagonalSkeleton.Application.Query;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain.ValueObjects;
using HexagonalSkeleton.Test.Unit.User.Domain;
using Moq;
using Xunit;
using DomainUser = HexagonalSkeleton.Domain.User;

namespace HexagonalSkeleton.Test.Unit.User.Application.Query
{
    public class LoginQueryHandlerTest
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
        }

        [Fact]
        public async Task Handle_WithValidCredentials_ShouldReturnSuccessResultWithToken()
        {
            // Arrange
            var query = new LoginQuery("test@example.com", "password123");
            var user = TestHelper.CreateTestUser(1);
            var expectedToken = "jwt.token.here";

            _mockValidator.Setup(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockAuthenticationService.Setup(x => x.ValidateCredentialsAsync(query.Email, query.Password, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);            _mockUserReadRepository.Setup(x => x.GetByEmailAsync(query.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockAuthenticationService.Setup(x => x.GenerateJwtTokenAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenInfo(expectedToken, DateTime.UtcNow.AddDays(7)));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedToken, result.AccessToken);

            _mockAuthenticationService.Verify(x => x.ValidateCredentialsAsync(query.Email, query.Password, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithInvalidCredentials_ShouldThrowAuthenticationException()
        {
            // Arrange
            var query = new LoginQuery("test@example.com", "wrongpassword");

            _mockValidator.Setup(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockAuthenticationService.Setup(x => x.ValidateCredentialsAsync(query.Email, query.Password, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AuthenticationException>(() =>
                _handler.Handle(query, CancellationToken.None));

            Assert.Contains("Invalid email or password", exception.Message);

            _mockAuthenticationService.Verify(x => x.GenerateJwtTokenAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithInvalidQuery_ShouldThrowValidationException()
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

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HexagonalSkeleton.Application.Exceptions.ValidationException>(() =>
                _handler.Handle(query, CancellationToken.None));

            Assert.True(exception.Errors.ContainsKey("Email"));
            Assert.True(exception.Errors.ContainsKey("Password"));
            Assert.Contains("Email is required", exception.Errors["Email"]);
            Assert.Contains("Password is required", exception.Errors["Password"]);

            _mockAuthenticationService.Verify(x => x.ValidateCredentialsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
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
                .ReturnsAsync(true);            _mockUserReadRepository.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockAuthenticationService.Setup(x => x.GenerateJwtTokenAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenInfo(expectedToken, DateTime.UtcNow.AddDays(7)));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedToken, result.AccessToken);
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

            Assert.Equal("Authentication service error", exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldCallServicesInCorrectOrder()
        {
            // Arrange
            var query = new LoginQuery("test@example.com", "password123");
            var user = TestHelper.CreateTestUser(1);
            var expectedToken = "jwt.token.here";

            _mockValidator.Setup(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockAuthenticationService.Setup(x => x.ValidateCredentialsAsync(query.Email, query.Password, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);            _mockUserReadRepository.Setup(x => x.GetByEmailAsync(query.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockAuthenticationService.Setup(x => x.GenerateJwtTokenAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenInfo(expectedToken, DateTime.UtcNow.AddDays(7)));

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
                .ReturnsAsync(new TokenInfo(expectedToken, DateTime.UtcNow.AddDays(7)));
        }
    }
}