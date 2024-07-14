using HexagonalSkeleton.API.Features.User.Domain;
using HexagonalSkeleton.CommonCore.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace HexagonalSkeleton.API.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public virtual DbSet<UserEntity> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
