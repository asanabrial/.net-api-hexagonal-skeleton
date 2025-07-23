using AutoMapper;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Infrastructure.Persistence.Query.Documents;
using HexagonalSkeleton.Infrastructure.Mapping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Xunit;

namespace HexagonalSkeleton.Test.Mapping
{
    public class AutoMapperTests
    {
        private readonly IMapper _mapper;

        public AutoMapperTests()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<InfrastructureMappingProfile>();
            });
            // Verify all mappings are properly configured
            configuration.AssertConfigurationIsValid();
            _mapper = new Mapper(configuration);
        }

        [Fact]
        public void Should_Map_UserQueryDocument_To_User()
        {
            // Arrange
            var document = new UserQueryDocument
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FullName = new FullNameDocument
                {
                    FirstName = "John",
                    LastName = "Doe",
                    DisplayName = "John Doe"
                },
                PhoneNumber = "+1234567890",
                Birthdate = DateTime.UtcNow.AddYears(-25),
                Location = new LocationDocument
                {
                    Latitude = 40.7128,
                    Longitude = -74.006
                },
                AboutMe = "Test about me",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            // Act & Assert
            var user = _mapper.Map<User>(document);
            Assert.NotNull(user);
            Assert.Equal(document.Id, user.Id);
            Assert.Equal(document.Email, user.Email.Value);
        }

        [Fact]
        public void Should_Map_List_Of_UserQueryDocument_To_List_Of_User()
        {
            // Arrange
            var documents = new List<UserQueryDocument>
            {
                new UserQueryDocument
                {
                    Id = Guid.NewGuid(),
                    Email = "test1@example.com",
                    FullName = new FullNameDocument
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        DisplayName = "John Doe"
                    },
                    PhoneNumber = "+1234567890",
                    Birthdate = DateTime.UtcNow.AddYears(-25),
                    Location = new LocationDocument
                    {
                        Latitude = 40.7128,
                        Longitude = -74.006
                    },
                    AboutMe = "Test about me",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            };

            // Act & Assert
            var users = _mapper.Map<List<User>>(documents);
            Assert.NotNull(users);
            Assert.Single(users);
            Assert.Equal(documents[0].Email, users[0].Email.Value);
        }
    }
}
