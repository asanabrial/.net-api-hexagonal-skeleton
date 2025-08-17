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
}
