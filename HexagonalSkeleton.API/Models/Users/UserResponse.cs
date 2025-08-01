using AutoMapper;
using HexagonalSkeleton.Application.Features.UserManagement.Dto;

namespace HexagonalSkeleton.API.Models.Users
{
    /// <summary>
    /// Response model for user operations
    /// Maps to multiple DTOs depending on the context
    /// </summary>
    [AutoMap(typeof(GetUserDto), ReverseMap = true)]
    [AutoMap(typeof(GetAllUsersDto), ReverseMap = true)]
    public class UserResponse
    {
        /// <summary>
        /// User identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// User's first name
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// User's last name
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// User's full name
        /// </summary>
        public string FullName => $"{FirstName} {LastName}".Trim();

        /// <summary>
        /// User email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's phone number
        /// </summary>
        public string? PhoneNumber { get; set; }        /// <summary>
        /// User's birth date
        /// </summary>
        public DateTime? Birthdate { get; set; }

        /// <summary>
        /// User's latitude coordinate
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// User's longitude coordinate
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// User's about me description
        /// </summary>
        public string? AboutMe { get; set; }

        /// <summary>
        /// User's profile image file name
        /// </summary>
        public string? ProfileImageName { get; set; }

        /// <summary>
        /// Last login timestamp
        /// </summary>
        public DateTime? LastLogin { get; set; }

        /// <summary>
        /// When the user was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the user was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
