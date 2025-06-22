using AutoMapper;
using HexagonalSkeleton.Domain.Specifications;
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
        protected abstract DbSet<TEntity> GetDbSet();

        /// <summary>
        /// Applies specification to entity query
        /// </summary>
        protected virtual IQueryable<TEntity> ApplySpecification(IQueryable<TEntity> query, Specification<TDomain>? specification)
        {
            if (specification == null)
                return query;

            // Convert domain specification to entity expression
            var entityExpression = ConvertDomainSpecificationToEntity(specification);
            return query.Where(entityExpression);
        }

        /// <summary>
        /// Converts domain specification to entity expression
        /// Must be implemented by derived classes to handle domain-to-entity mapping
        /// </summary>
        protected abstract Expression<Func<TEntity, bool>> ConvertDomainSpecificationToEntity(Specification<TDomain> specification);

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
    }
}
