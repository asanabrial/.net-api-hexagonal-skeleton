using HexagonalSkeleton.Test.TestInfrastructure.Shared;
using Xunit;
using System;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Collections
{
    /// <summary>
    /// Fast fixture that manages only the 3 essential containers
    /// </summary>
    public class FastContainersFixture : IAsyncLifetime
    {
        public async Task InitializeAsync()
        {
            try
            {
                Console.WriteLine("Initializing FAST fixture...");
                
                // Only initialize essential containers
                await FastTestContainerManager.Instance.InitializeAsync();
                
                Console.WriteLine("Fast fixture initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing fast fixture: {ex.Message}");
                throw;
            }
        }

        public async Task DisposeAsync()
        {
            try
            {
                Console.WriteLine("Cleaning up fast fixture...");
                
                await FastTestContainerManager.Instance.DisposeAsync();
                
                Console.WriteLine("Fast fixture cleaned up");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning up fast fixture: {ex.Message}");
                // Don't rethrow exception in Dispose
            }
        }
    }
}
