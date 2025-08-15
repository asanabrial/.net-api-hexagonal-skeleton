using HexagonalSkeleton.Test.TestInfrastructure.Shared;
using Xunit;
using System;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Collections
{
    /// <summary>
    /// Colección para tests RÁPIDOS que solo usan PostgreSQL + MongoDB + Kafka
    /// Ideal para desarrollo diario (60-90 segundos vs 3-4 minutos)
    /// </summary>
    [CollectionDefinition("FastContainers")]
    public class FastContainersCollection : ICollectionFixture<FastContainersFixture>
    {
        // Esta clase está vacía y solo sirve como marcador para xUnit
    }

    /// <summary>
    /// Fixture rápido que gestiona solo los 3 contenedores esenciales
    /// </summary>
    public class FastContainersFixture : IAsyncLifetime
    {
        public async Task InitializeAsync()
        {
            try
            {
                Console.WriteLine("⚡ Inicializando fixture RÁPIDO...");
                
                // Solo inicializar los contenedores esenciales
                await FastTestContainerManager.Instance.InitializeAsync();
                
                Console.WriteLine("✅ Fixture rápido inicializado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error inicializando fixture rápido: {ex.Message}");
                throw;
            }
        }

        public async Task DisposeAsync()
        {
            try
            {
                Console.WriteLine("🛑 Limpiando fixture rápido...");
                
                await FastTestContainerManager.Instance.DisposeAsync();
                
                Console.WriteLine("✅ Fixture rápido limpiado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error limpiando fixture rápido: {ex.Message}");
            }
        }
    }
}
