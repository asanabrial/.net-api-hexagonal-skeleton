using Microsoft.EntityFrameworkCore;
using HexagonalSkeleton.Infrastructure.Extensions;
using HexagonalSkeleton.Infrastructure.Persistence.Entities;

namespace HexagonalSkeleton.Infrastructure.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public virtual DbSet<UserEntity> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.SetDefaultGlobalFilters();
        }
    }
}
