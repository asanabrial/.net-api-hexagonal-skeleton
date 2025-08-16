using System.Text.Json.Serialization;

namespace HexagonalSkeleton.Infrastructure.CDC.Models
{
    /// <summary>
    /// Datos de cambio del usuario
    /// </summary>
    public class UserChangeData
    {
        [JsonPropertyName("Id")]
        public Guid Id { get; set; }
        
        [JsonPropertyName("FirstName")]
        public string FirstName { get; set; } = string.Empty;
        
        [JsonPropertyName("LastName")]
        public string LastName { get; set; } = string.Empty;
        
        [JsonPropertyName("Email")]
        public string Email { get; set; } = string.Empty;
        
        [JsonPropertyName("PhoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [JsonPropertyName("Birthdate")]
        public string? BirthdateString { get; set; }
        
        [JsonPropertyName("Latitude")]
        public double? Latitude { get; set; }
        
        [JsonPropertyName("Longitude")]
        public double? Longitude { get; set; }
        
        [JsonPropertyName("AboutMe")]
        public string? AboutMe { get; set; }
        
        [JsonPropertyName("CreatedAt")]
        public string CreatedAtString { get; set; } = string.Empty;
        
        [JsonPropertyName("UpdatedAt")]
        public string? UpdatedAtString { get; set; }
        
        [JsonPropertyName("LastLogin")]
        public string? LastLoginString { get; set; }
        
        [JsonPropertyName("IsDeleted")]
        public bool IsDeleted { get; set; }
        
        [JsonPropertyName("DeletedAt")]
        public string? DeletedAtString { get; set; }
        
        // Métodos para convertir strings a DateTime
        public DateTime GetCreatedAt() => DateTime.TryParse(CreatedAtString, out var result) ? result : DateTime.UtcNow;
        public DateTime GetUpdatedAt() => DateTime.TryParse(UpdatedAtString, out var result) ? result : DateTime.UtcNow;
        public DateTime? GetLastLogin() => DateTime.TryParse(LastLoginString, out var result) ? result : null;
        public DateTime? GetDeletedAt() => DateTime.TryParse(DeletedAtString, out var result) ? result : null;
        public DateTime? GetBirthdate() => DateTime.TryParse(BirthdateString, out var result) ? result : null;
    }
}
