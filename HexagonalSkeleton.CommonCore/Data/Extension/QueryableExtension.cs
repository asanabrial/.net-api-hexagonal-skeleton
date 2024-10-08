﻿using HexagonalSkeleton.CommonCore.Data.Entity;
using Microsoft.EntityFrameworkCore.Query;

namespace HexagonalSkeleton.CommonCore.Data.Extension
{
    public static class QueryableExtension
    {
        public static IQueryable<TEntity> ParseInclude<TEntity, TProperty>(this IQueryable<TEntity> query,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, TProperty>>? include) where TEntity : IEntity
            => include is null ? query : include(query);
    }
}
