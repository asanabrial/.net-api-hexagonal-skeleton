using System.ComponentModel.DataAnnotations;
using AutoMapper;
using HexagonalSkeleton.Application.Features.UserProfile.Commands;

namespace HexagonalSkeleton.API.Models.Users
{
    /// <summary>
    /// Request model for updating user profile (partial update)
    /// </summary>
    [AutoMap(typeof(UpdateProfileUserCommand), ReverseMap = true)]
    public class UpdateProfileRequest
    {
        /// <summary>
        /// User's first name
        /// </summary>
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string? FirstName { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string? LastName { get; set; }

        /// <summary>
        /// User's phone number
        /// </summary>
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// User's birth date
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? Birthdate { get; set; }

        /// <summary>
        /// User's about me section
        /// </summary>
        [StringLength(500, ErrorMessage = "About me cannot exceed 500 characters")]
        public string? AboutMe { get; set; }
    }
}
