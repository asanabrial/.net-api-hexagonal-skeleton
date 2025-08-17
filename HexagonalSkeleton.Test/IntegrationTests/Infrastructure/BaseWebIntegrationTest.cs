using System.Net.Http;
using System.Threading.Tasks;
using HexagonalSkeleton.Test.TestInfrastructure.Factories;
using HexagonalSkeleton.Test.TestInfrastructure.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HexagonalSkeleton.Test.Integration.Infrastructure
{
    /// <summary>
    /// Base class for web integration tests that need HttpClient
    /// Uses WebApplicationFactory pattern for web testing
    /// </summary>
    public abstract class BaseWebIntegrationTest
    {
        protected readonly HttpClient _client;
        protected readonly ConfiguredTestWebApplicationFactory _factory;
        private MongoDbSyncHelper? _mongoHelperInstance;

        protected MongoDbSyncHelper _mongoHelper => _mongoHelperInstance ??= CreateMongoHelper();

        protected BaseWebIntegrationTest(ConfiguredTestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private MongoDbSyncHelper CreateMongoHelper()
        {
            using var scope = _factory.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<MongoDbSyncHelper>>();
            return new MongoDbSyncHelper(_factory.Services, logger);
        }
    }
}
