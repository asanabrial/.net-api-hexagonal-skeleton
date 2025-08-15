using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using HexagonalSkeleton.Test.TestInfrastructure.Shared;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Factories
{
    /// <summary>
    /// Test Web Application Factory que utiliza contenedores compartidos para mejor rendimiento
    /// Todos los tests en la suite compartirán la misma instancia de PostgreSQL, MongoDB, Kafka, etc.
    /// </summary>
    public class SharedTestWebApplicationFactory : AbstractTestWebApplicationFactory
    {
        /// <summary>
        /// Expone el orquestador de contenedores para verificaciones en tests
        /// </summary>
        public new ITestContainerOrchestrator ContainerOrchestrator => base.ContainerOrchestrator;

        protected override ITestContainerOrchestrator CreateContainerOrchestrator()
        {
            // Usar el gestor compartido para obtener contenedores reutilizables
            return SharedTestContainerManager.Instance.Orchestrator;
        }

        public override async Task InitializeAsync()
        {
            // Inicializar contenedores compartidos (solo se ejecuta una vez)
            await SharedTestContainerManager.Instance.InitializeAsync();
            
            // Ejecutar inicialización base
            await base.InitializeAsync();
            
            // Configurar Debezium una sola vez para todos los tests
            try
            {
                await SharedTestContainerManager.Instance.ConfigureDebeziumConnectorAsync("shared-test-connector");
            }
            catch
            {
                // Si falla Debezium Connect, continuamos con TestCdcEventPublisher como fallback
            }
        }

        public override async Task DisposeAsync()
        {
            // No disponer los contenedores compartidos aquí
            // Solo ejecutar la limpieza base sin los contenedores
            await base.DisposeAsync();
        }
    }
}
