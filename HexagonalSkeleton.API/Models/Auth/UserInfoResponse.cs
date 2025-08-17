using AutoMapper;
using HexagonalSkeleton.Application.Features.UserAuthentication.Dto;

namespace HexagonalSkeleton.API.Models.Auth
{
    /// <summary>
    /// Basic user information included in login response
    /// </summary>
    [AutoMap(typeof(AuthenticatedUserDto), ReverseMap = true)]
    public class UserInfoResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? Birthdate { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? AboutMe { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
