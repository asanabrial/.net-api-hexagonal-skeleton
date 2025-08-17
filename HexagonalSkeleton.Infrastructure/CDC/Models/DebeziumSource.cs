using System.Text.Json.Serialization;

namespace HexagonalSkeleton.Infrastructure.CDC.Models
{
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
}
