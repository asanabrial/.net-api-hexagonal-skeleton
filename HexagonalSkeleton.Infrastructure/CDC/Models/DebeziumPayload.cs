using System.Text.Json.Serialization;

namespace HexagonalSkeleton.Infrastructure.CDC.Models
{
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
}
