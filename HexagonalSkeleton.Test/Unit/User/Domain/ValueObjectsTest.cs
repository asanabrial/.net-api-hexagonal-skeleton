using HexagonalSkeleton.Domain.ValueObjects;
using Xunit;

namespace HexagonalSkeleton.Test.Unit.User.Domain
{
    public class ValueObjectsTest
    {
        [Theory]
        [InlineData("test@example.com")]
        [InlineData("user.name@domain.co.uk")]
        [InlineData("test+tag@example.org")]
        public void Email_Should_Create_With_Valid_Email(string validEmail)
        {
            // Act & Assert
            var email = new Email(validEmail);
            Assert.Equal(validEmail, email.Value);
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid-email")]
        [InlineData("@example.com")]
        [InlineData("test@")]
        [InlineData("test@.com")]
        public void Email_Should_Throw_For_Invalid_Email(string invalidEmail)
        {
            // Act & Assert
            var act = () => new Email(invalidEmail);
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void FullName_Should_Create_With_Valid_Names()
        {
            // Arrange
            var firstName = "John";
            var lastName = "Doe";

            // Act
            var fullName = new FullName(firstName, lastName);            // Assert            Assert.Equal(firstName, fullName.FirstName);
            Assert.Equal(lastName, fullName.LastName);
            Assert.Equal($"{firstName} {lastName}", fullName.GetFullName());
        }

        [Theory]
        [InlineData("", "Doe")]
        [InlineData("John", "")]
        [InlineData("   ", "Doe")]
        [InlineData("John", "   ")]
        public void FullName_Should_Throw_For_Invalid_Names(string firstName, string lastName)
        {
            // Act & Assert
            var act = () => new FullName(firstName, lastName);
            Assert.Throws<ArgumentException>(act);
        }        [Theory]
        [InlineData("123456789", "123456789")]
        [InlineData("+1234567890", "+1234567890")]
        [InlineData("555-123-4567", "5551234567")]
        public void PhoneNumber_Should_Create_With_Valid_Phone(string inputPhone, string expectedPhone)
        {
            // Act & Assert
            var phoneNumber = new PhoneNumber(inputPhone);
            Assert.Equal(expectedPhone, phoneNumber.Value);
        }

        [Theory]
        [InlineData("")]
        [InlineData("123")]
        [InlineData("abcdefghij")]
        public void PhoneNumber_Should_Throw_For_Invalid_Phone(string invalidPhone)
        {
            // Act & Assert
            var act = () => new PhoneNumber(invalidPhone);
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void Location_Should_Create_With_Valid_Coordinates()
        {
            // Arrange
            var latitude = 40.7128;
            var longitude = -74.0060;

            // Act
            var location = new Location(latitude, longitude);

            // Assert
            Assert.Equal(latitude, location.Latitude);
            Assert.Equal(longitude, location.Longitude);
        }

        [Theory]
        [InlineData(91, 0)] // Latitude too high
        [InlineData(-91, 0)] // Latitude too low
        [InlineData(0, 181)] // Longitude too high
        [InlineData(0, -181)] // Longitude too low
        public void Location_Should_Throw_For_Invalid_Coordinates(double latitude, double longitude)
        {
            // Act & Assert
            var act = () => new Location(latitude, longitude);
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void ValueObjects_Should_Be_Equal_When_Values_Are_Same()
        {
            // Arrange
            var email1 = new Email("test@example.com");
            var email2 = new Email("test@example.com");

            var fullName1 = new FullName("John", "Doe");
            var fullName2 = new FullName("John", "Doe");

            var phoneNumber1 = new PhoneNumber("123456789");
            var phoneNumber2 = new PhoneNumber("123456789");

            var location1 = new Location(40.7128, -74.0060);
            var location2 = new Location(40.7128, -74.0060);

            // Act & Assert
            Assert.Equal(email2, email1);
            Assert.Equal(fullName2, fullName1);
            Assert.Equal(phoneNumber2, phoneNumber1);
            Assert.Equal(location2, location1);
        }

        [Fact]
        public void ValueObjects_Should_Not_Be_Equal_When_Values_Are_Different()
        {
            // Arrange
            var email1 = new Email("test1@example.com");
            var email2 = new Email("test2@example.com");

            var fullName1 = new FullName("John", "Doe");
            var fullName2 = new FullName("Jane", "Smith");

            var phoneNumber1 = new PhoneNumber("123456789");
            var phoneNumber2 = new PhoneNumber("987654321");

            var location1 = new Location(40.7128, -74.0060);
            var location2 = new Location(34.0522, -118.2437);

            // Act & Assert
            Assert.NotEqual(email2, email1);
            Assert.NotEqual(fullName2, fullName1);
            Assert.NotEqual(phoneNumber2, phoneNumber1);
            Assert.NotEqual(location2, location1);
        }
    }
}
