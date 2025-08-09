using AutoMapper;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain.ValueObjects;
using HexagonalSkeleton.Infrastructure.Persistence.Command.Entities;
using HexagonalSkeleton.Infrastructure.Persistence.Query.Documents;

namespace HexagonalSkeleton.Infrastructure.Mapping
{
    /// <summary>
    /// AutoMapper profile for CQRS mapping
    /// Handles conversion between domain entities, command entities, and query documents
    /// Follows separation of concerns principle
    /// </summary>
    public class CqrsMappingProfile : Profile
    {
        public CqrsMappingProfile()
        {
            // Domain to Command Entity mapping (for writes)
            CreateMap<User, UserCommandEntity>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FullName.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.FullName.LastName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber.Value))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Location.Latitude))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Location.Longitude))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
                .ForMember(dest => dest.LastLogin, opt => opt.MapFrom(src => src.LastLogin))
                .ReverseMap()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => new Email(src.Email)))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => new FullName(src.FirstName, src.LastName)))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => new PhoneNumber(src.PhoneNumber)))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => new Location(src.Latitude, src.Longitude)));

            // Domain to Query Document mapping (for reads)
            CreateMap<User, UserQueryDocument>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => new FullNameDocument
                {
                    FirstName = src.FullName.FirstName,
                    LastName = src.FullName.LastName,
                    DisplayName = $"{src.FullName.FirstName} {src.FullName.LastName}"
                }))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber.Value))
                .ForMember(dest => dest.Birthdate, opt => opt.MapFrom(src => src.Birthdate))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => new LocationDocument
                {
                    Latitude = src.Location.Latitude,
                    Longitude = src.Location.Longitude
                }))
                .ForMember(dest => dest.AboutMe, opt => opt.MapFrom(src => src.AboutMe))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.LastLogin, opt => opt.MapFrom(src => src.LastLogin))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
                .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt))
                .ForMember(dest => dest.SearchTerms, opt => opt.MapFrom(src => new List<string>
                {
                    src.Email.Value.ToLowerInvariant(),
                    src.FullName.FirstName.ToLowerInvariant(),
                    src.FullName.LastName.ToLowerInvariant(),
                    $"{src.FullName.FirstName} {src.FullName.LastName}".ToLowerInvariant(),
                    src.PhoneNumber.Value
                }))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => CalculateAge(src.Birthdate)))
                .ForMember(dest => dest.ProfileCompleteness, opt => opt.MapFrom(src => CalculateProfileCompleteness(src)));

            // Query Document to Query DTO mapping
            CreateMap<UserQueryDocument, UserQueryDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FullName.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.FullName.LastName))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.FullName.DisplayName))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Location.Latitude))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Location.Longitude));

            // Command Entity to Query Document mapping (for synchronization)
            CreateMap<UserCommandEntity, UserQueryDocument>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => new FullNameDocument
                {
                    FirstName = src.FirstName,
                    LastName = src.LastName,
                    DisplayName = $"{src.FirstName} {src.LastName}"
                }))
                .ForMember(dest => dest.Birthdate, opt => opt.MapFrom(src => src.Birthdate))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => new LocationDocument
                {
                    Latitude = src.Latitude,
                    Longitude = src.Longitude
                }))
                .ForMember(dest => dest.AboutMe, opt => opt.MapFrom(src => src.AboutMe))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.LastLogin, opt => opt.MapFrom(src => src.LastLogin))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
                .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt))
                .ForMember(dest => dest.SearchTerms, opt => opt.MapFrom(src => new List<string>
                {
                    src.Email.ToLowerInvariant(),
                    src.FirstName.ToLowerInvariant(),
                    src.LastName.ToLowerInvariant(),
                    $"{src.FirstName} {src.LastName}".ToLowerInvariant(),
                    src.PhoneNumber
                }))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => CalculateAge(src.Birthdate)))
                .ForMember(dest => dest.ProfileCompleteness, opt => opt.MapFrom(src => CalculateProfileCompletenessFromEntity(src)));
        }

        /// <summary>
        /// Calculate age from birthdate
        /// </summary>
        private static int? CalculateAge(DateTime? birthdate)
        {
            if (!birthdate.HasValue) return null;

            return HexagonalSkeleton.Domain.Common.AgeCalculator.CalculateAge(birthdate.Value, DateTime.UtcNow.Date);
        }

        /// <summary>
        /// Calculate profile completeness for domain entity
        /// </summary>
        private static double CalculateProfileCompleteness(User user)
        {
            var fields = new[]
            {
                !string.IsNullOrWhiteSpace(user.Email?.Value),
                !string.IsNullOrWhiteSpace(user.FullName?.FirstName),
                !string.IsNullOrWhiteSpace(user.FullName?.LastName),
                !string.IsNullOrWhiteSpace(user.PhoneNumber?.Value),
                user.Birthdate.HasValue,
                user.Location?.Latitude != 0 && user.Location?.Longitude != 0,
                !string.IsNullOrWhiteSpace(user.AboutMe)
            };

            var completedFields = fields.Count(f => f);
            return (double)completedFields / fields.Length * 100;
        }

        /// <summary>
        /// Calculate profile completeness for command entity
        /// </summary>
        private static double CalculateProfileCompletenessFromEntity(UserCommandEntity entity)
        {
            var fields = new[]
            {
                !string.IsNullOrWhiteSpace(entity.Email),
                !string.IsNullOrWhiteSpace(entity.FirstName),
                !string.IsNullOrWhiteSpace(entity.LastName),
                !string.IsNullOrWhiteSpace(entity.PhoneNumber),
                entity.Birthdate.HasValue,
                entity.Latitude != 0 && entity.Longitude != 0,
                !string.IsNullOrWhiteSpace(entity.AboutMe)
            };

            var completedFields = fields.Count(f => f);
            return (double)completedFields / fields.Length * 100;
        }
    }
}
