using System.Linq.Expressions;
using HexagonalSkeleton.CommonCore.Data.Entity;
using HexagonalSkeleton.CommonCore.Data.Extension;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace HexagonalSkeleton.CommonCore.Data.Repository
{
    public class GenericRepository<TEntity>(DbContext dbContext)
        where TEntity : class, IEntity, new()
    {
        internal readonly DbSet<TEntity> Repository = dbContext.Set<TEntity>();

        protected List<TEntity> FindAll(
            bool tracking = false)
            => tracking 
                ? [..Repository]
                : [..Repository.AsNoTracking()];
                                                                                                                                                                                                                                                                                                                
        protected async Task<List<TEntity>> FindAllAsync(
            bool tracking = false,
            CancellationToken cancellationToken = default)
            => tracking 
                ? await Repository.ToListAsync(cancellationToken)
                : await Repository.AsNoTracking().ToListAsync(cancellationToken);
        

        protected async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> where, CancellationToken cancellationToken = default)
            => await Repository.AnyAsync(where, cancellationToken);

        protected List<TEntity> Find<TProperty>(
            Expression<Func<TEntity, bool>> where,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, TProperty>> include,
            bool tracking = false)
            => [.. Queryable(tracking).ParseInclude(include).Where(where)];

        protected List<TEntity> Find(
            Expression<Func<TEntity, bool>> where,
            bool tracking = false)
            => [.. Queryable(tracking).Where(where)];

        protected async Task<List<TEntity>> FindAsync<TProperty>(
            Expression<Func<TEntity, bool>> where,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, TProperty>> include,
            bool tracking = false,
            CancellationToken cancellationToken = default)
            => await Queryable(tracking).ParseInclude(include).Where(where).ToListAsync(cancellationToken);

        protected async Task<List<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> where,
            bool tracking = false,
            CancellationToken cancellationToken = default)
            => await Queryable(tracking).Where(where).ToListAsync(cancellationToken);

        protected TEntity? FindOne<TProperty>(
            Expression<Func<TEntity, bool>> where,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, TProperty>>? include = null,
            bool tracking = false)
            => Queryable(tracking).ParseInclude(include).FirstOrDefault(where);

        protected async Task<TEntity?> FindOneAsync<TProperty>(
            Expression<Func<TEntity, bool>> where,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, TProperty>> include,
            bool tracking = false,
            CancellationToken cancellationToken = default)
            => await Queryable(tracking).ParseInclude(include).FirstOrDefaultAsync(where, cancellationToken);

        protected async Task<TEntity?> FindOneAsync(
            Expression<Func<TEntity, bool>> where,
            bool tracking = false,
            CancellationToken cancellationToken = default)
            => await Queryable(tracking).FirstOrDefaultAsync(where, cancellationToken);

        protected TEntity? FindOne<TProperty>(
            int id,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, TProperty>> include,
            bool tracking = false)
            => Queryable(tracking).ParseInclude(include).FirstOrDefault(e => e.Id == id);
        

        protected async Task<TEntity?> FindOneAsync<TProperty>(
            int id,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, TProperty>> include,
            bool tracking = false,
            CancellationToken cancellationToken = default)
            => await Queryable(tracking).ParseInclude(include).FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        protected async Task<TEntity?> FindOneAsync(
            int id,
            bool tracking = false,
            CancellationToken cancellationToken = default)
            => await Queryable(tracking).FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        protected TEntity Create(TEntity entity)
            => Repository.Add(entity).Entity;

        protected async Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
            => await Repository.AddAsync(entity, cancellationToken);
        protected Task Update(TEntity entity, params Expression<Func<TEntity, object?>>[] properties)
        {
            Detach(entity.Id);
            UpdateSpecificField(entity, properties);

            return Task.CompletedTask;
        }

        protected Task Update(TEntity entity)
        {
            Detach(entity.Id);
            Repository.Update(entity);
            return Task.CompletedTask;
        }

        protected Task HardDelete(int id)
        {
            Detach(id);
            Repository
                .Entry(new TEntity() { Id = id })
                .State = EntityState.Deleted;
            return Task.CompletedTask;
        }

        protected Task SoftDelete(int id)
        {
            Detach(id);
            var entity = new TEntity() { Id = id, IsDeleted = true };
            UpdateSpecificField(entity, p => p.IsDeleted);

            return Task.CompletedTask;
        }

        protected Task Restore(int id)
        {
            Detach(id);
            var entity = new TEntity() { Id = id, IsDeleted = false };
            UpdateSpecificField(entity, p => p.IsDeleted);

            return Task.CompletedTask;
        }

        private void Detach(int id)
        {
            var entityAttached = GetAttached(id);

            if (entityAttached is not null)
                Repository.Entry(entityAttached).State = EntityState.Detached;
        }

        private TEntity? GetAttached(int id)
            => Repository.Local.FirstOrDefault(e => e.Id == id);
        

        private void UpdateSpecificField(TEntity entity, params Expression<Func<TEntity, object?>>[] updatedProperties)
        {
            foreach (var property in updatedProperties)
                Repository.Entry(entity).Property(property).IsModified = true;
        }

        private IQueryable<TEntity> Queryable(bool tracking)
            => tracking ? Repository : Repository.AsNoTracking();
    }
}
