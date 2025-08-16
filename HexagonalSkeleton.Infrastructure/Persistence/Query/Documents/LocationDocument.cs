using MongoDB.Bson.Serialization.Attributes;

namespace HexagonalSkeleton.Infrastructure.Persistence.Query.Documents
{
    /// <summary>
    /// Embedded document for location
    /// </summary>
    public class LocationDocument
    {
        [BsonElement("latitude")]
        public double Latitude { get; set; }

        [BsonElement("longitude")]
        public double Longitude { get; set; }

        [BsonElement("address")]
        public string Address { get; set; } = string.Empty;

        [BsonElement("city")]
        public string City { get; set; } = string.Empty;

        [BsonElement("country")]
        public string Country { get; set; } = string.Empty;

        /// <summary>
        /// Get GeoJSON coordinates [longitude, latitude] for MongoDB geospatial queries
        /// </summary>
        [BsonElement("coordinates")]
        public double[] Coordinates => new[] { Longitude, Latitude };
    }
}
