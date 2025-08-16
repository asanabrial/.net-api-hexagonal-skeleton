using MongoDB.Bson.Serialization.Attributes;

namespace HexagonalSkeleton.Infrastructure.Persistence.Query.Models
{
    /// <summary>
    /// Result for database statistics
    /// </summary>
    [BsonIgnoreExtraElements]
    public class DatabaseStatsResult
    {
        [BsonElement("db")]
        public string Db { get; set; } = string.Empty;
        
        [BsonElement("collections")]
        public long Collections { get; set; }
        
        [BsonElement("views")]
        public long Views { get; set; }
        
        [BsonElement("objects")]
        public long Objects { get; set; }
        
        [BsonElement("avgObjSize")]
        public double AvgObjSize { get; set; }
        
        [BsonElement("dataSize")]
        public long DataSize { get; set; }
        
        [BsonElement("storageSize")]
        public long StorageSize { get; set; }
        
        [BsonElement("indexes")]
        public long Indexes { get; set; }
        
        [BsonElement("indexSize")]
        public long IndexSize { get; set; }
        
        [BsonElement("ok")]
        public double Ok { get; set; }
    }
}
