using AutoMapper;
using HexagonalSkeleton.Domain.ValueObjects;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Infrastructure.Persistence.Query.Documents;
using HexagonalSkeleton.Infrastructure.Extensions;
using MongoDB.Driver.GeoJsonObjectModel;

namespace HexagonalSkeleton.Infrastructure.Mapping
{
    /// <summary>
    /// AutoMapper profile for Infrastructure layer mappings
    /// Handles the mapping between domain objects and entity objects,
    /// including Value Objects and complex domain mappings
    /// </summary>
    public class InfrastructureMappingProfile : Profile
    {
        public InfrastructureMappingProfile()
        {
            // DateTime converters - explicit UTC handling for PostgreSQL
            CreateMap<DateTime, DateTime>()
                .ConvertUsing(src => src.Kind == DateTimeKind.Utc ? src : 
                    src.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(src, DateTimeKind.Utc) : 
                    src.ToUniversalTime());
            
            CreateMap<DateTime?, DateTime?>()
                .ConvertUsing(src => !src.HasValue ? null : 
                    src.Value.Kind == DateTimeKind.Utc ? src.Value : 
                    src.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(src.Value, DateTimeKind.Utc) : 
                    src.Value.ToUniversalTime());

            // FullName Value Object mappings
            CreateMap<FullName, string>().ConvertUsing(vo => vo != null ? vo.GetFullName() : string.Empty);
            CreateMap<string, FullName>().ConvertUsing(value => CreateFullNameFromString(value));
            
            // FullNameDocument Value Object mappings
            CreateMap<FullNameDocument, FullName>().ConvertUsing(doc => doc != null ? 
                new FullName(doc.FirstName, doc.LastName) : new FullName("Unknown", "User"));
            CreateMap<FullName, FullNameDocument>().ConvertUsing(vo => vo != null ? new FullNameDocument
            {
                FirstName = vo.FirstName,
                LastName = vo.LastName,
                DisplayName = vo.GetFullName()
            } : new FullNameDocument());
            
            // Location Value Object mappings
            CreateMap<Location, LocationDocument>().ConvertUsing(vo => vo != null ? new LocationDocument 
            { 
                Latitude = vo.Latitude, 
                Longitude = vo.Longitude 
            } : new LocationDocument());
            CreateMap<LocationDocument, Location>().ConvertUsing(doc => doc != null ? 
                new Location(doc.Latitude, doc.Longitude) : new Location(0, 0));

            // Email Value Object mappings
            CreateMap<Email, string>().ConvertUsing(vo => vo != null ? vo.Value : default!);
            CreateMap<string, Email>().ConvertUsing(value => !string.IsNullOrEmpty(value) ? new Email(value) : default!);
            
            // PhoneNumber Value Object mappings
            CreateMap<PhoneNumber, string>().ConvertUsing(vo => vo != null ? vo.Value : default!);
            CreateMap<string, PhoneNumber>().ConvertUsing(value => !string.IsNullOrEmpty(value) ? new PhoneNumber(value) : default!);
            
            // UserId Value Object mappings
            CreateMap<UserId, Guid>().ConvertUsing(vo => vo != null ? vo.Value : Guid.Empty);
            CreateMap<Guid, UserId>().ConvertUsing(value => value != Guid.Empty ? new UserId(value) : default!);
            
            // MongoDB UserDocument mappings for read model (CQRS)

            // Domain -> Document (For writing to MongoDB)
            CreateMap<User, UserDocument>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FullName.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.FullName.LastName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber.Value))
                .ForMember(dest => dest.AboutMe, opt => opt.MapFrom(src => src.AboutMe))
                .ForMember(dest => dest.ProfileImageName, opt => opt.MapFrom(src => src.ProfileImageName))
                .ForMember(dest => dest.Birthdate, opt => opt.MapFrom(src => src.Birthdate))
                // Use extension method to create GeoJsonPoint
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => 
                    LocationExtensions.CreateGeoJsonPoint(src.Location.Latitude, src.Location.Longitude)))
                .ForMember(dest => dest.LastLogin, opt => opt.MapFrom(src => src.LastLogin))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
                // Set equivalent fields
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => !src.IsDeleted))
                // Denormalized fields
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName.GetFullName()))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.GetAge()))
                .ForMember(dest => dest.IsAdult, opt => opt.MapFrom(src => src.IsAdult()))
                // Additional fields
                .ForMember(dest => dest.SearchTerms, opt => opt.MapFrom(src => new List<string> { 
                    src.FullName.FirstName.ToLowerInvariant(),
                    src.FullName.LastName.ToLowerInvariant(),
                    src.Email.Value.ToLowerInvariant()
                }))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => new List<string>())) // Empty list for now
                .ForMember(dest => dest.SyncedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Version, opt => opt.MapFrom(src => 1));

            // Document -> Domain (For reading from MongoDB)
            CreateMap<UserDocument, User>()
                .ConstructUsing((src, ctx) => {
                    // Safely extract latitude and longitude with fallback values
                    double latitude = 0.0;
                    double longitude = 0.0;
                    
                    if (src.Location != null && src.Location.Coordinates != null)
                    {
                        latitude = src.Location.Coordinates.Latitude;
                        longitude = src.Location.Coordinates.Longitude;
                    }
                    
                    return User.Reconstitute(
                        id: src.Id,
                        email: src.Email,
                        firstName: src.FirstName,
                        lastName: src.LastName,
                        birthdate: src.Birthdate ?? DateTime.UtcNow.AddYears(-20),
                        phoneNumber: src.PhoneNumber,
                        latitude: latitude,
                        longitude: longitude,
                        aboutMe: src.AboutMe,
                        passwordSalt: "mongodb-read", // MongoDB doesn't store auth data
                        passwordHash: "mongodb-read", // MongoDB doesn't store auth data
                        lastLogin: src.LastLogin,
                        createdAt: src.CreatedAt,
                        updatedAt: src.UpdatedAt,
                        deletedAt: src.DeletedAt,
                        isDeleted: src.IsDeleted,
                        profileImageName: src.ProfileImageName
                    );
                })
                .ForAllMembers(opt => opt.Ignore()); // Ignore all other members since we use ConstructUsing

            // UserQueryDocument -> User (For reading from MongoDB query model)
            CreateMap<UserQueryDocument, User>()
                .ConstructUsing((src, ctx) => {
                    // Safely extract latitude and longitude with fallback values
                    double latitude = 0.0;
                    double longitude = 0.0;
                    
                    if (src.Location != null)
                    {
                        latitude = src.Location.Latitude;
                        longitude = src.Location.Longitude;
                    }
                    
                    return User.Reconstitute(
                        id: src.Id,
                        email: src.Email,
                        firstName: src.FullName.FirstName,
                        lastName: src.FullName.LastName,
                        birthdate: src.Birthdate ?? DateTime.UtcNow.AddYears(-20),
                        phoneNumber: src.PhoneNumber,
                        latitude: latitude,
                        longitude: longitude,
                        aboutMe: src.AboutMe,
                        passwordSalt: "query-read", // Query model doesn't store auth data
                        passwordHash: "query-read", // Query model doesn't store auth data
                        lastLogin: src.LastLogin ?? DateTime.UtcNow,
                        createdAt: src.CreatedAt,
                        updatedAt: src.UpdatedAt,
                        deletedAt: src.DeletedAt,
                        isDeleted: src.IsDeleted,
                        profileImageName: null // Query model may not have this field
                    );
                })
                .ForAllMembers(opt => opt.Ignore()); // Ignore all other members since we use ConstructUsing

            // User -> UserQueryDocument (For writing to MongoDB query model)
            CreateMap<User, UserQueryDocument>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => new FullNameDocument
                {
                    FirstName = src.FullName.FirstName,
                    LastName = src.FullName.LastName,
                    DisplayName = src.FullName.GetFullName()
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
                // Computed fields
                .ForMember(dest => dest.SearchTerms, opt => opt.MapFrom(src => new List<string> { 
                    src.FullName.FirstName.ToLowerInvariant(),
                    src.FullName.LastName.ToLowerInvariant(),
                    src.Email.Value.ToLowerInvariant()
                }))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.GetAge()))
                .ForMember(dest => dest.ProfileCompleteness, opt => opt.MapFrom(src => 1.0));
        }

        /// <summary>
        /// Helper method to create FullName from string
        /// </summary>
        private static FullName CreateFullNameFromString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new FullName("Unknown", "User");
            
            var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length switch
            {
                0 => new FullName("Unknown", "User"),
                1 => new FullName(parts[0], ""),
                _ => new FullName(parts[0], string.Join(" ", parts.Skip(1)))
            };
        }
    }
}
