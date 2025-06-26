using HexagonalSkeleton.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Query;

namespace HexagonalSkeleton.Infrastructure.Extensions
{
    public static class QueryableExtension
    {
        public static IQueryable<TEntity> ParseInclude<TEntity, TProperty>(this IQueryable<TEntity> query,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, TProperty>>? include) where TEntity : BaseEntity
            => include is null ? query : include(query);
    }
}
