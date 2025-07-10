using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace HexagonalSkeleton.Infrastructure.Persistence.Query.Documents
{
    /// <summary>
    /// User document model optimized for read operations in MongoDB
    /// Following CQRS pattern with dedicated read model
    /// </summary>
    public class UserDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        
        [BsonElement("email")]
        [BsonRequired]
        public string Email { get; set; } = null!;
        
        [BsonElement("firstName")]
        [BsonRequired]
        public string FirstName { get; set; } = null!;
        
        [BsonElement("lastName")]
        [BsonRequired]
        public string LastName { get; set; } = null!;
        
        [BsonElement("phoneNumber")]
        public string PhoneNumber { get; set; } = null!;
        
        [BsonElement("birthdate")]
        public DateTime? Birthdate { get; set; }
        
        [BsonElement("age")]
        public int Age { get; set; }
        
        [BsonElement("isAdult")]
        public bool IsAdult { get; set; }
        
        [BsonElement("aboutMe")]
        public string AboutMe { get; set; } = string.Empty;
        
        [BsonElement("profileImageName")]
        public string? ProfileImageName { get; set; }
        
        [BsonElement("lastLogin")]
        public DateTime LastLogin { get; set; }
        
        [BsonElement("location")]
        public GeoJsonPoint<GeoJson2DGeographicCoordinates> Location { get; set; } = null!;
        
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }
        
        [BsonElement("updatedAt")]
        public DateTime? UpdatedAt { get; set; }
        
        [BsonElement("deletedAt")]
        public DateTime? DeletedAt { get; set; }
        
        [BsonElement("isDeleted")]
        public bool IsDeleted { get; set; }
        
        // Additional denormalized fields for query optimization
        
        [BsonElement("fullName")]
        public string FullName { get; set; } = null!; // Denormalized for text search
        
        [BsonElement("tags")]
        public List<string> Tags { get; set; } = new();
        
        [BsonElement("isActive")]
        public bool IsActive { get; set; }
        
        [BsonElement("searchTerms")]
        public List<string> SearchTerms { get; set; } = new();
        
        [BsonElement("syncedAt")]
        public DateTime? SyncedAt { get; set; }
        
        [BsonElement("version")]
        public int Version { get; set; }
        
        // Helper methods
        
        /// <summary>
        /// Gets the latitude from the location point
        /// </summary>
        public double? GetLatitude() => Location?.Coordinates?.Latitude;
        
        /// <summary>
        /// Gets the longitude from the location point
        /// </summary>
        public double? GetLongitude() => Location?.Coordinates?.Longitude;
    }
}
