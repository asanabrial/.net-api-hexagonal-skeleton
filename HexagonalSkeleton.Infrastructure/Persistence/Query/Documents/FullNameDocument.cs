using MongoDB.Bson.Serialization.Attributes;

namespace HexagonalSkeleton.Infrastructure.Persistence.Query.Documents
{
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
}
