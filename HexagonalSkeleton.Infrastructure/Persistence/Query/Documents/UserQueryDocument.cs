using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HexagonalSkeleton.Infrastructure.Persistence.Query.Documents
{
    /// <summary>
    /// Query-side document for User aggregate
    /// Optimized for read operations and complex queries
    /// Denormalized structure for MongoDB
    /// </summary>
    [BsonIgnoreExtraElements]
    public class UserQueryDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("fullName")]
        public FullNameDocument FullName { get; set; } = new();

        [BsonElement("phoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;

        [BsonElement("birthdate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? Birthdate { get; set; }

        [BsonElement("location")]
        public LocationDocument Location { get; set; } = new();

        [BsonElement("aboutMe")]
        public string AboutMe { get; set; } = string.Empty;

        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? UpdatedAt { get; set; }

        [BsonElement("lastLogin")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? LastLogin { get; set; }

        [BsonElement("isDeleted")]
        public bool IsDeleted { get; set; }

        [BsonElement("deletedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? DeletedAt { get; set; }

        // Computed fields for query optimization
        [BsonElement("searchTerms")]
        public List<string> SearchTerms { get; set; } = new();

        [BsonElement("age")]
        public int? Age { get; set; }

        [BsonElement("profileCompleteness")]
        public double ProfileCompleteness { get; set; }
    }
}
