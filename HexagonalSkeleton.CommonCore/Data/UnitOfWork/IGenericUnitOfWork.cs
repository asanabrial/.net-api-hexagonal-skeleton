using HexagonalSkeleton.CommonCore.Data.Entity;
using HexagonalSkeleton.CommonCore.Data.Repository;

namespace HexagonalSkeleton.CommonCore.Data.UnitOfWork
{
    public interface IGenericUnitOfWork : IDisposable
    {
        IDatabaseTransaction BeginTransaction();

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <returns>If the number of state entries written to the database is greater than zero,
        /// return true, in otherwise false</returns>
        bool SaveChanges();

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains true if
        /// the number of state entries written to the database is greater than zero, in otherwise false</returns>
        Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
