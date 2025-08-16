using Confluent.Kafka;
using HexagonalSkeleton.Infrastructure.CDC;
using HexagonalSkeleton.Infrastructure.CDC.Configuration;
using HexagonalSkeleton.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HexagonalSkeleton.API.Config
{
    /// <summary>
    /// Extension para configurar servicios CDC con Debezium + Kafka
    /// Reemplaza MassTransit con una implementación enterprise-grade basada en WAL
    /// Implementa patrones CDC usados por Netflix, Uber, LinkedIn
    /// </summary>
    public static class CdcServiceExtension
    {
        /// <summary>
        /// Configures the complete CDC infrastructure with Debezium + Kafka
        /// CDC is mandatory in this CQRS architecture
        /// </summary>
        public static IServiceCollection AddDebeziumCdc(this IServiceCollection services, IConfiguration configuration)
        {
            // Configurar opciones CDC
            services.Configure<CdcOptions>(configuration.GetSection(CdcOptions.SectionName));
            
        // Configurar Kafka Producer desde configuración CDC
        services.Configure<ProducerConfig>(options =>
        {
            // This configuration will be evaluated at DI resolution time, not registration time
            // However, for test scenarios, we need to ensure dynamic config is available
            // We'll implement delayed configuration in the service constructor instead
        });        // Configurar Kafka Consumer desde configuración CDC
        services.Configure<ConsumerConfig>(options =>
        {
            // This configuration will be evaluated at DI resolution time, not registration time
            // However, for test scenarios, we need to ensure dynamic config is available
            // We'll implement delayed configuration in the service constructor instead
        });            // Registrar servicios CDC
            services.AddScoped<DebeziumEventProcessor>(); // Changed to Scoped to match QueryDbContext lifetime
            services.AddHostedService<DebeziumConsumerService>();

            return services;
        }

        /// <summary>
        /// Configura tópicos y conectores Debezium mediante API calls
        /// Se ejecuta durante el startup de la aplicación
        /// </summary>
        public static async Task<IServiceCollection> ConfigureDebeziumConnectors(
            this IServiceCollection services, 
            IConfiguration configuration,
            ILogger logger)
        {
            var debeziumApiUrl = configuration["CDC:DebeziumConnectUrl"] ?? "http://localhost:8083";
            
            try
            {
                using var httpClient = new HttpClient();
                
                // Configuración del conector PostgreSQL
                var postgresConnectorConfig = new
                {
                    name = "hexagonal-postgres-connector",
                    config = new
                    {
                        // Configuración del conector
                        connector_class = "io.debezium.connector.postgresql.PostgresConnector",
                        plugin_name = "pgoutput", // Plugin nativo de PostgreSQL 10+
                        
                        // Conexión a base de datos (desde docker-compose)
                        database_hostname = configuration["CDC:PostgreSQL:Host"] ?? "postgresql",
                        database_port = configuration["CDC:PostgreSQL:Port"] ?? "5432",
                        database_user = configuration["CDC:PostgreSQL:User"] ?? "hexagonal_user",
                        database_password = configuration["CDC:PostgreSQL:Password"] ?? "hexagonal_password",
                        database_dbname = configuration["CDC:PostgreSQL:Database"] ?? "HexagonalSkeleton",
                        database_server_name = configuration["CDC:PostgreSQL:ServerName"] ?? "hexagonal-postgres",
                        
                        // Configuración de tablas
                        table_include_list = configuration["CDC:TableIncludeList"] ?? "public.users",
                        
                        // Configuración de tópicos
                        topic_prefix = configuration["CDC:TopicPrefix"] ?? "hexagonal",
                        
                        // Configuración de transformaciones
                        transforms = "route",
                        transforms_route_type = "org.apache.kafka.connect.transforms.RegexRouter",
                        transforms_route_regex = "([^.]+)\\.([^.]+)\\.([^.]+)",
                        transforms_route_replacement = "hexagonal.cdc.$3",
                        
                        // Configuración de snapshots
                        snapshot_mode = "initial",
                        
                        // Configuración de formato
                        key_converter = "org.apache.kafka.connect.json.JsonConverter",
                        value_converter = "org.apache.kafka.connect.json.JsonConverter",
                        key_converter_schemas_enable = false,
                        value_converter_schemas_enable = false,
                        
                        // Configuración de slots de replicación
                        slot_name = configuration["CDC:PostgreSQL:SlotName"] ?? "hexagonal_slot",
                        publication_name = configuration["CDC:PostgreSQL:PublicationName"] ?? "hexagonal_publication"
                    }
                };

                // Enviar configuración del conector
                var connectorJson = System.Text.Json.JsonSerializer.Serialize(postgresConnectorConfig);
                var content = new StringContent(connectorJson, System.Text.Encoding.UTF8, "application/json");
                
                var response = await httpClient.PostAsync($"{debeziumApiUrl}/connectors", content);
                
                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation("✅ Debezium PostgreSQL connector configurado exitosamente");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    logger.LogWarning("⚠️ Error configurando Debezium connector: {Error}", error);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning("⚠️ No se pudo conectar a Debezium Connect: {Message}", ex.Message);
                logger.LogInformation("💡 Asegúrate de que los servicios CDC estén ejecutándose: docker-compose -f docker-compose.debezium.yml up -d");
            }

            return services;
        }
    }
}
