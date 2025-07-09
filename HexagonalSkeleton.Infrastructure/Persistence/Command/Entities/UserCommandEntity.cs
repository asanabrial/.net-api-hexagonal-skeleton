using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HexagonalSkeleton.Infrastructure.Persistence.Command.Entities
{
    /// <summary>
    /// Command-side entity for User aggregate
    /// Optimized for write operations and transactional consistency
    /// Maps to PostgreSQL for ACID compliance
    /// </summary>
    [Table("Users")]
    public class UserCommandEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string PasswordSalt { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public DateTime? Birthdate { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        [MaxLength(500)]
        public string AboutMe { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Update timestamp for audit purposes
        /// </summary>
        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Soft delete the entity
        /// </summary>
        public void Delete()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            UpdateTimestamp();
        }
    }
}
