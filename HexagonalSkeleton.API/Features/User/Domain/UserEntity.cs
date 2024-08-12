using System.ComponentModel.DataAnnotations;
using HexagonalSkeleton.CommonCore.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace HexagonalSkeleton.API.Features.User.Domain
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(PhoneNumber), IsUnique = true)]
    public class UserEntity : Entity
    {
        public UserEntity() { }

        public UserEntity(string email)
        {
            Email = email;
        }

        public UserEntity(int userId, IFormFile profileImage)
        {
            Id = userId;
            ProfileImageName = profileImage.FileName;
        }

        public UserEntity(
            string email,
            string passwordSalt,
            string passwordHash, 
            string name,
            string surname,
            DateTime birthdate, 
            string phoneNumber,
            double latitude,
            double longitude,
            string aboutMe)
        {
            Email = email;
            PasswordSalt = passwordSalt;
            PasswordHash = passwordHash;
            Name = name;
            Surname = surname;
            Birthdate = birthdate;
            PhoneNumber = phoneNumber;
            AboutMe = aboutMe;
            Latitude = latitude;
            Longitude = longitude;
        }


        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? Surname { get; set; }

        public DateTime? Birthdate { get; set; }

        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? ProfileImageName { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
        
        [MaxLength(500)]
        public string? AboutMe { get; set; }

        [MaxLength(250)]
        public string? PasswordSalt { get; set; }

        [MaxLength(250)]
        public string? PasswordHash { get; set; }

        public DateTime LastLogin { get; set; }
    }
}
