using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using HexagonalSkeleton.Infrastructure.Persistence.Entities;

namespace HexagonalSkeleton.Infrastructure.Extensions
{
    public static class ModelBuilderExtension
    {
        public static ModelBuilder SetDefaultGlobalFilters(this ModelBuilder modelBuilder)
        {
            Expression<Func<BaseEntity, bool>> filterExpr = bm => !bm.IsDeleted;
            foreach (var mutableEntityType in modelBuilder.Model.GetEntityTypes())
            {
                // check if current entity type is child of BaseEntity
                if (!mutableEntityType.ClrType.IsAssignableTo(typeof(BaseEntity))) continue;

                // modify expression to handle correct child type
                var parameter = Expression.Parameter(mutableEntityType.ClrType);
                var body = ReplacingExpressionVisitor.Replace(filterExpr.Parameters.First(), parameter, filterExpr.Body);
                var lambdaExpression = Expression.Lambda(body, parameter);

                // set filter
                mutableEntityType.SetQueryFilter(lambdaExpression);
            }

            return modelBuilder;
        }
    }
}
