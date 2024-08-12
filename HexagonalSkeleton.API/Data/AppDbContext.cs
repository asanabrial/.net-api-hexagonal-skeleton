using HexagonalSkeleton.API.Features.User.Domain;
using Microsoft.EntityFrameworkCore;

namespace HexagonalSkeleton.API.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public virtual DbSet<UserEntity> Users { get; set; }
    }
}
