using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HexagonalSkeleton.Infrastructure.Persistence.Query.Documents
{
    /// <summary>
    /// Shared document definitions for MongoDB query operations
    /// Centralized to avoid duplication and maintain consistency
    /// </summary>
    
    /// <summary>
    /// Embedded document for full name
    /// </summary>
    public class FullNameDocument
    {
        [BsonElement("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [BsonElement("lastName")]
        public string LastName { get; set; } = string.Empty;

        [BsonElement("displayName")]
        public string DisplayName { get; set; } = string.Empty;
    }

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
