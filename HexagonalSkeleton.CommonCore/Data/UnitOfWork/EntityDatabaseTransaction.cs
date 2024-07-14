using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;

namespace HexagonalSkeleton.CommonCore.Data.UnitOfWork
{
    public class EntityDatabaseTransaction(DbContext context) : IDatabaseTransaction
    {
        public IDbContextTransaction Transaction { get; } = context.Database.BeginTransaction();

        public void Commit()
        {
            Transaction.Commit();
        }

        public void Rollback()
        {
            Transaction.Rollback();
        }

        public void Dispose()
        {
            Transaction.Dispose();
        }
    }
}
