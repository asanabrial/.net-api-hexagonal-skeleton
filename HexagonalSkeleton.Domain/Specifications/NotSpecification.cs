using System.Linq.Expressions;

namespace HexagonalSkeleton.Domain.Specifications
{
    /// <summary>
    /// Simple NOT specification - Easy to understand
    /// </summary>
    internal class NotSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _specification;

        public NotSpecification(ISpecification<T> specification)
        {
            _specification = specification ?? throw new ArgumentNullException(nameof(specification));
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var expr = _specification.ToExpression();
            
            // Simple approach: Use Expression.Invoke
            var param = Expression.Parameter(typeof(T), "x");
            var body = Expression.Not(Expression.Invoke(expr, param));

            return Expression.Lambda<Func<T, bool>>(body, param);
        }
    }
}
