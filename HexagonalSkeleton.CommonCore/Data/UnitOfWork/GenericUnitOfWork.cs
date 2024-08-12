using HexagonalSkeleton.CommonCore.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace HexagonalSkeleton.CommonCore.Data.UnitOfWork
{

    public class GenericUnitOfWork(DbContext context) : IGenericUnitOfWork
    {
        public bool SaveChanges()
        {
            LogChanges();
            return context.SaveChanges() > 0;
        }
        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
        {
            LogChanges();
            return await context.SaveChangesAsync(cancellationToken) > 0;
        }

        private void LogChanges()
        {
            var entries = context.ChangeTracker
                .Entries()
                .Where(e => e is { Entity: IEntity, State: EntityState.Added or EntityState.Modified });

            foreach (var entityEntry in entries)
            {
                var entity = (IEntity)entityEntry.Entity;
                if (!entity.IsDeleted)
                {
                    entity.UpdatedAt = DateTime.UtcNow;

                    if (entityEntry.State == EntityState.Added)
                        entity.CreatedAt = DateTime.UtcNow;
                }
                else
                {
                    entity.DeletedAt = DateTime.UtcNow;
                }
            }
        }

        public void Dispose()
        {
            context.Dispose();
            GC.SuppressFinalize(this);
        }

        public IDatabaseTransaction BeginTransaction() => new EntityDatabaseTransaction(context);
    }
}
