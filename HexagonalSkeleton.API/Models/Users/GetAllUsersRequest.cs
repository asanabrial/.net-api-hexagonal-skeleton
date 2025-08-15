using System.ComponentModel.DataAnnotations;
using AutoMapper;
using HexagonalSkeleton.Application.Features.UserManagement.Queries;

namespace HexagonalSkeleton.API.Models.Users
{
    /// <summary>
    /// Request model for getting all users with advanced filtering using Specification pattern
    /// Supports multiple filter criteria that can be combined for powerful searches
    /// Follows Request-Response pattern with clear validation rules
    /// </summary>
    [AutoMap(typeof(GetAllUsersManagementQuery), ReverseMap = true)]
    public class GetAllUsersRequest
    {
        /// <summary>
        /// Page number (1-based, default: 1)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Number of items per page (1-100, default: 10)
        /// </summary>
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Search term for filtering users
        /// Searches across: first name, last name, email, and phone number using partial matching (contains)
        /// </summary>
        [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Optional sorting criteria
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Sort direction (asc/desc)
        /// </summary>
        public string SortDirection { get; set; } = "asc";

        // Business logic filters

        /// <summary>
        /// Minimum age filter (0-120)
        /// </summary>
        [Range(0, 120, ErrorMessage = "Minimum age must be between 0 and 120")]
        public int? MinAge { get; set; }

        /// <summary>
        /// Maximum age filter (0-120)
        /// </summary>
        [Range(0, 120, ErrorMessage = "Maximum age must be between 0 and 120")]
        public int? MaxAge { get; set; }

        /// <summary>
        /// Filter only adult users (18+)
        /// </summary>
        public bool? OnlyAdults { get; set; }

        /// <summary>
        /// Filter only active users (exclude deleted users)
        /// Default to true for security and business rules
        /// </summary>
        public bool? OnlyActive { get; set; } = true;

        /// <summary>
        /// Filter users with complete profiles only
        /// </summary>
        public bool? OnlyCompleteProfiles { get; set; }

        /// <summary>
        /// Center latitude for location-based filtering (-90 to 90)
        /// </summary>
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double? Latitude { get; set; }

        /// <summary>
        /// Center longitude for location-based filtering (-180 to 180)
        /// </summary>
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double? Longitude { get; set; }

        /// <summary>
        /// Radius in kilometers for location-based filtering (1-10000)
        /// </summary>
        [Range(1, 10000, ErrorMessage = "Radius must be between 1 and 10000 kilometers")]
        public double? RadiusInKm { get; set; }
    }
}
