using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.Common;
using HexagonalSkeleton.Domain.Ports.Dtos;
using HexagonalSkeleton.Infrastructure.Persistence.Command.Entities;
using HexagonalSkeleton.Infrastructure.Persistence.Query.Documents;

namespace HexagonalSkeleton.Infrastructure.Mapping
{
    /// <summary>
    /// Hand-written mappers for the query-side document (MongoDB) and the read DTO.
    /// Replaces the previous AutoMapper-based InfrastructureMappingProfile and the
    /// query-document parts of CqrsMappingProfile.
    /// </summary>
    public static class UserQueryDocumentMapper
    {
        /// <summary>
        /// Reconstitutes a User domain aggregate from a query document.
        /// The query model does not store authentication data, so placeholder
        /// credentials are used (matching the original AutoMapper behaviour).
        /// Location is read defensively with a fallback to (0, 0) when absent.
        /// </summary>
        public static User ToDomain(this UserQueryDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);

            var latitude = document.Location?.Latitude ?? 0.0;
            var longitude = document.Location?.Longitude ?? 0.0;

            return User.Reconstitute(
                id: document.Id,
                email: document.Email,
                firstName: document.FullName.FirstName,
                lastName: document.FullName.LastName,
                birthdate: document.Birthdate ?? DateTime.UtcNow.AddYears(-20),
                phoneNumber: document.PhoneNumber,
                latitude: latitude,
                longitude: longitude,
                aboutMe: document.AboutMe,
                passwordSalt: "query-read",
                passwordHash: "query-read",
                lastLogin: document.LastLogin ?? DateTime.UtcNow,
                createdAt: document.CreatedAt,
                updatedAt: document.UpdatedAt,
                deletedAt: document.DeletedAt,
                isDeleted: document.IsDeleted);
        }

        /// <summary>
        /// Maps a User domain aggregate to a query document for the read model.
        /// Builds embedded documents and computed fields. DateTime values are
        /// normalized to UTC for consistent storage.
        /// </summary>
        public static UserQueryDocument ToDocument(this User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var firstName = user.FullName.FirstName;
            var lastName = user.FullName.LastName;
            var email = user.Email.Value;
            var phone = user.PhoneNumber.Value;

            return new UserQueryDocument
            {
                Id = user.Id,
                Email = email,
                FullName = new FullNameDocument
                {
                    FirstName = firstName,
                    LastName = lastName,
                    DisplayName = user.FullName.GetFullName()
                },
                PhoneNumber = phone,
                Birthdate = DateTimeNormalizer.ToUtc(user.Birthdate),
                Location = new LocationDocument
                {
                    Latitude = user.Location.Latitude,
                    Longitude = user.Location.Longitude
                },
                AboutMe = user.AboutMe,
                CreatedAt = DateTimeNormalizer.ToUtc(user.CreatedAt),
                UpdatedAt = DateTimeNormalizer.ToUtc(user.UpdatedAt),
                LastLogin = DateTimeNormalizer.ToUtc(user.LastLogin),
                IsDeleted = user.IsDeleted,
                DeletedAt = DateTimeNormalizer.ToUtc(user.DeletedAt),
                SearchTerms = BuildSearchTerms(email, firstName, lastName, phone),
                Age = CalculateAge(user.Birthdate),
                ProfileCompleteness = CalculateProfileCompleteness(user)
            };
        }

        /// <summary>
        /// Maps a command entity to a query document for store synchronization.
        /// Mirrors <see cref="ToDocument(User)"/> but reads from the flat entity fields.
        /// </summary>
        public static UserQueryDocument ToDocument(this UserCommandEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new UserQueryDocument
            {
                Id = entity.Id,
                Email = entity.Email,
                FullName = new FullNameDocument
                {
                    FirstName = entity.FirstName,
                    LastName = entity.LastName,
                    DisplayName = $"{entity.FirstName} {entity.LastName}"
                },
                PhoneNumber = entity.PhoneNumber,
                Birthdate = DateTimeNormalizer.ToUtc(entity.Birthdate),
                Location = new LocationDocument
                {
                    Latitude = entity.Latitude,
                    Longitude = entity.Longitude
                },
                AboutMe = entity.AboutMe,
                CreatedAt = DateTimeNormalizer.ToUtc(entity.CreatedAt),
                UpdatedAt = DateTimeNormalizer.ToUtc(entity.UpdatedAt),
                LastLogin = DateTimeNormalizer.ToUtc(entity.LastLogin),
                IsDeleted = entity.IsDeleted,
                DeletedAt = DateTimeNormalizer.ToUtc(entity.DeletedAt),
                SearchTerms = BuildSearchTerms(entity.Email, entity.FirstName, entity.LastName, entity.PhoneNumber),
                Age = CalculateAge(entity.Birthdate),
                ProfileCompleteness = CalculateProfileCompletenessFromEntity(entity)
            };
        }

        /// <summary>
        /// Maps a query document to the read DTO, flattening embedded documents.
        /// </summary>
        public static UserQueryDto ToQueryDto(this UserQueryDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);

            return new UserQueryDto
            {
                Id = document.Id,
                Email = document.Email,
                FirstName = document.FullName.FirstName,
                LastName = document.FullName.LastName,
                DisplayName = document.FullName.DisplayName,
                PhoneNumber = document.PhoneNumber,
                Birthdate = document.Birthdate,
                Latitude = document.Location.Latitude,
                Longitude = document.Location.Longitude,
                AboutMe = document.AboutMe,
                CreatedAt = document.CreatedAt,
                LastLogin = document.LastLogin,
                Age = document.Age,
                ProfileCompleteness = document.ProfileCompleteness,
                SearchTerms = document.SearchTerms
            };
        }

        private static List<string> BuildSearchTerms(string email, string firstName, string lastName, string phone) =>
            new()
            {
                email.ToLowerInvariant(),
                firstName.ToLowerInvariant(),
                lastName.ToLowerInvariant(),
                $"{firstName} {lastName}".ToLowerInvariant(),
                phone
            };

        private static int? CalculateAge(DateTime? birthdate)
        {
            if (!birthdate.HasValue) return null;

            return AgeCalculator.CalculateAge(birthdate.Value, DateTime.UtcNow.Date);
        }

        private static double CalculateProfileCompleteness(User user)
        {
            var fields = new[]
            {
                !string.IsNullOrWhiteSpace(user.Email?.Value),
                !string.IsNullOrWhiteSpace(user.FullName?.FirstName),
                !string.IsNullOrWhiteSpace(user.FullName?.LastName),
                !string.IsNullOrWhiteSpace(user.PhoneNumber?.Value),
                user.Birthdate.HasValue,
                user.Location?.Latitude != 0 && user.Location?.Longitude != 0,
                !string.IsNullOrWhiteSpace(user.AboutMe)
            };

            var completedFields = fields.Count(f => f);
            return (double)completedFields / fields.Length * 100;
        }

        private static double CalculateProfileCompletenessFromEntity(UserCommandEntity entity)
        {
            var fields = new[]
            {
                !string.IsNullOrWhiteSpace(entity.Email),
                !string.IsNullOrWhiteSpace(entity.FirstName),
                !string.IsNullOrWhiteSpace(entity.LastName),
                !string.IsNullOrWhiteSpace(entity.PhoneNumber),
                entity.Birthdate.HasValue,
                entity.Latitude != 0 && entity.Longitude != 0,
                !string.IsNullOrWhiteSpace(entity.AboutMe)
            };

            var completedFields = fields.Count(f => f);
            return (double)completedFields / fields.Length * 100;
        }
    }
}
