using HexagonalSkeleton.CommonCore.Data.Entity;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HexagonalSkeleton.CommonCore.Data.Extension
{
    public static class ModelBuilderExtension
    {
        public static ModelBuilder SetDefaultGlobalFilters(this ModelBuilder modelBuilder)
        {
            Expression<Func<IEntity, bool>> filterExpr = bm => !bm.IsDeleted;
            foreach (var mutableEntityType in modelBuilder.Model.GetEntityTypes())
            {
                // check if current entity type is child of BaseModel
                if (!mutableEntityType.ClrType.IsAssignableTo(typeof(IEntity))) continue;

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
