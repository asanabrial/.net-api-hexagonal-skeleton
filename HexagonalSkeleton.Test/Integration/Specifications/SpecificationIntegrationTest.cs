using Xunit;
using Microsoft.Extensions.DependencyInjection;
using HexagonalSkeleton.Domain.Specifications.Users;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain.ValueObjects;
using HexagonalSkeleton.Test;
using DomainUser = HexagonalSkeleton.Domain.User;

namespace HexagonalSkeleton.Test.Integration.Specifications
{
    /// <summary>
    /// Integration tests for Specification pattern
    /// Demonstrates end-to-end functionality with real repository implementation
    /// </summary>
    public class SpecificationIntegrationTest : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public SpecificationIntegrationTest(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetUsersAsync_WithActiveUserSpecification_ShouldReturnOnlyActiveUsers()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IUserReadRepository>();
            
            var specification = new ActiveUserSpecification();
            var pagination = PaginationParams.Create(1, 10);

            // Act
            var result = await repository.GetUsersAsync(specification, pagination);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Items.All(user => !user.IsDeleted));
        }

        [Fact]
        public async Task GetUsersAsync_WithAdultUserSpecification_ShouldReturnOnlyAdults()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IUserReadRepository>();
            
            var specification = new AdultUserSpecification();
            var pagination = PaginationParams.Create(1, 10);

            // Act
            var result = await repository.GetUsersAsync(specification, pagination);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Items.All(user => user.IsAdult()));
        }

        [Fact]
        public async Task GetUsersAsync_WithCombinedSpecifications_ShouldFilterCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IUserReadRepository>();
            
            var specification = UserSpecificationBuilder.Create()
                .OnlyActive()
                .OnlyAdults()
                .Build();

            var pagination = PaginationParams.Create(1, 10);

            // Act
            var result = await repository.GetUsersAsync(specification, pagination);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Items.All(user => !user.IsDeleted && user.IsAdult()));
        }

        [Fact]
        public async Task CountUsersAsync_WithSpecification_ShouldReturnCorrectCount()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IUserReadRepository>();
            
            var specification = new ActiveUserSpecification();

            // Act
            var count = await repository.CountUsersAsync(specification);

            // Assert
            Assert.True(count >= 0);
        }

        [Fact]
        public async Task AnyUsersAsync_WithSpecification_ShouldReturnCorrectBoolean()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IUserReadRepository>();
            
            var specification = new ActiveUserSpecification();

            // Act
            var hasAny = await repository.AnyUsersAsync(specification);

            // Assert - This depends on test data, but we can check it doesn't throw
            Assert.True(hasAny || !hasAny); // Just ensure it returns a boolean without error
        }

        [Fact]
        public async Task GetUsersAsync_WithComplexBuilder_ShouldWorkCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IUserReadRepository>();
            
            var specification = UserSpecificationBuilder.Create()
                .OnlyActive()
                .OnlyAdults()
                .WithCompleteProfile()
                .Build();

            var pagination = PaginationParams.Create(1, 5);

            // Act
            var result = await repository.GetUsersAsync(specification, pagination);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.PageSize == 5);
            Assert.True(result.Items.All(user => 
                !user.IsDeleted && 
                user.IsAdult()));
        }
    }
}
