using System.ComponentModel.DataAnnotations;
using HexagonalSkeleton.CommonCore.Data.Entity;

namespace HexagonalSkeleton.API.Features.User.Domain
{
    public class UserEntity : Entity
    {
        public UserEntity() { }

        public UserEntity(string email/*, string password*/)
        {
            Email = email;
            //Password = password;
        }

        public UserEntity(string email, /*string password,*/ string passwordSalt, string passwordHash)
        {
            Email = email;
            //Password = password;
            PasswordSalt = passwordSalt;
            PasswordHash = passwordHash;
        }

        public UserEntity(string? email, string? name, string? surname, DateTime? birthdate) : this()
        {
            Email = email;
            Name = name;
            Surname = surname;
            Birthdate = birthdate;
        }

        public UserEntity(int id, string? email, string? name, string? surname, DateTime? birthdate, DateTime lastLogin, DateTime createdAt, DateTime updatedAt) : this()
        {
            Id = id;
            Email = email;
            Name = name;
            Surname = surname;
            Birthdate = birthdate;
            LastLogin = lastLogin;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? Surname { get; set; }

        [MaxLength(100)]
        public DateTime? Birthdate { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        //[MaxLength(100)]
        //public string? Password { get; set; }

        [MaxLength(250)]
        public string? PasswordSalt { get; set; }

        [MaxLength(250)]
        public string? PasswordHash { get; set; }
        public DateTime LastLogin { get; set; }
    }
}
