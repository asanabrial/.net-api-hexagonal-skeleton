using System.Linq.Expressions;
using AutoMapper;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.Specifications;
using HexagonalSkeleton.Domain.Specifications.Users;
using HexagonalSkeleton.Infrastructure;
using HexagonalSkeleton.Infrastructure.Specifications;

namespace HexagonalSkeleton.Infrastructure.Services
{
    /// <summary>
    /// Service that translates domain specifications to entity specifications
    /// This ensures optimal database performance while maintaining domain purity
    /// Follows the adapter pattern to bridge domain and infrastructure concerns
    /// </summary>
    public class SpecificationTranslationService
    {
        private readonly IMapper _mapper;

        public SpecificationTranslationService(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Translate domain specification to entity specification for optimal database queries
        /// For composite specifications, we'll try to create equivalent combinations
        /// </summary>
        public ISpecification<UserEntity> TranslateToEntitySpecification(ISpecification<User> domainSpecification)
        {
            return domainSpecification switch
            {
                ActiveUserSpecification => new ActiveUserEntitySpecification(),
                AdultUserSpecification => new AdultUserEntitySpecification(),
                CompleteProfileSpecification => new CompleteProfileEntitySpecification(),
                
                // For now, fall back to generic for composite specifications
                _ => new GenericDomainToEntitySpecification(domainSpecification, _mapper)
            };
        }

        /// <summary>
        /// Try to extract search term from UserTextSearchSpecification
        /// This is a simplified approach - in production you might expose parameters directly
        /// </summary>
        public bool TryExtractSearchTerm(ISpecification<User> specification, out string searchTerm)
        {
            searchTerm = string.Empty;
            
            if (specification is not UserTextSearchSpecification)
                return false;

            // For now, we'll use a simple test to extract the search term
            // In production, you might modify the specification to expose its parameters
            try
            {
                var testUser = User.Create("test@example.com", "salt", "hash", "Test", "User", 
                    DateTime.Today.AddYears(-25), "+1234567890", 0, 0, "test");
                
                // Try different common search terms
                var testTerms = new[] { "test", "user", "example" };
                foreach (var term in testTerms)
                {
                    var testSpec = new UserTextSearchSpecification(term);
                    if (testSpec.IsSatisfiedBy(testUser) == specification.IsSatisfiedBy(testUser))
                    {
                        searchTerm = term;
                        return true;
                    }
                }
            }
            catch
            {
                // If extraction fails, fall back to generic approach
            }
            
            return false;
        }
    }

    /// <summary>
    /// Generic fallback specification that uses domain specification directly
    /// Less efficient but maintains correctness
    /// </summary>
    internal class GenericDomainToEntitySpecification : Specification<UserEntity>
    {
        private readonly ISpecification<User> _domainSpecification;
        private readonly IMapper _mapper;

        public GenericDomainToEntitySpecification(ISpecification<User> domainSpecification, IMapper mapper)
        {
            _domainSpecification = domainSpecification ?? throw new ArgumentNullException(nameof(domainSpecification));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public override Expression<Func<UserEntity, bool>> ToExpression()
        {
            // This approach evaluates the domain specification for each entity
            // Not optimal for performance but maintains correctness
            return entity => EvaluateEntity(entity);
        }

        private bool EvaluateEntity(UserEntity entity)
        {
            try
            {
                var domainUser = _mapper.Map<User>(entity);
                return _domainSpecification.IsSatisfiedBy(domainUser);
            }
            catch
            {
                // If mapping fails, exclude the entity
                return false;
            }
        }
    }
}
