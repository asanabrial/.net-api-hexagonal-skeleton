using System.Text.Json;
using System.Text.Json.Serialization;
using HexagonalSkeleton.Infrastructure.Persistence.Query;
using HexagonalSkeleton.Infrastructure.Persistence.Query.Documents;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace HexagonalSkeleton.Infrastructure.CDC
{
    /// <summary>
    /// Procesador de eventos CDC de Debezium
    /// Transforma eventos de PostgreSQL y los sincroniza con MongoDB
    /// </summary>
    public class DebeziumEventProcessor
    {
        private readonly QueryDbContext _queryDbContext;
        private readonly ILogger<DebeziumEventProcessor> _logger;

        public DebeziumEventProcessor(
            QueryDbContext queryDbContext,
            ILogger<DebeziumEventProcessor> logger)
        {
            _queryDbContext = queryDbContext;
            _logger = logger;
        }

        /// <summary>
        /// Procesa un evento CDC de Debezium
        /// </summary>
        public async Task<bool> ProcessChangeEventAsync(string eventPayload, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(eventPayload))
                {
                    _logger.LogWarning("🚨 Empty payload received");
                    return false;
                }

                _logger.LogWarning("🔍 Raw Debezium payload: {Payload}", eventPayload);

                // Intentar deserializar con diferentes opciones para debugging
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var changeEvent = JsonSerializer.Deserialize<DebeziumChangeEvent>(eventPayload, options);
                if (changeEvent?.Payload == null)
                {
                    _logger.LogWarning("🚨 Parsed changeEvent is null or Payload is null");
                    _logger.LogWarning("🔍 changeEvent?.Schema type: {SchemaType}", changeEvent?.Schema?.GetType().Name ?? "null");
                    _logger.LogWarning("🔍 changeEvent?.Payload: {PayloadInfo}", changeEvent?.Payload?.ToString() ?? "null");
                    return false;
                }

                _logger.LogWarning("🔍 Parsed event - Op: {Op}, Table: {Table}, Source: {Source}",
                    changeEvent.Payload.Op, changeEvent.Payload.Source?.Table, changeEvent.Payload.Source?.Name);

                // Procesar evento según operación
                return changeEvent.Payload.Op switch
                {
                    "c" => await HandleUserCreatedAsync(changeEvent.Payload.After, cancellationToken), // Create
                    "u" => await HandleUserUpdatedAsync(changeEvent.Payload.After, cancellationToken), // Update
                    "d" => await HandleUserDeletedAsync(changeEvent.Payload.Before, cancellationToken), // Delete
                    "r" => await HandleUserCreatedAsync(changeEvent.Payload.After, cancellationToken), // Read (snapshot)
                    _ => await HandleUnknownOperationAsync(changeEvent, cancellationToken)
                };
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "🚨 JSON parsing error: {Error}", jsonEx.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🚨 Error processing user change event: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Maneja la creación de un usuario
        /// </summary>
        private async Task<bool> HandleUserCreatedAsync(UserChangeData? userData, CancellationToken cancellationToken)
        {
            if (userData == null)
            {
                _logger.LogWarning("🚨 User data is null for creation event");
                return false;
            }

            try
            {
                _logger.LogInformation("👤 Creating user in MongoDB: {UserId}", userData.Id);

                var userQueryDocument = new UserQueryDocument
                {
                    Id = userData.Id,
                    FullName = new FullNameDocument
                    {
                        FirstName = userData.FirstName,
                        LastName = userData.LastName
                    },
                    Email = userData.Email,
                    PhoneNumber = userData.PhoneNumber,
                    CreatedAt = userData.GetCreatedAt(),
                    UpdatedAt = userData.GetUpdatedAt(),
                    IsDeleted = userData.IsDeleted,
                    DeletedAt = userData.GetDeletedAt(),
                    LastLogin = userData.GetLastLogin()
                };

                var filter = Builders<UserQueryDocument>.Filter.Eq(x => x.Id, userData.Id);
                await _queryDbContext.Users.ReplaceOneAsync(
                    filter,
                    userQueryDocument,
                    new ReplaceOptions { IsUpsert = true },
                    cancellationToken);

                _logger.LogInformation("✅ User created in MongoDB: {UserId}", userData.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🚨 Error creating user in MongoDB: {UserId}, Error: {Error}", userData.Id, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Maneja la actualización de un usuario
        /// </summary>
        private async Task<bool> HandleUserUpdatedAsync(UserChangeData? userData, CancellationToken cancellationToken)
        {
            if (userData == null)
            {
                _logger.LogWarning("🚨 User data is null for update event");
                return false;
            }

            try
            {
                _logger.LogInformation("🔄 Updating user in MongoDB: {UserId}", userData.Id);

                var userQueryDocument = new UserQueryDocument
                {
                    Id = userData.Id,
                    FullName = new FullNameDocument
                    {
                        FirstName = userData.FirstName,
                        LastName = userData.LastName
                    },
                    Email = userData.Email,
                    PhoneNumber = userData.PhoneNumber,
                    CreatedAt = userData.GetCreatedAt(),
                    UpdatedAt = userData.GetUpdatedAt(),
                    IsDeleted = userData.IsDeleted,
                    DeletedAt = userData.GetDeletedAt(),
                    LastLogin = userData.GetLastLogin()
                };

                var filter = Builders<UserQueryDocument>.Filter.Eq(x => x.Id, userData.Id);
                var result = await _queryDbContext.Users.ReplaceOneAsync(
                    filter,
                    userQueryDocument,
                    new ReplaceOptions { IsUpsert = true },
                    cancellationToken);

                if (result.MatchedCount > 0 || result.UpsertedId != null)
                {
                    _logger.LogInformation("✅ User updated in MongoDB: {UserId}", userData.Id);
                    return true;
                }
                else
                {
                    _logger.LogWarning("⚠️ User not found for update in MongoDB: {UserId}", userData.Id);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🚨 Error updating user in MongoDB: {UserId}, Error: {Error}", userData.Id, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Maneja la eliminación lógica de un usuario
        /// </summary>
        private async Task<bool> HandleUserDeletedAsync(UserChangeData? userData, CancellationToken cancellationToken)
        {
            if (userData == null)
            {
                _logger.LogWarning("🚨 User data is null for deletion event");
                return false;
            }

            try
            {
                _logger.LogInformation("🗑️ Marking user as deleted in MongoDB: {UserId}", userData.Id);

                var filter = Builders<UserQueryDocument>.Filter.Eq(x => x.Id, userData.Id);
                var update = Builders<UserQueryDocument>.Update.Set(x => x.IsDeleted, true);

                var result = await _queryDbContext.Users.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

                if (result.MatchedCount > 0)
                {
                    _logger.LogInformation("✅ User marked as deleted in MongoDB: {UserId}", userData.Id);
                    return true;
                }
                else
                {
                    _logger.LogWarning("⚠️ User not found for deletion in MongoDB: {UserId}", userData.Id);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🚨 Error deleting user in MongoDB: {UserId}, Error: {Error}", userData.Id, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Maneja operaciones desconocidas
        /// </summary>
        private async Task<bool> HandleUnknownOperationAsync(DebeziumChangeEvent changeEvent, CancellationToken cancellationToken)
        {
            _logger.LogWarning("❓ Unknown operation received: {Op} for table: {Table}",
                changeEvent.Payload?.Op, changeEvent.Payload?.Source?.Table);

            // Retornar true para que no se considere un error de procesamiento
            await Task.CompletedTask;
            return true;
        }
    }

    /// <summary>
    /// Modelo del evento de cambio de Debezium (wrapper completo)
    /// </summary>
    public class DebeziumChangeEvent
    {
        [JsonPropertyName("schema")]
        public object? Schema { get; set; } // Schema de Debezium (no lo procesamos)
        
        [JsonPropertyName("payload")]
        public DebeziumPayload? Payload { get; set; }
    }

    /// <summary>
    /// Payload del evento de Debezium
    /// </summary>
    public class DebeziumPayload
    {
        [JsonPropertyName("before")]
        public UserChangeData? Before { get; set; }
        
        [JsonPropertyName("after")]
        public UserChangeData? After { get; set; }
        
        [JsonPropertyName("source")]
        public DebeziumSource? Source { get; set; }
        
        [JsonPropertyName("op")]
        public string Op { get; set; } = string.Empty; // c=create, u=update, d=delete, r=read
        
        [JsonPropertyName("ts_ms")]
        public long? TsMs { get; set; }
        
        [JsonPropertyName("transaction")]
        public object? Transaction { get; set; }
    }

    /// <summary>
    /// Información de origen del evento
    /// </summary>
    public class DebeziumSource
    {
        [JsonPropertyName("version")]
        public string? Version { get; set; }
        
        [JsonPropertyName("connector")]
        public string? Connector { get; set; }
        
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [JsonPropertyName("ts_ms")]
        public long? TsMs { get; set; }
        
        [JsonPropertyName("snapshot")]
        public string? Snapshot { get; set; }
        
        [JsonPropertyName("db")]
        public string? Db { get; set; }
        
        [JsonPropertyName("sequence")]
        public string? Sequence { get; set; }
        
        [JsonPropertyName("schema")]
        public string? Schema { get; set; }
        
        [JsonPropertyName("table")]
        public string? Table { get; set; }
        
        [JsonPropertyName("txId")]
        public long? TxId { get; set; }
        
        [JsonPropertyName("lsn")]
        public long? Lsn { get; set; }
        
        [JsonPropertyName("xmin")]
        public long? Xmin { get; set; }
    }

    /// <summary>
    /// Datos de cambio del usuario
    /// </summary>
    public class UserChangeData
    {
        [JsonPropertyName("Id")]
        public Guid Id { get; set; }
        
        [JsonPropertyName("FirstName")]
        public string FirstName { get; set; } = string.Empty;
        
        [JsonPropertyName("LastName")]
        public string LastName { get; set; } = string.Empty;
        
        [JsonPropertyName("Email")]
        public string Email { get; set; } = string.Empty;
        
        [JsonPropertyName("PhoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [JsonPropertyName("CreatedAt")]
        public string CreatedAtString { get; set; } = string.Empty;
        
        [JsonPropertyName("UpdatedAt")]
        public string? UpdatedAtString { get; set; }
        
        [JsonPropertyName("LastLogin")]
        public string? LastLoginString { get; set; }
        
        [JsonPropertyName("IsDeleted")]
        public bool IsDeleted { get; set; }
        
        [JsonPropertyName("DeletedAt")]
        public string? DeletedAtString { get; set; }
        
        // Métodos para convertir strings a DateTime
        public DateTime GetCreatedAt() => DateTime.TryParse(CreatedAtString, out var result) ? result : DateTime.UtcNow;
        public DateTime GetUpdatedAt() => DateTime.TryParse(UpdatedAtString, out var result) ? result : DateTime.UtcNow;
        public DateTime? GetLastLogin() => DateTime.TryParse(LastLoginString, out var result) ? result : null;
        public DateTime? GetDeletedAt() => DateTime.TryParse(DeletedAtString, out var result) ? result : null;
    }
}
