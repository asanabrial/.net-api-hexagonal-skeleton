using System.Linq.Expressions;

namespace HexagonalSkeleton.Domain.Specifications
{
    /// <summary>
    /// Contract for specification pattern following SOLID principles
    /// Enables dependency inversion and facilitates unit testing
    /// </summary>
    public interface ISpecification<T>
    {
        /// <summary>
        /// Converts the specification to an expression tree for query translation
        /// </summary>
        Expression<Func<T, bool>> ToExpression();

        /// <summary>
        /// Evaluates the specification against an entity in memory
        /// </summary>
        bool IsSatisfiedBy(T entity);
    }
}
