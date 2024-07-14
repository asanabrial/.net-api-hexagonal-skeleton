using HexagonalSkeleton.API.Features.User.Domain;
using MediatR;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    /// <summary>
    /// This class represents a command for logging in a user.
    /// </summary>
    public class UpdateUserCommand : IRequest<IResult>
    {
        public required int Id { get; set; }
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public required DateTime Birthdate { get; set; }
        public required string Password { get; set; }
        public required string PasswordSalt { get; set; }
        public required string PasswordHash { get; set; }
        public required DateTime LastLogin { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime UpdatedAt { get; set; }
        public required DateTime DeletedAt { get; set; }

        public required bool IsDeleted { get; set; }

        /// <summary>
        /// Method to convert the DTO to a domain entity
        /// </summary>
        /// <returns></returns>
        public UserEntity ToDomainEntity()
        {
            return new UserEntity
            {
                Id = Id,
                Email = Email,
                Name = Name,
                Surname = Surname,
                Birthdate = Birthdate,
                PasswordSalt = PasswordSalt,
                PasswordHash = PasswordHash,
                LastLogin = LastLogin,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                DeletedAt = DeletedAt,
                IsDeleted = IsDeleted
            };
        }
    }
}
