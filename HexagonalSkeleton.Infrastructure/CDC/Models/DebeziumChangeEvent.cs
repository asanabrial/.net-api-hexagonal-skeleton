using System.Text.Json.Serialization;

namespace HexagonalSkeleton.Infrastructure.CDC.Models
{
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
}
