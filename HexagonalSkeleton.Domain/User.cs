using System.ComponentModel.DataAnnotations;

namespace HexagonalSkeleton.Domain
{
    public class User
    {
        public User() { }

        public User(string email)
        {
            Email = email;
        }

        public User(int userId, string fileName)
        {
            Id = userId;
            ProfileImageName = fileName;
        }

        public User(
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

        public int Id { get; set; }

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
