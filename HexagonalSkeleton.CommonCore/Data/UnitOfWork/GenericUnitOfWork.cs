using HexagonalSkeleton.CommonCore.Data.Entity;
using HexagonalSkeleton.CommonCore.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace HexagonalSkeleton.CommonCore.Data.UnitOfWork
{

    public class GenericUnitOfWork(DbContext context) : IGenericUnitOfWork
    {
        public bool SaveChanges() => context.SaveChanges() > 0;
        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken) =>
            await context.SaveChangesAsync(cancellationToken) > 0;

        public void Dispose()
        {
            context.Dispose();
            GC.SuppressFinalize(this);
        }

        public IDatabaseTransaction BeginTransaction() => new EntityDatabaseTransaction(context);
    }
}
