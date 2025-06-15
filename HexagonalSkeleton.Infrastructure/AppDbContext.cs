using Microsoft.EntityFrameworkCore;
using HexagonalSkeleton.CommonCore.Data.Extension;

namespace HexagonalSkeleton.Infrastructure
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
