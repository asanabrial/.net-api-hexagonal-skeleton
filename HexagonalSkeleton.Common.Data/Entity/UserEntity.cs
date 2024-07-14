using System.ComponentModel.DataAnnotations;

namespace HexagonalSkeleton.Common.Data.Entity
{
    public class UserEntity : Entity
    {
        public UserEntity() { }

        public UserEntity(string email, string password)
        {
            Email = email;
            Password = password;
        }

        public UserEntity(string email, string password, string passwordSalt, string passwordHash)
        {
            Email = email;
            Password = password;
            PasswordSalt = passwordSalt;
            PasswordHash = passwordHash;
        }

        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? Surname { get; set; }

        [MaxLength(100)]
        public DateTime? Birthdate { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string? Password { get; set; }

        [MaxLength(250)]
        public string? PasswordSalt { get; set; }

        [MaxLength(250)]
        public string? PasswordHash { get; set; }
        public DateTime LastLogin { get; set; }
    }
}
