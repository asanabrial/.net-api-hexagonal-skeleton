using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Infrastructure.Persistence.Command.Entities;

namespace HexagonalSkeleton.Infrastructure.Mapping
{
    /// <summary>
    /// Hand-written mappers between the User domain aggregate and the
    /// command-side persistence entity (PostgreSQL).
    /// Replaces the previous AutoMapper-based CqrsMappingProfile.
    /// </summary>
    public static class UserCommandEntityMapper
    {
        /// <summary>
        /// Maps a User domain aggregate to a new command entity.
        /// Value objects are flattened to their primitive values.
        /// DateTime values are normalized to UTC for PostgreSQL.
        /// </summary>
        public static UserCommandEntity ToEntity(this User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var entity = new UserCommandEntity();
            user.MapInto(entity);
            return entity;
        }

        /// <summary>
        /// Copies the User domain state onto an existing tracked command entity.
        /// Mutates the provided instance in place so EF Core change tracking is preserved.
        /// Authentication fields (PasswordHash, PasswordSalt) are not overwritten here,
        /// matching the original AutoMapper profile which did not map them.
        /// </summary>
        public static void MapInto(this User user, UserCommandEntity target)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(target);

            target.Id = user.Id;
            target.Email = user.Email.Value;
            target.FirstName = user.FullName.FirstName;
            target.LastName = user.FullName.LastName;
            target.PhoneNumber = user.PhoneNumber.Value;
            target.Latitude = user.Location.Latitude;
            target.Longitude = user.Location.Longitude;
            target.AboutMe = user.AboutMe;
            target.Birthdate = DateTimeNormalizer.ToUtc(user.Birthdate);
            target.CreatedAt = DateTimeNormalizer.ToUtc(user.CreatedAt);
            target.UpdatedAt = DateTimeNormalizer.ToUtc(user.UpdatedAt);
            target.DeletedAt = DateTimeNormalizer.ToUtc(user.DeletedAt);
            target.LastLogin = DateTimeNormalizer.ToUtc(user.LastLogin);
            target.IsDeleted = user.IsDeleted;
        }

        /// <summary>
        /// Reconstitutes a User domain aggregate from a command entity.
        /// Value objects are rebuilt from the flat persistence fields.
        /// </summary>
        public static User ToDomain(this UserCommandEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return User.Reconstitute(
                id: entity.Id,
                email: entity.Email,
                firstName: entity.FirstName,
                lastName: entity.LastName,
                birthdate: entity.Birthdate ?? DateTime.UtcNow.AddYears(-20),
                phoneNumber: entity.PhoneNumber,
                latitude: entity.Latitude,
                longitude: entity.Longitude,
                aboutMe: entity.AboutMe,
                passwordSalt: entity.PasswordSalt,
                passwordHash: entity.PasswordHash,
                lastLogin: entity.LastLogin ?? DateTime.UtcNow,
                createdAt: entity.CreatedAt,
                updatedAt: entity.UpdatedAt,
                deletedAt: entity.DeletedAt,
                isDeleted: entity.IsDeleted);
        }
    }
}
