using Microsoft.EntityFrameworkCore;
using HexagonalSkeleton.Infrastructure.Extensions;
using HexagonalSkeleton.Infrastructure.Persistence.Command.Entities;

namespace HexagonalSkeleton.Infrastructure.Persistence.Command
{
    /// <summary>
    /// Command Database Context for write operations
    /// Optimized for transactional consistency and data integrity
    /// Uses PostgreSQL for ACID compliance
    /// Follows CQRS pattern - only for write operations
    /// </summary>
    public class CommandDbContext : DbContext
    {
        public CommandDbContext(DbContextOptions<CommandDbContext> options) : base(options)
        {
        }

        public virtual DbSet<UserCommandEntity> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure UserCommandEntity for write optimization
            modelBuilder.Entity<UserCommandEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Unique constraints for business rules
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.PhoneNumber).IsUnique();
                
                // String length constraints
                entity.Property(e => e.Email).HasMaxLength(150).IsRequired();
                entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
                entity.Property(e => e.PasswordSalt).HasMaxLength(255).IsRequired();
                entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.AboutMe).HasMaxLength(500);

                // Precision for coordinates
                entity.Property(e => e.Latitude).HasPrecision(18, 10);
                entity.Property(e => e.Longitude).HasPrecision(18, 10);

                // Audit fields
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                // Indexes for performance
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.IsDeleted);
                entity.HasIndex(e => e.LastLogin);
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Command store optimizations
                optionsBuilder.EnableSensitiveDataLogging(false);
                optionsBuilder.EnableDetailedErrors(false);
            }
        }
    }
}
