using Xunit;
using AutoMapper;
using HexagonalSkeleton.Infrastructure.Mapping;
using DomainUser = HexagonalSkeleton.Domain.User;
using HexagonalSkeleton.Infrastructure.Persistence.Entities;

namespace HexagonalSkeleton.Test.Unit.Infrastructure.Mapping
{
    public class UserMappingProfileTest
    {
        private readonly IMapper _mapper;

        public UserMappingProfileTest()
        {            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<InfrastructureMappingProfile>();
            });
            
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void Map_UserToUserEntity_ShouldMapCorrectly()
        {
            // Arrange
            var user = DomainUser.Create(
                email: "test@example.com",
                passwordSalt: "testsalt",
                passwordHash: "testhash",
                firstName: "John",
                lastName: "Doe",
                birthdate: new DateTime(1990, 1, 1),
                phoneNumber: "+1234567890",
                latitude: 40.7128,
                longitude: -74.0060,
                aboutMe: "Test user"
            );

            // Act
            var entity = _mapper.Map<UserEntity>(user);

            // Assert
            Assert.NotNull(entity);
            Assert.Equal("test@example.com", entity.Email);
            Assert.Equal("John", entity.Name);
            Assert.Equal("Doe", entity.Surname);
            Assert.Equal("+1234567890", entity.PhoneNumber);
            Assert.Equal(40.7128, entity.Latitude);
            Assert.Equal(-74.0060, entity.Longitude);
            Assert.Equal(new DateTime(1990, 1, 1), entity.Birthdate);
            Assert.Equal("Test user", entity.AboutMe);
            Assert.Equal("testsalt", entity.PasswordSalt);
            Assert.Equal("testhash", entity.PasswordHash);
        }

        [Fact]
        public void Map_UserEntityToUser_ShouldMapCorrectly()
        {
            // Arrange
            var entity = new UserEntity
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Name = "John",
                Surname = "Doe",
                PhoneNumber = "+1234567890",
                Latitude = 40.7128,
                Longitude = -74.0060,
                Birthdate = new DateTime(1990, 1, 1),
                AboutMe = "Test user",
                PasswordSalt = "testsalt",
                PasswordHash = "testhash",
                LastLogin = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                DeletedAt = null,
                IsDeleted = false,
                ProfileImageName = null
            };

            // Act
            var user = _mapper.Map<DomainUser>(entity);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(entity.Id, user.Id);
            Assert.Equal("test@example.com", user.Email.Value);
            Assert.Equal("John", user.FullName.FirstName);
            Assert.Equal("Doe", user.FullName.LastName);
            Assert.Equal("+1234567890", user.PhoneNumber.Value);
            Assert.Equal(40.7128, user.Location.Latitude);
            Assert.Equal(-74.0060, user.Location.Longitude);
            Assert.Equal(new DateTime(1990, 1, 1), user.Birthdate);
            Assert.Equal("Test user", user.AboutMe);
            Assert.Equal("testsalt", user.PasswordSalt);
            Assert.Equal("testhash", user.PasswordHash);
        }
    }
}
