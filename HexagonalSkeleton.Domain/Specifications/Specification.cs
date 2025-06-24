using System.Linq.Expressions;

namespace HexagonalSkeleton.Domain.Specifications
{
    /// <summary>
    /// SIMPLIFIED Specification pattern - Easy to understand and maintain
    /// Follows KISS principle while maintaining functionality
    /// </summary>
    public abstract class Specification<T> : ISpecification<T>
    {
        public abstract Expression<Func<T, bool>> ToExpression();

        /// <summary>
        /// Evaluates the specification against an entity in memory
        /// </summary>
        public bool IsSatisfiedBy(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var predicate = ToExpression().Compile();
            return predicate(entity);
        }

        // Operators for composing specifications
        public static Specification<T> operator &(Specification<T> left, Specification<T> right)
            => new AndSpecification<T>(left, right);

        public static Specification<T> operator |(Specification<T> left, Specification<T> right)
            => new OrSpecification<T>(left, right);

        public static Specification<T> operator !(Specification<T> specification)
            => new NotSpecification<T>(specification);

        // Fluent API methods
        public Specification<T> And(ISpecification<T> specification)
            => new AndSpecification<T>(this, specification);

        public Specification<T> Or(ISpecification<T> specification)
            => new OrSpecification<T>(this, specification);

        public Specification<T> Not()
            => new NotSpecification<T>(this);
    }

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

    /// <summary>
    /// Simple OR specification - Easy to understand
    /// </summary>
    internal class OrSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _left;
        private readonly ISpecification<T> _right;

        public OrSpecification(ISpecification<T> left, ISpecification<T> right)
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
            var body = Expression.OrElse(
                Expression.Invoke(leftExpr, param),
                Expression.Invoke(rightExpr, param));

            return Expression.Lambda<Func<T, bool>>(body, param);
        }
    }

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
