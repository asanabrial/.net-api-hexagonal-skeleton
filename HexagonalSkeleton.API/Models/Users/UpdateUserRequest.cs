using System.ComponentModel.DataAnnotations;
using AutoMapper;
using HexagonalSkeleton.Application.Features.UserManagement.Commands;

namespace HexagonalSkeleton.API.Models.Users
{
    /// <summary>
    /// Request model for updating user information
    /// </summary>
    [AutoMap(typeof(UpdateUserCommand), ReverseMap = true)]
    public class UpdateUserRequest
    {
        /// <summary>
        /// User identifier
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public Guid Id { get; set; }

        /// <summary>
        /// User's first name
        /// </summary>
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public required string FirstName { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public required string LastName { get; set; }

        /// <summary>
        /// User's phone number
        /// </summary>
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public required string PhoneNumber { get; set; }

        /// <summary>
        /// User's birth date
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? Birthdate { get; set; }

        /// <summary>
        /// User's latitude location
        /// </summary>
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double Latitude { get; set; }

        /// <summary>
        /// User's longitude location
        /// </summary>
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double Longitude { get; set; }

        /// <summary>
        /// User's about me description
        /// </summary>
        [StringLength(500, ErrorMessage = "About me cannot exceed 500 characters")]
        public string AboutMe { get; set; } = string.Empty;
    }
}
