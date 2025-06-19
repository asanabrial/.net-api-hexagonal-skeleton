using System.ComponentModel.DataAnnotations;

namespace HexagonalSkeleton.API.Models.Users
{
    /// <summary>
    /// Request model for creating a new user
    /// </summary>
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
        public required string PhoneNumber { get; set; }

        /// <summary>
        /// User's birth date
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? Birthdate { get; set; }
    }
}
