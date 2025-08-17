using HexagonalSkeleton.Application.Features.UserProfile.Queries;
using HexagonalSkeleton.Application.Features.UserProfile.Dto;
using HexagonalSkeleton.Application.Exceptions;
using HexagonalSkeleton.Domain.Ports;
using FluentValidation;
using AutoMapper;
using Moq;
using Xunit;
using DomainUser = HexagonalSkeleton.Domain.User;

namespace HexagonalSkeleton.Test.Unit.Application.Features.UserProfile.Queries
{
    public class GetMyProfileQueryHandlerTest
    {
        private readonly Mock<IValidator<GetMyProfileQuery>> _mockValidator;
        private readonly Mock<IUserReadRepository> _mockUserReadRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GetMyProfileQueryHandler _handler;

        public GetMyProfileQueryHandlerTest()
        {
            _mockValidator = new Mock<IValidator<GetMyProfileQuery>>();
            _mockUserReadRepository = new Mock<IUserReadRepository>();
            _mockMapper = new Mock<IMapper>();
            _handler = new GetMyProfileQueryHandler(
                _mockValidator.Object,
                _mockUserReadRepository.Object,
                _mockMapper.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldReturnUserProfile()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetMyProfileQuery(userId);

            var user = DomainUser.Create(
                "john.doe@example.com",
                "salt",
                "hashedPassword",
                "John",
                "Doe",
                DateTime.UtcNow.AddYears(-25),
                "+1234567890",
                12.34,
                56.78,
                "About me");

            var expectedResult = new UserProfileDto
            {
                Id = userId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "+1234567890"
            };

            _mockValidator.Setup(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _mockUserReadRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockMapper.Setup(x => x.Map<UserProfileDto>(user))
                .Returns(expectedResult);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.Id, result.Id);
            Assert.Equal(expectedResult.FirstName, result.FirstName);
            Assert.Equal(expectedResult.LastName, result.LastName);
            Assert.Equal(expectedResult.Email, result.Email);
        }

        [Fact]
        public async Task Handle_DeletedUser_ShouldThrowNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetMyProfileQuery(userId);

            _mockValidator.Setup(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            // Mock read repository returns null for deleted users (filtered out)
            _mockUserReadRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DomainUser?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(query, CancellationToken.None));

            Assert.Contains("User profile not found or user has been deleted", exception.Message);
        }

        [Fact]
        public async Task Handle_InvalidRequest_ShouldThrowValidationException()
        {
            // Arrange
            var query = new GetMyProfileQuery(Guid.Empty);

            var validationResult = new FluentValidation.Results.ValidationResult();
            validationResult.Errors.Add(new FluentValidation.Results.ValidationFailure("UserId", "User ID is required"));

            _mockValidator.Setup(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HexagonalSkeleton.Application.Exceptions.ValidationException>(
                () => _handler.Handle(query, CancellationToken.None));

            Assert.False(validationResult.IsValid);
            _mockUserReadRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldUseReadRepositoryWithFiltering()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetMyProfileQuery(userId);

            _mockValidator.Setup(x => x.ValidateAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _mockUserReadRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DomainUser?)null);

            // Act
            await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(query, CancellationToken.None));

            // Assert
            // Verify that we use the READ repository (which filters deleted users)
            // NOT the write repository which would include deleted users
            _mockUserReadRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
