using System.Text.Json;
using HexagonalSkeleton.Infrastructure.CDC.Configuration;
using HexagonalSkeleton.Infrastructure.CDC.Models;
using HexagonalSkeleton.Infrastructure.Persistence.Query;
using HexagonalSkeleton.Infrastructure.Persistence.Query.Documents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly CdcOptions _cdcOptions;

        public DebeziumEventProcessor(
            QueryDbContext queryDbContext,
            ILogger<DebeziumEventProcessor> logger,
            IOptions<CdcOptions> cdcOptions)
        {
            _queryDbContext = queryDbContext;
            _logger = logger;
            _cdcOptions = cdcOptions.Value;
            
            _logger.LogInformation("🔧 CDC Configuration - TargetDatabase: {Database}, ProcessOnlyTargetDatabase: {ProcessOnly}", 
                _cdcOptions.TargetDatabase, _cdcOptions.ProcessOnlyTargetDatabase);
        }

        /// <summary>
        /// Valida si el evento proviene de la base de datos objetivo
        /// </summary>
        private bool IsValidDatabaseSource(string? sourceDatabaseName)
        {
            // Si no está configurado para filtrar, procesa todos los eventos
            if (!_cdcOptions.ProcessOnlyTargetDatabase || string.IsNullOrEmpty(_cdcOptions.TargetDatabase))
                return true;
                
            // Filtrar por base de datos objetivo
            return string.Equals(sourceDatabaseName, _cdcOptions.TargetDatabase, StringComparison.OrdinalIgnoreCase);
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

                _logger.LogWarning("🔍 Parsed event - Op: {Op}, Table: {Table}, Source: {Source}, Database: {Database}",
                    changeEvent.Payload.Op, changeEvent.Payload.Source?.Table, changeEvent.Payload.Source?.Name, changeEvent.Payload.Source?.Db);

                // ✨ Filtro elegante: Solo procesar eventos de la base de datos objetivo
                var sourceDatabaseName = changeEvent.Payload.Source?.Db;
                if (!IsValidDatabaseSource(sourceDatabaseName))
                {
                    _logger.LogDebug("⏭️ Skipping event from database '{Database}' - not target database '{Target}'", 
                        sourceDatabaseName, _cdcOptions.TargetDatabase);
                    return true; // No es error, simplemente no procesamos este evento
                }

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
                    Birthdate = userData.GetBirthdate(),
                    Location = new LocationDocument
                    {
                        Latitude = userData.Latitude ?? 0.0,
                        Longitude = userData.Longitude ?? 0.0
                    },
                    AboutMe = userData.AboutMe ?? string.Empty,
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
                    Birthdate = userData.GetBirthdate(),
                    Location = new LocationDocument
                    {
                        Latitude = userData.Latitude ?? 0.0,
                        Longitude = userData.Longitude ?? 0.0
                    },
                    AboutMe = userData.AboutMe ?? string.Empty,
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
}
