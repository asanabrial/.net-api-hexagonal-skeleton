using System.Linq.Expressions;
using HexagonalSkeleton.CommonCore.Data.Entity;

namespace HexagonalSkeleton.CommonCore.Data.Repository
{
    public interface IGenericRepository<TEntity> where TEntity : class, IEntity
    {
        List<TEntity> FindAll();

        Task<List<TEntity>> FindAllAsync(CancellationToken cancellationToken);

        List<TEntity> Find(Expression<Func<TEntity, bool>> where);

        Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> where, CancellationToken cancellationToken);

        TEntity? FindOne(Expression<Func<TEntity, bool>> where);

        Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> where, CancellationToken cancellationToken);

        TEntity? FindOne(int id);

        Task<TEntity?> FindOneAsync(int id, CancellationToken cancellationToken);

        Task Create(TEntity entity);

        Task CreateAsync(TEntity entity, CancellationToken cancellationToken);

        Task Update(int id, TEntity entity);

        Task Update(TEntity entity);

        Task HardDelete(int id);

        Task SoftDelete(int id);
    }
}
