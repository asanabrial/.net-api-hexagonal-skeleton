using System.Linq.Expressions;
using HexagonalSkeleton.CommonCore.Data.Entity;
using Microsoft.EntityFrameworkCore.Query;

namespace HexagonalSkeleton.CommonCore.Data.Repository
{
    public interface IGenericRepository<TEntity> where TEntity : class, IEntity
    {
        public List<TEntity> FindAll(bool tracking = false);

        Task<List<TEntity>> FindAllAsync(bool tracking = false, CancellationToken cancellationToken = default);


        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> where, CancellationToken cancellationToken = default);

        List<TEntity> Find<TProperty>(
            Expression<Func<TEntity, bool>> where,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, TProperty>> include,
            bool tracking = false);

        List<TEntity> Find(
            Expression<Func<TEntity, bool>> where,
            bool tracking = false);

        Task<List<TEntity>> FindAsync<TProperty>(
            Expression<Func<TEntity, bool>> where,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, TProperty>> include,
            bool tracking = false,
            CancellationToken cancellationToken = default);

        Task<List<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> where,
            bool tracking = false,
            CancellationToken cancellationToken = default);

        TEntity? FindOne<TProperty>(
            Expression<Func<TEntity, bool>> where,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, TProperty>>? include = null,
            bool tracking = false);

        Task<TEntity?> FindOneAsync<TProperty>(
            Expression<Func<TEntity, bool>> where,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, TProperty>> include,
            bool tracking = false,
            CancellationToken cancellationToken = default);

        Task<TEntity?> FindOneAsync(
            Expression<Func<TEntity, bool>> where,
            bool tracking = false,
            CancellationToken cancellationToken = default);

        TEntity? FindOne<TProperty>(
            int id,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, TProperty>> include,
            bool tracking = false);


        Task<TEntity?> FindOneAsync<TProperty>(
            int id,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, TProperty>> include,
            bool tracking = false,
            CancellationToken cancellationToken = default);

        Task<TEntity?> FindOneAsync(
            int id,
            bool tracking = false,
            CancellationToken cancellationToken = default);

        TEntity Create(TEntity entity);

        Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task Update(TEntity entity, params Expression<Func<TEntity, object>>[] properties);

        Task Update(TEntity entity);

        Task HardDelete(int id);

        Task SoftDelete(int id);
    }
}
