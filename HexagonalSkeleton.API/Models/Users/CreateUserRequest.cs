using System.ComponentModel.DataAnnotations;
using AutoMapper;
using HexagonalSkeleton.Application.Command;

namespace HexagonalSkeleton.API.Models.Users
{
    /// <summary>
    /// Request model for creating a new user
    /// </summary>
    [AutoMap(typeof(RegisterUserCommand), ReverseMap = true)]
    public class CreateUserRequest
    {
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
        /// User password
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public required string Password { get; set; }

        /// <summary>
        /// User's phone number
        /// </summary>
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public required string PhoneNumber { get; set; }        /// <summary>
        /// User's birth date
        /// </summary>
        [Required(ErrorMessage = "Birth date is required")]
        [DataType(DataType.Date)]
        public DateTime Birthdate { get; set; }

        /// <summary>
        /// Password confirmation
        /// </summary>
        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
        public required string PasswordConfirmation { get; set; }

        /// <summary>
        /// User's latitude coordinate
        /// </summary>
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double Latitude { get; set; }

        /// <summary>
        /// User's longitude coordinate
        /// </summary>
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double Longitude { get; set; }

        /// <summary>
        /// User's personal description
        /// </summary>
        [StringLength(500, ErrorMessage = "About me cannot exceed 500 characters")]
        public string AboutMe { get; set; } = string.Empty;
    }
}
