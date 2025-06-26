using AutoMapper;
using HexagonalSkeleton.Domain.Specifications;
using HexagonalSkeleton.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HexagonalSkeleton.Infrastructure.Adapters.Base
{
    /// <summary>
    /// Base repository adapter implementing common patterns
    /// Follows DRY principle and provides shared functionality for all repository adapters
    /// </summary>
    public abstract class BaseRepositoryAdapter<TEntity, TDomain>
        where TEntity : class
        where TDomain : class
    {
        protected readonly AppDbContext _dbContext;
        protected readonly IMapper _mapper;

        protected BaseRepositoryAdapter(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Gets the DbSet for the entity type
        /// </summary>
        protected abstract DbSet<TEntity> GetDbSet();        /// <summary>
        /// Applies specification to entity query (ISpecification version)
        /// For composite specifications that can't be translated to SQL, returns the original query
        /// and the specification will be applied client-side later
        /// </summary>
        protected virtual IQueryable<TEntity> ApplySpecification(IQueryable<TEntity> query, ISpecification<TDomain>? specification)
        {
            if (specification == null)
                return query;

            // Convert to concrete specification if needed
            if (specification is Specification<TDomain> concreteSpec)
            {
                return ApplySpecification(query, concreteSpec);
            }

            // For ISpecification that is not a concrete Specification, try to convert the expression
            try
            {
                var domainExpression = specification.ToExpression();
                var entityExpression = ConvertDomainExpressionToEntity(domainExpression);
                return query.Where(entityExpression);
            }
            catch (NotSupportedException)
            {
                // Cannot translate to SQL, will need client-side evaluation
                // Return original query - client-side evaluation will be handled by the caller
                return query;
            }
        }        /// <summary>
        /// Applies specification to entity query (concrete Specification version)
        /// For composite specifications that can't be translated to SQL, throws NotSupportedException
        /// which should be caught and handled with client-side evaluation
        /// </summary>
        protected virtual IQueryable<TEntity> ApplySpecification(IQueryable<TEntity> query, Specification<TDomain>? specification)
        {
            if (specification == null)
                return query;

            try
            {
                // Convert domain specification to entity expression
                var entityExpression = ConvertDomainSpecificationToEntity(specification);
                return query.Where(entityExpression);
            }
            catch (NotSupportedException)
            {
                // Cannot translate to SQL, will need client-side evaluation
                // Return original query - client-side evaluation will be handled by the caller
                return query;
            }
        }/// <summary>
        /// Converts domain specification to entity expression
        /// Must be implemented by derived classes to handle domain-to-entity mapping
        /// </summary>
        protected abstract Expression<Func<TEntity, bool>> ConvertDomainSpecificationToEntity(Specification<TDomain> specification);

        /// <summary>
        /// Converts domain expression to entity expression
        /// Must be implemented by derived classes to handle domain-to-entity mapping
        /// </summary>
        protected abstract Expression<Func<TEntity, bool>> ConvertDomainExpressionToEntity(Expression<Func<TDomain, bool>> domainExpression);

        /// <summary>
        /// Maps entity to domain object with null handling
        /// </summary>
        protected virtual TDomain? MapToDomain(TEntity? entity)
        {
            return entity == null ? null : _mapper.Map<TDomain>(entity);
        }

        /// <summary>
        /// Maps entity collection to domain collection
        /// </summary>
        protected virtual List<TDomain> MapToDomain(IEnumerable<TEntity> entities)
        {
            return _mapper.Map<List<TDomain>>(entities);
        }

        /// <summary>
        /// Creates base query with no tracking for read operations
        /// </summary>
        protected virtual IQueryable<TEntity> CreateBaseQuery()
        {
            return GetDbSet().AsNoTracking();
        }

        /// <summary>
        /// Validates that an ID is positive
        /// </summary>
        protected static void ValidateId(int id, string parameterName = "id")
        {
            if (id <= 0)
                throw new ArgumentException("ID must be greater than 0", parameterName);
        }

        /// <summary>
        /// Validates that a string parameter is not null or empty
        /// </summary>
        protected static void ValidateStringParameter(string? value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{parameterName} cannot be null or empty", parameterName);
        }

        /// <summary>
        /// Checks if a specification requires client-side evaluation
        /// </summary>
        protected virtual bool RequiresClientSideEvaluation(ISpecification<TDomain>? specification)
        {
            if (specification == null)
                return false;

            // Check if it's a concrete specification that can be translated
            if (specification is Specification<TDomain> concreteSpec)
            {
                try
                {
                    ConvertDomainSpecificationToEntity(concreteSpec);
                    return false; // Can be translated
                }
                catch (NotSupportedException)
                {
                    return true; // Requires client-side evaluation
                }
            }

            // For ISpecification that is not concrete, try to convert the expression
            try
            {
                var domainExpression = specification.ToExpression();
                ConvertDomainExpressionToEntity(domainExpression);
                return false; // Can be translated
            }
            catch (NotSupportedException)
            {
                return true; // Requires client-side evaluation
            }
        }

        /// <summary>
        /// Applies specification client-side to a collection of domain objects
        /// </summary>
        protected virtual IEnumerable<TDomain> ApplySpecificationClientSide(IEnumerable<TDomain> items, ISpecification<TDomain> specification)
        {
            var compiledExpression = specification.ToExpression().Compile();
            return items.Where(compiledExpression);
        }
    }
}
