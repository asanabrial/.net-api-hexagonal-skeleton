using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.Specifications;
using HexagonalSkeleton.Domain.Specifications.Users;
using HexagonalSkeleton.Infrastructure.Persistence.Query.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HexagonalSkeleton.Infrastructure.Services
{
    public class MongoFilterBuilder : IMongoFilterBuilder
    {
        private static readonly FilterDefinitionBuilder<UserQueryDocument> Filter =
            Builders<UserQueryDocument>.Filter;

        /// <summary>
        /// Converts a domain specification into an equivalent MongoDB filter for the read model.
        /// </summary>
        /// <remarks>
        /// The domain expressions are written against the <c>User</c> aggregate shape
        /// (e.g. <c>user.Email.Value</c>), which does not match the denormalized
        /// <see cref="UserQueryDocument"/> shape, so a generic expression-tree translation is not
        /// possible. Instead, the known public leaf specifications are translated explicitly.
        ///
        /// The boolean composites (<c>And</c>/<c>Or</c>/<c>Not</c>) are <c>internal</c> to the domain
        /// assembly and expose no way to decompose their operands from here, so a composed
        /// specification cannot be translated. Rather than silently returning an empty (match-all)
        /// filter, unsupported specifications throw <see cref="NotSupportedException"/> so the gap is
        /// loud. The only exceptions are <see cref="AllUsersSpecification"/> and
        /// <see cref="ActiveUserSpecification"/>, which map cleanly to read-model filters.
        /// </remarks>
        public FilterDefinition<UserQueryDocument> ConvertSpecificationToMongoFilter(ISpecification<User> specification)
        {
            ArgumentNullException.ThrowIfNull(specification);

            return specification switch
            {
                AllUsersSpecification => Filter.Empty,

                ActiveUserSpecification => Filter.Eq(u => u.IsDeleted, false),

                AdultUserSpecification => Filter.Gte(u => u.Age, 18),

                UserAgeRangeSpecification ageRange => BuildAgeRangeFilter(ageRange),

                UserEmailSpecification email => Filter.Eq(
                    u => u.Email, GetPrivateField<string>(email, "_email")),

                UserPhoneNumberSpecification phone => Filter.Eq(
                    u => u.PhoneNumber, GetPrivateField<string>(phone, "_phoneNumber")),

                UserTextSearchSpecification text => BuildTextSearchFilter(
                    GetPrivateField<string>(text, "_searchTerm")),

                CompleteProfileSpecification => BuildCompleteProfileFilter(),

                _ => throw new NotSupportedException(
                    $"MongoFilterBuilder cannot translate specification of type " +
                    $"'{specification.GetType().Name}'. Composed (And/Or/Not) and custom specifications " +
                    $"are not supported by the read-model translator.")
            };
        }

        private static FilterDefinition<UserQueryDocument> BuildAgeRangeFilter(UserAgeRangeSpecification spec)
        {
            var minAge = GetPrivateField<int>(spec, "_minAge");
            var maxAge = GetPrivateField<int>(spec, "_maxAge");

            return Filter.And(
                Filter.Gte(u => u.Age, minAge),
                Filter.Lte(u => u.Age, maxAge));
        }

        private static FilterDefinition<UserQueryDocument> BuildTextSearchFilter(string searchTerm)
        {
            var regex = new BsonRegularExpression(searchTerm, "i");

            return Filter.Or(
                Filter.Regex(u => u.FullName.FirstName, regex),
                Filter.Regex(u => u.FullName.LastName, regex),
                Filter.Regex(u => u.Email, regex),
                Filter.Regex(u => u.PhoneNumber, regex));
        }

        private static FilterDefinition<UserQueryDocument> BuildCompleteProfileFilter()
        {
            return Filter.And(
                Filter.Ne(u => u.FullName.FirstName, string.Empty),
                Filter.Ne(u => u.FullName.LastName, string.Empty),
                Filter.Ne(u => u.Email, string.Empty),
                Filter.Ne(u => u.PhoneNumber, string.Empty),
                Filter.Ne(u => u.Birthdate, null),
                Filter.Ne(u => u.AboutMe, string.Empty));
        }

        /// <summary>
        /// Reads a private field from a specification instance. The leaf specifications keep their
        /// criteria in private fields with no public accessors, so reflection is the only way to
        /// recover them for translation. This is constrained to the small, known set of specifications
        /// handled above.
        /// </summary>
        private static T GetPrivateField<T>(object instance, string fieldName)
        {
            var field = instance.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?? throw new InvalidOperationException(
                    $"Expected field '{fieldName}' on '{instance.GetType().Name}' was not found.");

            return (T)field.GetValue(instance)!;
        }
    }
}
