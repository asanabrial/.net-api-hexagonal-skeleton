using HexagonalSkeleton.Test.TestInfrastructure.Shared;
using Xunit;
using System;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Collections
{
    /// <summary>
    /// Colección de tests que comparten contenedores para mejor rendimiento
    /// Inicializa una vez los contenedores para toda la suite y los limpia al final
    /// </summary>
    [CollectionDefinition("SharedContainers")]
    public class SharedContainersCollection : ICollectionFixture<SharedContainersFixture>
    {
        // Esta clase está vacía y solo sirve como marcador para xUnit
        // La lógica está en SharedContainersFixture
    }

    /// <summary>
    /// Fixture que gestiona el ciclo de vida de los contenedores compartidos
    /// Se ejecuta una vez antes del primer test y se limpia después del último test
    /// </summary>
    public class SharedContainersFixture : IAsyncLifetime
    {
        /// <summary>
        /// Inicialización que se ejecuta una vez antes de todos los tests
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                Console.WriteLine("🚀 Inicializando fixture de contenedores compartidos...");
                
                // Inicializar contenedores compartidos
                await SharedTestContainerManager.Instance.InitializeAsync();
                
                // Configurar Debezium Connect
                await SharedTestContainerManager.Instance.ConfigureDebeziumConnectorAsync("fixture-postgres-connector");
                
                Console.WriteLine("✅ Fixture de contenedores compartidos inicializado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error inicializando fixture: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Limpieza que se ejecuta una vez después de todos los tests
        /// </summary>
        public async Task DisposeAsync()
        {
            try
            {
                Console.WriteLine("🛑 Limpiando fixture de contenedores compartidos...");
                
                // Disponer contenedores compartidos
                await SharedTestContainerManager.Instance.DisposeAsync();
                
                Console.WriteLine("✅ Fixture de contenedores compartidos limpiado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error limpiando fixture: {ex.Message}");
                // No relanzar la excepción en Dispose para evitar errores al finalizar tests
            }
        }
    }
}
