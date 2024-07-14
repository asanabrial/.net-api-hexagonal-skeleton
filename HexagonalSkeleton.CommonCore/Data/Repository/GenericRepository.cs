using System.Linq.Expressions;
using HexagonalSkeleton.CommonCore.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace HexagonalSkeleton.CommonCore.Data.Repository
{
    public class GenericRepository<TEntity>(DbContext dbContext) : IGenericRepository<TEntity>
        where TEntity : class, IEntity, new()
    {
        private readonly DbSet<TEntity> _repository = dbContext.Set<TEntity>();

        public List<TEntity> FindAll()
        {
            return _repository.Where(w => !w.IsDeleted).AsNoTracking().ToList();
        }

        public async Task<List<TEntity>> FindAllAsync(CancellationToken cancellationToken)
        {
            return await _repository.Where(w => !w.IsDeleted).AsNoTracking().ToListAsync(cancellationToken);
        }

        public List<TEntity> Find(Expression<Func<TEntity, bool>> where)
        {
            if (where is null) throw new ArgumentNullException(nameof(where));
            return _repository
                .AsNoTracking()
                .Where(where)
                .ToList();
        }

        public async Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> where, CancellationToken cancellationToken)
        {
            if (where is null) throw new ArgumentNullException(nameof(where));
            return await _repository
                .AsNoTracking()
                .Where(where)
                .ToListAsync(cancellationToken);
        }

        public TEntity? FindOne(Expression<Func<TEntity, bool>> where)
        {
            if (where is null) throw new ArgumentNullException(nameof(where));
            return _repository
                .AsNoTracking()
                .FirstOrDefault(where);
        }

        public async Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> where, CancellationToken cancellationToken)
        {
            if (where is null) throw new ArgumentNullException(nameof(where));
            return await _repository
                .AsNoTracking()
                .FirstOrDefaultAsync(where, cancellationToken);
        }

        public TEntity? FindOne(int id)
        {
            return _repository
                .AsNoTracking()
                .FirstOrDefault(e => e.Id == id && !e.IsDeleted);
        }

        public async Task<TEntity?> FindOneAsync(int id, CancellationToken cancellationToken)
        {
            return await _repository
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
        }

        public Task Create(TEntity entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            _repository.Add(entity);
            return Task.CompletedTask;
        }

        public async Task CreateAsync(TEntity entity, CancellationToken cancellationToken)
        {
            entity.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(entity, cancellationToken);
        }

        public Task Update(int id, TEntity entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            var entry = _repository.Entry(new TEntity() { Id = id });
            entry.CurrentValues.SetValues(entity);
            entry.State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public Task Update(TEntity entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _repository.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public Task HardDelete(int id)
        {
            var entity = new TEntity() { Id = id };
            _repository.Entry(entity).State = EntityState.Deleted;
            return Task.CompletedTask;
        }

        public async Task HardDeleteAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken)
            => await _repository.Where(expression).ExecuteDeleteAsync(cancellationToken);

        public Task SoftDelete(int id)
        {
            var entry = _repository.Entry(new TEntity() { Id = id });
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = DateTime.UtcNow;
            entry.State = EntityState.Modified;

            return Task.CompletedTask;
        }
    }
}
