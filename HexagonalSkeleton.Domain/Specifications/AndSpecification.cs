using System.Linq.Expressions;

namespace HexagonalSkeleton.Domain.Specifications
{
    /// <summary>
    /// Simple AND specification - Easy to understand
    /// </summary>
    internal class AndSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _left;
        private readonly ISpecification<T> _right;

        public AndSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var leftExpr = _left.ToExpression();
            var rightExpr = _right.ToExpression();
            
            // Simple approach: Use Expression.Invoke (works for 99% of cases)
            var param = Expression.Parameter(typeof(T), "x");
            var body = Expression.AndAlso(
                Expression.Invoke(leftExpr, param),
                Expression.Invoke(rightExpr, param));

            return Expression.Lambda<Func<T, bool>>(body, param);
        }
    }
}
