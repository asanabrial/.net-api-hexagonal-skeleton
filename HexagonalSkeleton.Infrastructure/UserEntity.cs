using System.ComponentModel.DataAnnotations;
using HexagonalSkeleton.CommonCore.Data.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HexagonalSkeleton.Infrastructure
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(PhoneNumber), IsUnique = true)]    public class UserEntity : Entity
    {
        // Entity Framework requires a parameterless constructor
        public UserEntity() { }

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
