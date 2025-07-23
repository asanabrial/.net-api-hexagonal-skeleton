using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using System;

namespace HexagonalSkeleton.Test.TestInfrastructure.Implementations
{
    /// <summary>
    /// Testcontainers implementation of the container factory
    /// </summary>
    public class TestcontainersFactory : ITestContainerFactory
    {
        public IPostgreSqlTestContainer CreatePostgreSqlContainer(TestContainerConfiguration? config = null)
        {
            var settings = config ?? new TestContainerConfiguration();
            
            return new TestcontainersPostgreSqlContainer(
                image: settings.Image ?? "postgres:15-alpine",
                database: settings.Database ?? "hexagonal_test",
                username: settings.Username ?? "test_user",
                password: settings.Password ?? "test_password"
            );
        }

        public IMongoDbTestContainer CreateMongoDbContainer(TestContainerConfiguration? config = null)
        {
            var settings = config ?? new TestContainerConfiguration();
            
            return new TestcontainersMongoDbContainer(
                image: settings.Image ?? "mongo:7",
                database: settings.Database ?? "hexagonal_test",
                username: settings.Username ?? "test_user",
                password: settings.Password ?? "test_password"
            );
        }

        public IRabbitMqTestContainer CreateRabbitMqContainer(TestContainerConfiguration? config = null)
        {
            var settings = config ?? new TestContainerConfiguration();
            
            return new TestcontainersRabbitMqContainer(
                image: settings.Image ?? "rabbitmq:3-management-alpine",
                username: settings.Username ?? "test_user",
                password: settings.Password ?? "test_password"
            );
        }
    }
}
