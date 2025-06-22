using System.ComponentModel.DataAnnotations;
using AutoMapper;
using HexagonalSkeleton.Application.Command;

namespace HexagonalSkeleton.API.Models.Auth
{
    /// <summary>
    /// Request model for user authentication
    /// </summary>
    [AutoMap(typeof(LoginCommand), ReverseMap = true)]
    public class LoginRequest
    {
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
        /// Whether to remember the user session
        /// </summary>
        public bool RememberMe { get; set; } = false;
    }
}
