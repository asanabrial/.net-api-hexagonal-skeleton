using FluentAssertions;
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
            email.Value.Should().Be(validEmail);
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
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void FullName_Should_Create_With_Valid_Names()
        {
            // Arrange
            var firstName = "John";
            var lastName = "Doe";

            // Act
            var fullName = new FullName(firstName, lastName);

            // Assert            fullName.FirstName.Should().Be(firstName);
            fullName.LastName.Should().Be(lastName);
            fullName.GetFullName().Should().Be($"{firstName} {lastName}");
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
            act.Should().Throw<ArgumentException>();
        }        [Theory]
        [InlineData("123456789", "123456789")]
        [InlineData("+1234567890", "+1234567890")]
        [InlineData("555-123-4567", "5551234567")]
        public void PhoneNumber_Should_Create_With_Valid_Phone(string inputPhone, string expectedPhone)
        {
            // Act & Assert
            var phoneNumber = new PhoneNumber(inputPhone);
            phoneNumber.Value.Should().Be(expectedPhone);
        }

        [Theory]
        [InlineData("")]
        [InlineData("123")]
        [InlineData("abcdefghij")]
        public void PhoneNumber_Should_Throw_For_Invalid_Phone(string invalidPhone)
        {
            // Act & Assert
            var act = () => new PhoneNumber(invalidPhone);
            act.Should().Throw<ArgumentException>();
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
            location.Latitude.Should().Be(latitude);
            location.Longitude.Should().Be(longitude);
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
            act.Should().Throw<ArgumentException>();
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
            email1.Should().Be(email2);
            fullName1.Should().Be(fullName2);
            phoneNumber1.Should().Be(phoneNumber2);
            location1.Should().Be(location2);
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
            email1.Should().NotBe(email2);
            fullName1.Should().NotBe(fullName2);
            phoneNumber1.Should().NotBe(phoneNumber2);
            location1.Should().NotBe(location2);
        }
    }
}
