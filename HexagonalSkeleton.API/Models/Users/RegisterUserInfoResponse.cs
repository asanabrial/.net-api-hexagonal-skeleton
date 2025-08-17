using AutoMapper;
using HexagonalSkeleton.Application.Features.UserRegistration.Dto;

namespace HexagonalSkeleton.API.Models.Users
{
    /// <summary>
    /// Basic user information included in registration response
    /// </summary>
    [AutoMap(typeof(RegisterUserInfoDto), ReverseMap = true)]
    public class RegisterUserInfoResponse
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
