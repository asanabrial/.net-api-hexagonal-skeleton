using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Infrastructure.Persistence.Query.Documents;
using HexagonalSkeleton.Infrastructure.Mapping;
using Xunit;

namespace HexagonalSkeleton.Test.Mapping
{
    /// <summary>
    /// Tests for the hand-written Infrastructure mappers that replaced AutoMapper.
    /// Covers the MongoDB read document -> domain User reconstruction.
    /// </summary>
    public class UserQueryDocumentMapperTests
    {
        private static UserQueryDocument BuildDocument(string email) => new()
        {
            Id = Guid.NewGuid(),
            Email = email,
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

        [Fact]
        public void ToDomain_MapsDocumentToUser()
        {
            var document = BuildDocument("test@example.com");

            var user = document.ToDomain();

            Assert.NotNull(user);
            Assert.Equal(document.Id, user.Id);
            Assert.Equal(document.Email, user.Email.Value);
            Assert.Equal("John", user.FullName.FirstName);
            Assert.Equal("Doe", user.FullName.LastName);
        }

        [Fact]
        public void ToDomain_MapsEachDocumentInList()
        {
            var documents = new List<UserQueryDocument> { BuildDocument("test1@example.com") };

            var users = documents.Select(d => d.ToDomain()).ToList();

            Assert.Single(users);
            Assert.Equal(documents[0].Email, users[0].Email.Value);
        }
    }
}
