using HexagonalSkeleton.API.Features.User.Domain;
using HexagonalSkeleton.CommonCore.Data.UnitOfWork;

namespace HexagonalSkeleton.API.Data
{
    public interface IUnitOfWork : IDisposable
    {
        public IUserRepository Users { get; }

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
