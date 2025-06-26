using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HexagonalSkeleton.Infrastructure.Persistence.Entities
{
    /// <summary>
    /// Base entity for infrastructure persistence concerns.
    /// This represents the technical/infrastructure aspect of entities,
    /// separate from domain concepts.
    /// </summary>
    public interface IBaseEntity
    {
        int Id { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime? UpdatedAt { get; set; }
        DateTime? DeletedAt { get; set; }

        [DefaultValue(false)]
        bool IsDeleted { get; set; }
    }    /// <summary>
    /// Base implementation for database entities.
    /// </summary>
    public abstract class BaseEntity : IBaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        [DefaultValue(false)]
        public bool IsDeleted { get; set; }
    }
}
