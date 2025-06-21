using System.ComponentModel.DataAnnotations;
using AutoMapper;
using HexagonalSkeleton.Application.Command;

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
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be greater than 0")]
        public int Id { get; set; }

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
        /// User email address
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public required string Email { get; set; }

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
    }
}
