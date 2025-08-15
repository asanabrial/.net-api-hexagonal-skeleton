using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HexagonalSkeleton.Test.TestInfrastructure.Services
{
    /// <summary>
    /// Test CDC Event Publisher that simulates Debezium events for testing
    /// Publishes CDC events directly to Kafka simulating Debezium behavior
    /// </summary>
    public class TestCdcEventPublisher : IAsyncDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<TestCdcEventPublisher> _logger;
        
        public TestCdcEventPublisher(
            IOptions<ProducerConfig> config,
            ILogger<TestCdcEventPublisher> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            var producerConfig = config?.Value ?? throw new ArgumentNullException(nameof(config));
            
            _producer = new ProducerBuilder<string, string>(producerConfig)
                .SetErrorHandler((_, error) =>
                {
                    _logger.LogError("Test CDC Producer Error: {Reason}", error.Reason);
                })
                .Build();
                
            _logger.LogInformation("Test CDC Event Publisher initialized");
        }

        /// <summary>
        /// Publishes a user creation CDC event to simulate Debezium behavior
        /// </summary>
        public async Task PublishUserCreatedEventAsync(
            Guid userId, 
            string firstName, 
            string lastName, 
            string email, 
            string phoneNumber,
            DateTime? createdAt = null,
            CancellationToken cancellationToken = default)
        {
            var changeEvent = CreateUserChangeEvent(
                userId, firstName, lastName, email, phoneNumber, 
                createdAt ?? DateTime.UtcNow, "c"); // "c" = create
                
            await PublishEventAsync(changeEvent, userId.ToString(), cancellationToken);
        }

        /// <summary>
        /// Publishes a user update CDC event to simulate Debezium behavior
        /// </summary>
        public async Task PublishUserUpdatedEventAsync(
            Guid userId, 
            string firstName, 
            string lastName, 
            string email, 
            string phoneNumber,
            DateTime? updatedAt = null,
            CancellationToken cancellationToken = default)
        {
            var changeEvent = CreateUserChangeEvent(
                userId, firstName, lastName, email, phoneNumber, 
                updatedAt ?? DateTime.UtcNow, "u"); // "u" = update
                
            await PublishEventAsync(changeEvent, userId.ToString(), cancellationToken);
        }

        /// <summary>
        /// Publishes a user deletion CDC event to simulate Debezium behavior
        /// </summary>
        public async Task PublishUserDeletedEventAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var changeEvent = CreateUserDeleteEvent(userId);
            await PublishEventAsync(changeEvent, userId.ToString(), cancellationToken);
        }

        private object CreateUserChangeEvent(
            Guid userId, 
            string firstName, 
            string lastName, 
            string email, 
            string phoneNumber,
            DateTime timestamp,
            string operation)
        {
            var userData = new
            {
                Id = userId,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PhoneNumber = phoneNumber,
                CreatedAt = timestamp,
                UpdatedAt = timestamp,
                IsDeleted = false,
                LastLogin = timestamp
            };

            return new
            {
                before = operation == "c" ? null : userData, // null for create
                after = userData,
                source = new
                {
                    version = "2.5.4.Final",
                    connector = "postgresql",
                    name = "hexagonal-postgres-test",
                    ts_ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    snapshot = "false",
                    db = "HexagonalSkeleton",
                    sequence = "[null,\"31437792\"]",
                    schema = "public",
                    table = "Users",
                    txId = 999,
                    lsn = 31437792,
                    xmin = (object?)null
                },
                op = operation,
                ts_ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                transaction = (object?)null
            };
        }

        private object CreateUserDeleteEvent(Guid userId)
        {
            return new
            {
                before = new
                {
                    Id = userId,
                    IsDeleted = false
                },
                after = (object?)null, // null for delete
                source = new
                {
                    version = "2.5.4.Final",
                    connector = "postgresql",
                    name = "hexagonal-postgres-test",
                    ts_ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    snapshot = "false",
                    db = "HexagonalSkeleton",
                    sequence = "[null,\"31437792\"]",
                    schema = "public",
                    table = "Users",
                    txId = 999,
                    lsn = 31437792,
                    xmin = (object?)null
                },
                op = "d", // delete
                ts_ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                transaction = (object?)null
            };
        }

        private async Task PublishEventAsync(object changeEvent, string key, CancellationToken cancellationToken)
        {
            try
            {
                var eventJson = JsonSerializer.Serialize(changeEvent);
                var topic = "hexagonal-postgres.public.users"; // Match actual table name in lowercase

                var message = new Message<string, string>
                {
                    Key = key,
                    Value = eventJson
                };

                var result = await _producer.ProduceAsync(topic, message, cancellationToken);
                
                _logger.LogDebug("Published test CDC event to {Topic}: {Key} -> {Operation}", 
                    result.Topic, key, GetOperationFromEvent(changeEvent));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish test CDC event for key: {Key}", key);
                throw;
            }
        }

        private string GetOperationFromEvent(object changeEvent)
        {
            if (changeEvent is not Dictionary<string, object> dict || !dict.TryGetValue("op", out var op))
                return "unknown";
            return op?.ToString() ?? "unknown";
        }

        public ValueTask DisposeAsync()
        {
            try
            {
                _producer?.Flush(TimeSpan.FromSeconds(10));
                _producer?.Dispose();
                _logger.LogInformation("Test CDC Event Publisher disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing Test CDC Event Publisher");
            }
            
            return ValueTask.CompletedTask;
        }
    }
}
