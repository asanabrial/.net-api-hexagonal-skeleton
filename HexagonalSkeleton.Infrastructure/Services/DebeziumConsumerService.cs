using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using HexagonalSkeleton.Infrastructure.CDC;
using HexagonalSkeleton.Infrastructure.CDC.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HexagonalSkeleton.Infrastructure.Services
{
    /// <summary>
    /// Background service for consuming Debezium CDC events from Kafka.
    /// Implements enterprise patterns for reliable message processing with error handling and retry policies.
    /// </summary>
    public class DebeziumConsumerService : BackgroundService
    {
        private IConsumer<string, string>? _consumer; // Remove readonly for runtime initialization
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DebeziumConsumerService> _logger;
        private readonly IConfiguration _configuration; // Add configuration injection
        private ConsumerConfig? _config; // Remove readonly, will be set at runtime
        private readonly CdcOptions _cdcOptions;
        private readonly string[] _topics;

        public DebeziumConsumerService(
            IOptions<CdcOptions> cdcOptions,
            IServiceProvider serviceProvider,
            ILogger<DebeziumConsumerService> logger,
            IConfiguration configuration) // Inject IConfiguration directly
        {
            _cdcOptions = cdcOptions?.Value ?? throw new ArgumentNullException(nameof(cdcOptions));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // Configure topics to consume from CDC configuration
            _topics = _cdcOptions.Kafka.Topics ?? new[] { "hexagonal-postgres.public.users" };

            // Delay consumer creation until StartAsync is called
        }

        /// <summary>
        /// Creates the Kafka consumer with runtime configuration resolution
        /// </summary>
        private void InitializeConsumer()
        {
            // Resolve configuration at runtime to get dynamic values from TestContainers
            var cdcSection = _configuration.GetSection($"{CdcOptions.SectionName}:Kafka");
            
            Console.WriteLine($"🔧 [DEBUG] Initializing Consumer at runtime");
            Console.WriteLine($"🔧 [DEBUG] Configuration Section: {CdcOptions.SectionName}:Kafka");
            Console.WriteLine($"🔧 [DEBUG] All CDC:Kafka configuration keys:");
            foreach (var kvp in cdcSection.AsEnumerable())
            {
                Console.WriteLine($"🔧 [DEBUG]   {kvp.Key} = {kvp.Value}");
            }
            
            var baseGroupId = _configuration[$"{CdcOptions.SectionName}:Kafka:ConsumerGroupId"] ?? "hexagonal-cdc-consumer-group";
            var generateUniqueGroupId = bool.Parse(_configuration[$"{CdcOptions.SectionName}:Kafka:GenerateUniqueGroupId"] ?? "false");
            
            var groupId = generateUniqueGroupId 
                ? $"{baseGroupId}-{Guid.NewGuid()}" 
                : baseGroupId;

            var bootstrapServers = _configuration[$"{CdcOptions.SectionName}:Kafka:BootstrapServers"];
            if (string.IsNullOrEmpty(bootstrapServers))
            {
                // Check if we're in a test environment
                var environment = _configuration["ASPNETCORE_ENVIRONMENT"];
                if (environment == "Test")
                {
                    _logger.LogWarning("CDC:Kafka:BootstrapServers not configured for Test environment. Debezium Consumer Service will be disabled.");
                    return;
                }
                
                throw new InvalidOperationException("CDC:Kafka:BootstrapServers configuration is required");
            }
            
            Console.WriteLine($"🔧 [DEBUG] Consumer BootstrapServers from runtime config: {bootstrapServers}");
            Console.WriteLine($"🔧 [DEBUG] Consumer GroupId: {groupId}");
            
            _config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                SecurityProtocol = SecurityProtocol.Plaintext,
                ClientId = _configuration[$"{CdcOptions.SectionName}:Kafka:ConsumerClientId"] ?? "hexagonal-consumer"
            };
            
            _consumer = new ConsumerBuilder<string, string>(_config)
                .SetErrorHandler((_, error) =>
                {
                    _logger.LogError("Kafka Consumer Error: {Reason} | Code: {Code}", error.Reason, error.Code);
                })
                .SetStatisticsHandler((_, stats) =>
                {
                    _logger.LogDebug("Kafka Consumer Stats: {Stats}", stats);
                })
                .SetPartitionsAssignedHandler((c, partitions) =>
                {
                    _logger.LogInformation("Assigned partitions: {Partitions}", 
                        string.Join(", ", partitions.Select(p => $"{p.Topic}[{p.Partition}]")));
                    // Log current offset position for debugging
                    foreach (var partition in partitions)
                    {
                        try
                        {
                            var watermarkOffsets = c.QueryWatermarkOffsets(partition, TimeSpan.FromSeconds(5));
                            _logger.LogInformation("Topic {Topic}[{Partition}] watermark: Low={Low}, High={High}", 
                                partition.Topic, partition.Partition, watermarkOffsets.Low, watermarkOffsets.High);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("Could not query watermark offsets for {Topic}[{Partition}]: {Error}", 
                                partition.Topic, partition.Partition, ex.Message);
                        }
                    }
                })
                .SetPartitionsRevokedHandler((c, partitions) =>
                {
                    _logger.LogInformation("Revoked partitions: {Partitions}", 
                        string.Join(", ", partitions.Select(p => $"{p.Topic}[{p.Partition}]")));
                })
                .Build();

            _logger.LogInformation("Debezium Consumer Service initialized for topics: {Topics}", 
                string.Join(", ", _topics));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Debezium Consumer Service...");

            // Initialize consumer with runtime configuration
            InitializeConsumer();
            
            // Check if consumer was initialized (it won't be in test environment if config is missing)
            if (_consumer == null)
            {
                _logger.LogWarning("Debezium Consumer Service not initialized. Service will exit.");
                return;
            }

            // Don't block startup - run consumer in background
            await Task.Run(async () =>
            {
                try
                {
                    _consumer!.Subscribe(_topics);
                    _logger.LogInformation("Subscribed to Debezium CDC topics: {Topics}", string.Join(", ", _topics));

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(5));
                            if (consumeResult?.Message != null)
                            {
                                _logger.LogInformation("📨 Received message from {Topic} with key {Key}", 
                                    consumeResult.Topic, consumeResult.Message.Key);
                                await ProcessMessageAsync(consumeResult, stoppingToken);
                            }
                            else
                            {
                                _logger.LogDebug("🔍 No messages available, continuing to poll...");
                            }
                        }
                        catch (ConsumeException ex)
                        {
                            if (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                            {
                                _logger.LogWarning("Topics not available yet: {Error}. Waiting...", ex.Error.Reason);
                                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                                continue;
                            }
                            _logger.LogError(ex, "Error consuming message from Kafka: {Error}", ex.Error.Reason);
                        
                            // For critical errors, wait before retrying
                            if (ex.Error.IsFatal)
                            {
                                _logger.LogError("Fatal Kafka error, waiting 15s before retry...");
                                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.LogInformation("Debezium Consumer Service stopping...");
                            break;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Unexpected error in Debezium Consumer Service");
                            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fatal error in Debezium Consumer Service");
                }
                finally
                {
                    try
                    {
                        _consumer?.Close();
                        _logger.LogInformation("Debezium Consumer Service stopped gracefully");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error closing Kafka consumer");
                    }
                }
            }, stoppingToken);
        }

        /// <summary>
        /// Procesa un mensaje individual de Kafka
        /// </summary>
        private async Task ProcessMessageAsync(ConsumeResult<string, string> consumeResult, CancellationToken cancellationToken)
        {
            var message = consumeResult.Message;
            var topic = consumeResult.Topic;
            var partition = consumeResult.Partition.Value;
            var offset = consumeResult.Offset.Value;

            _logger.LogDebug("📨 Processing message from {Topic}[{Partition}] @ {Offset} | Key: {Key}", 
                topic, partition, offset, message.Key);

            try
            {
                // Check if this is a user change event
                if (IsUserChangeEvent(topic))
                {
                    await ProcessUserChangeEventAsync(message.Value, cancellationToken);
                }
                else
                {
                    _logger.LogDebug("Skipping non-user event from topic: {Topic}", topic);
                }

                // Commit offset after successful processing
                try
                {
                    _consumer?.Commit(consumeResult);
                    _logger.LogDebug("Committed offset {Offset} for {Topic}[{Partition}]", 
                        offset, topic, partition);
                }
                catch (KafkaException ex)
                {
                    _logger.LogError(ex, "Failed to commit offset {Offset} for {Topic}[{Partition}]", 
                        offset, topic, partition);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from {Topic}[{Partition}] @ {Offset} | Key: {Key} | Value: {Value}", 
                    topic, partition, offset, message.Key, message.Value);

                
                await HandleProcessingErrorAsync(consumeResult, ex, cancellationToken);
            }
        }

        /// <summary>
        /// Processes a user change event
        /// </summary>
        private async Task ProcessUserChangeEventAsync(string eventPayload, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var eventProcessor = scope.ServiceProvider.GetRequiredService<DebeziumEventProcessor>();

            var success = await eventProcessor.ProcessChangeEventAsync(eventPayload, cancellationToken);

            if (success)
            {
                _logger.LogDebug("Successfully processed user change event");
            }
            else
            {
                _logger.LogWarning("Failed to process user change event");
                // Could implement retry logic here
            }
        }

        /// <summary>
        /// Checks if the topic contains user change events
        /// </summary>
        private static bool IsUserChangeEvent(string topic)
        {
            return topic.Contains("users", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Handles processing errors with different strategies
        /// </summary>
        private async Task HandleProcessingErrorAsync(
            ConsumeResult<string, string> consumeResult, 
            Exception exception, 
            CancellationToken cancellationToken)
        {
            var topic = consumeResult.Topic;
            var partition = consumeResult.Partition.Value;
            var offset = consumeResult.Offset.Value;

            // Error handling strategies:
            // 1. For transient errors (network, DB), retry
            // 2. For format errors, log and continue
            // 3. For critical errors, stop the service

            if (IsTransientError(exception))
            {
                _logger.LogWarning("Transient error processing message, will retry in 2s: {Error}", exception.Message);
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken); // Reduced from 5s
                return; // Don't commit, reprocess the message
            }

            if (IsFormatError(exception))
            {
                _logger.LogError(exception, "Message format error, skipping message from {Topic}[{Partition}] @ {Offset}", 
                    topic, partition, offset);
                
                
                _consumer?.Commit(consumeResult);
                return;
            }

            
            _logger.LogError(exception, "Unexpected error processing message, skipping");
            _consumer?.Commit(consumeResult);
        }

        /// <summary>
        /// Determines if an error is transient (network, database temporarily unavailable)
        /// </summary>
        private static bool IsTransientError(Exception exception)
        {
            return exception is TaskCanceledException ||
                   exception is TimeoutException ||
                   exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
                   exception.Message.Contains("connection", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines if an error is data format related
        /// </summary>
        private static bool IsFormatError(Exception exception)
        {
            return exception is JsonException ||
                   exception is ArgumentException ||
                   exception is FormatException;
        }

        /// <summary>
        /// Crea los topics necesarios si no existen (para entornos de testing)
        /// </summary>
        private async Task EnsureTopicsExist(CancellationToken cancellationToken)
        {
            try
            {
                if (_config == null)
                {
                    _logger.LogWarning("Kafka config is null, cannot ensure topics exist");
                    return;
                }
                
                using var adminClient = new AdminClientBuilder(new AdminClientConfig
                {
                    BootstrapServers = _config.BootstrapServers
                }).Build();

                // Check which topics exist
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));
                var existingTopics = metadata.Topics.Select(t => t.Topic).ToHashSet();

                var topicsToCreate = _topics.Where(topic => !existingTopics.Contains(topic)).ToList();

                if (topicsToCreate.Any())
                {
                    _logger.LogInformation("Creating missing topics for testing: {Topics}", string.Join(", ", topicsToCreate));

                    var topicSpecs = topicsToCreate.Select(topic => new TopicSpecification
                    {
                        Name = topic,
                        NumPartitions = 1,
                        ReplicationFactor = 1
                    }).ToList();

                    await adminClient.CreateTopicsAsync(topicSpecs);
                    _logger.LogInformation("Topics created successfully: {Topics}", string.Join(", ", topicsToCreate));
                }
                else
                {
                    _logger.LogDebug("All topics already exist");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not create topics automatically: {Error}", ex.Message);
                // Not critical, topics can be created manually or by Debezium
            }
        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public override void Dispose()
        {
            try
            {
                if (_consumer != null)
                {
                    try
                    {
                        _consumer.Close();
                    }
                    catch (ObjectDisposedException)
                    {
                        // Handle already disposed - ignore
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug("Error closing consumer: {Error}", ex.Message);
                    }
                    
                    try
                    {
                        _consumer.Dispose();
                    }
                    catch (ObjectDisposedException)
                    {
                        // Handle already disposed - ignore
                    }
                }
                _logger.LogInformation("Debezium Consumer Service disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing Debezium Consumer Service");
            }
            finally
            {
                base.Dispose();
            }
        }
    }
}
