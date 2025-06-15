using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain;
using MediatR;

namespace HexagonalSkeleton.Application.Command
{
    public class UpdateUserCommand : IRequest<ResultDto>
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
        public User ToDomainEntity()
        {
            return new User
            {
                Id = Id,
                Email = Email,
                Name = Name,
                Surname = Surname,
                Birthdate = Birthdate,
                PasswordSalt = PasswordSalt,
                PasswordHash = PasswordHash,
                LastLogin = LastLogin,
            };
        }
    }
}
