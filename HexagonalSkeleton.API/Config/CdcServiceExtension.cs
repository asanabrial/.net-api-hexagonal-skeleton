using HexagonalSkeleton.Infrastructure.CDC;
using HexagonalSkeleton.Infrastructure.CDC.Configuration;
using HexagonalSkeleton.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HexagonalSkeleton.API.Config
{
    /// <summary>
    /// Extension to configure CDC services with Debezium + Kafka
    /// Replaces MassTransit with an enterprise-grade implementation based on WAL
    /// Implements CDC patterns used by Netflix, Uber, LinkedIn
    /// </summary>
    public static class CdcServiceExtension
    {
        /// <summary>
        /// Configures the complete CDC infrastructure with Debezium + Kafka
        /// CDC is mandatory in this CQRS architecture
        /// </summary>
        public static IServiceCollection AddDebeziumCdc(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure CDC options
            services.Configure<CdcOptions>(configuration.GetSection(CdcOptions.SectionName));

            // Register CDC services
            services.AddScoped<DebeziumEventProcessor>(); // Changed to Scoped to match QueryDbContext lifetime
            services.AddHostedService<DebeziumConsumerService>();

            return services;
        }

        /// <summary>
        /// Configures topics and Debezium connectors via API calls
        /// Runs during application startup
        /// </summary>
        public static async Task<IServiceCollection> ConfigureDebeziumConnectors(
            this IServiceCollection services, 
            IConfiguration configuration,
            ILogger logger)
        {
            var debeziumApiUrl = configuration["CDC:DebeziumConnectUrl"]
                ?? throw new InvalidOperationException("CDC:DebeziumConnectUrl configuration is required");
            
            try
            {
                using var httpClient = new HttpClient();
                
                // PostgreSQL connector configuration
                var postgresConnectorConfig = new
                {
                    name = "hexagonal-postgres-connector",
                    config = new
                    {
                        // Connector configuration
                        connector_class = "io.debezium.connector.postgresql.PostgresConnector",
                        plugin_name = "pgoutput", // Native PostgreSQL 10+ plugin
                        
                        // Connection to database (using configuration)
                        database_hostname = configuration["CDC:PostgreSQL:Host"] 
                            ?? throw new InvalidOperationException("CDC:PostgreSQL:Host configuration is required"),
                        database_port = configuration["CDC:PostgreSQL:Port"] 
                            ?? throw new InvalidOperationException("CDC:PostgreSQL:Port configuration is required"),
                        database_user = configuration["CDC:PostgreSQL:User"] 
                            ?? throw new InvalidOperationException("CDC:PostgreSQL:User configuration is required"),
                        database_password = configuration["CDC:PostgreSQL:Password"] 
                            ?? throw new InvalidOperationException("CDC:PostgreSQL:Password configuration is required"),
                        database_dbname = configuration["CDC:PostgreSQL:Database"] 
                            ?? throw new InvalidOperationException("CDC:PostgreSQL:Database configuration is required"),
                        database_server_name = configuration["CDC:PostgreSQL:ServerName"] 
                            ?? throw new InvalidOperationException("CDC:PostgreSQL:ServerName configuration is required"),
                        
                        // Table and topic configuration
                        table_include_list = configuration["CDC:TableIncludeList"] 
                            ?? throw new InvalidOperationException("CDC:TableIncludeList configuration is required"),
                        
                        // Topic configuration
                        topic_prefix = configuration["CDC:TopicPrefix"] 
                            ?? throw new InvalidOperationException("CDC:TopicPrefix configuration is required"),
                        
                        // Transformations configuration
                        transforms = "route",
                        transforms_route_type = "org.apache.kafka.connect.transforms.RegexRouter",
                        transforms_route_regex = "([^.]+)\\.([^.]+)\\.([^.]+)",
                        transforms_route_replacement = "hexagonal.cdc.$3",
                        
                        // Snapshots configuration
                        snapshot_mode = "initial",
                        
                        // Format configuration
                        key_converter = "org.apache.kafka.connect.json.JsonConverter",
                        value_converter = "org.apache.kafka.connect.json.JsonConverter",
                        key_converter_schemas_enable = false,
                        value_converter_schemas_enable = false,
                        
                        // Replication slots configuration
                        slot_name = configuration["CDC:PostgreSQL:SlotName"] ?? "hexagonal_slot",
                        publication_name = configuration["CDC:PostgreSQL:PublicationName"] ?? "hexagonal_publication"
                    }
                };

                // Send connector configuration
                var connectorJson = System.Text.Json.JsonSerializer.Serialize(postgresConnectorConfig);
                var content = new StringContent(connectorJson, System.Text.Encoding.UTF8, "application/json");
                
                var response = await httpClient.PostAsync($"{debeziumApiUrl}/connectors", content);
                
                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation("Debezium PostgreSQL connector configured successfully");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    logger.LogWarning("Error configuring Debezium connector: {Error}", error);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning("Could not connect to Debezium Connect: {Message}", ex.Message);
                logger.LogInformation("Make sure CDC services are running: docker-compose -f docker-compose.debezium.yml up -d");
            }

            return services;
        }
    }
}
