using HexagonalSkeleton.Test.TestInfrastructure.Shared;
using Xunit;
using System;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Collections
{
    /// <summary>
    /// Fixture that manages the lifecycle of shared containers
    /// Runs once before the first test and cleans up after the last test
    /// </summary>
    public class SharedContainersFixture : IAsyncLifetime
    {
        /// <summary>
        /// Initialization that runs once before all tests
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                Console.WriteLine("Initializing shared containers fixture...");
                
                // Initialize shared containers
                await SharedTestContainerManager.Instance.InitializeAsync();
                
                // Configure Debezium Connect
                await SharedTestContainerManager.Instance.ConfigureDebeziumConnectorAsync("fixture-postgres-connector");
                
                Console.WriteLine("Shared containers fixture initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing fixture: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Cleanup that runs once after all tests
        /// </summary>
        public async Task DisposeAsync()
        {
            try
            {
                Console.WriteLine("Cleaning up shared containers fixture...");
                
                // Dispose shared containers
                await SharedTestContainerManager.Instance.DisposeAsync();
                
                Console.WriteLine("Shared containers fixture cleaned up");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning up fixture: {ex.Message}");
                // Don't rethrow exception in Dispose to avoid errors when finalizing tests
            }
        }
    }
}
