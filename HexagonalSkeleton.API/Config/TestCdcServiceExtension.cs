using Confluent.Kafka;
using HexagonalSkeleton.Infrastructure.CDC.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HexagonalSkeleton.API.Config
{
    /// <summary>
    /// Extension class for configuring CDC services specifically for integration tests.
    /// This overrides production CDC configuration with test container settings.
    /// </summary>
    public static class TestCdcServiceExtension
    {
        /// <summary>
        /// Reconfigures CDC services for integration tests using TestContainer dynamic URLs
        /// This method should be called after the main CDC configuration to override with test-specific settings
        /// </summary>
        public static IServiceCollection ReconfigureCdcForTests(this IServiceCollection services, IConfiguration configuration)
        {
            // Reconfigure Producer and Consumer configs to use dynamic test container settings
            services.Configure<ProducerConfig>(options =>
            {
                var cdcSection = configuration.GetSection($"{CdcOptions.SectionName}:Kafka");
                var bootstrapServers = cdcSection.GetValue<string>("BootstrapServers");
                
                if (!string.IsNullOrEmpty(bootstrapServers))
                {
                    options.BootstrapServers = bootstrapServers;
                    Console.WriteLine($"🔧 Test Producer configured with BootstrapServers: {bootstrapServers}");
                }
            });

            services.Configure<ConsumerConfig>(options =>
            {
                var cdcSection = configuration.GetSection($"{CdcOptions.SectionName}:Kafka");
                var bootstrapServers = cdcSection.GetValue<string>("BootstrapServers");
                var baseGroupId = cdcSection.GetValue<string>("ConsumerGroupId") ?? "hexagonal-test-consumer-group";
                var generateUniqueGroupId = cdcSection.GetValue<bool>("GenerateUniqueGroupId");
                
                if (!string.IsNullOrEmpty(bootstrapServers))
                {
                    options.BootstrapServers = bootstrapServers;
                    Console.WriteLine($"🔧 Test Consumer configured with BootstrapServers: {bootstrapServers}");
                }
                
                var groupId = generateUniqueGroupId 
                    ? $"{baseGroupId}-{Guid.NewGuid()}" 
                    : baseGroupId;
                
                options.GroupId = groupId;
                options.AutoOffsetReset = AutoOffsetReset.Latest;
                options.EnableAutoCommit = false;
                options.SecurityProtocol = SecurityProtocol.Plaintext;
                options.ClientId = cdcSection.GetValue<string>("ConsumerClientId") ?? "hexagonal-test-consumer";
                options.SessionTimeoutMs = 6000;
                options.HeartbeatIntervalMs = 3000;
                
                Console.WriteLine($"🔧 Test Consumer configured with GroupId: {groupId}");
            });

            return services;
        }
    }
}