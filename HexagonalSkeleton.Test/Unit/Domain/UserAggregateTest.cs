using Xunit;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.Events;
using HexagonalSkeleton.Test.TestInfrastructure.Helpers;

namespace HexagonalSkeleton.Test.Unit.User.Domain;

public class UserAggregateTest
{
    [Fact]
    public void Create_ValidData_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var email = "test@example.com";
        var passwordSalt = "salt";
        var passwordHash = "hash";
        var firstName = "John";
        var lastName = "Doe";
        var birthdate = DateTime.UtcNow.AddYears(-25);
        var phoneNumber = "+1234567890";
        var latitude = 40.7128;
        var longitude = -74.0060;
        var aboutMe = "Test about me";

        // Act
        var user = HexagonalSkeleton.Domain.User.Create(
            email, passwordSalt, passwordHash, firstName, lastName, 
            birthdate, phoneNumber, latitude, longitude, aboutMe);

        // Assert
        Assert.Equal(email, user.Email.Value);
        Assert.Equal(passwordSalt, user.PasswordSalt);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.Equal(firstName, user.FullName.FirstName);
        Assert.Equal(lastName, user.FullName.LastName);
        Assert.Equal(phoneNumber, user.PhoneNumber.Value);
        Assert.Equal(latitude, user.Location.Latitude);
        Assert.Equal(longitude, user.Location.Longitude);
        Assert.Equal(aboutMe, user.AboutMe);
        Assert.False(user.IsDeleted);
        Assert.Null(user.DeletedAt);
        Assert.True(user.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Create_ShouldRaiseUserCreatedEvent()
    {
        // Arrange
        var email = "test@example.com";
        var passwordSalt = "salt";
        var passwordHash = "hash";
        var firstName = "John";
        var lastName = "Doe";
        var birthdate = DateTime.UtcNow.AddYears(-25);
        var phoneNumber = "+1234567890";
        var latitude = 40.7128;
        var longitude = -74.0060;
        var aboutMe = "Test about me";

        // Act
        var user = HexagonalSkeleton.Domain.User.Create(
            email, passwordSalt, passwordHash, firstName, lastName, 
            birthdate, phoneNumber, latitude, longitude, aboutMe);

        // Assert
        Assert.Single(user.DomainEvents);
        var domainEvent = user.DomainEvents.First();
        Assert.IsType<UserCreatedEvent>(domainEvent);
          var userCreatedEvent = (UserCreatedEvent)domainEvent;
        Assert.Equal(user.Id, userCreatedEvent.UserId);
        Assert.Equal(email, userCreatedEvent.Email);
        Assert.Equal(firstName, userCreatedEvent.Name);
        Assert.Equal(lastName, userCreatedEvent.Surname);
        Assert.Equal(phoneNumber, userCreatedEvent.PhoneNumber);
    }

    [Fact]
    public void UpdateProfile_ValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var user = TestHelper.CreateTestUser();
        var newFirstName = "Jane";
        var newLastName = "Smith";
        var newBirthdate = DateTime.UtcNow.AddYears(-30);
        var newAboutMe = "Updated about me";

        // Act
        user.UpdateProfile(newFirstName, newLastName, newBirthdate, newAboutMe);

        // Assert
        Assert.Equal(newFirstName, user.FullName.FirstName);
        Assert.Equal(newLastName, user.FullName.LastName);
        Assert.Equal(newBirthdate, user.Birthdate);
        Assert.Equal(newAboutMe, user.AboutMe);
        Assert.NotNull(user.UpdatedAt);
        
        // Should raise profile updated event when name changes
        Assert.Single(user.DomainEvents);
        var domainEvent = user.DomainEvents.First();
        Assert.IsType<UserProfileUpdatedEvent>(domainEvent);
    }

    [Fact]
    public void Delete_ShouldMarkAsDeleted()
    {
        // Arrange
        var user = TestHelper.CreateTestUser();

        // Act
        user.Delete();

        // Assert
        Assert.True(user.IsDeleted);
        Assert.NotNull(user.DeletedAt);
        Assert.True(user.DeletedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void UpdateProfile_DeletedUser_ShouldThrowException()
    {
        // Arrange
        var user = TestHelper.CreateTestUser();
        user.Delete();

        // Act & Assert
        Assert.Throws<HexagonalSkeleton.Domain.Exceptions.UserDomainException>(() => 
            user.UpdateProfile("Jane", "Smith", DateTime.UtcNow.AddYears(-30), "Updated about me"));
    }

    [Fact]  
    public void RecordLogin_ShouldUpdateLastLoginAndRaiseEvent()
    {
        // Arrange
        var user = TestHelper.CreateTestUser();
        var oldLastLogin = user.LastLogin;

        // Act
        user.RecordLogin();

        // Assert
        Assert.True(user.LastLogin > oldLastLogin);
        Assert.Single(user.DomainEvents);
        var domainEvent = user.DomainEvents.First();
        Assert.IsType<UserLoggedInEvent>(domainEvent);
    }

    [Fact]
    public void RecordLogin_DeletedUser_ShouldThrowException()
    {
        // Arrange
        var user = TestHelper.CreateTestUser();
        user.Delete();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => user.RecordLogin());
    }

    [Fact]
    public void UpdateLocation_ValidCoordinates_ShouldUpdateSuccessfully()
    {
        // Arrange
        var user = TestHelper.CreateTestUser();
        var newLatitude = 34.0522;
        var newLongitude = -118.2437;

        // Act
        user.UpdateLocation(newLatitude, newLongitude);

        // Assert
        Assert.Equal(newLatitude, user.Location.Latitude);
        Assert.Equal(newLongitude, user.Location.Longitude);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void UpdatePhoneNumber_ValidPhoneNumber_ShouldUpdateSuccessfully()
    {
        // Arrange
        var user = TestHelper.CreateTestUser();
        var newPhoneNumber = "+1987654321";

        // Act
        user.UpdatePhoneNumber(newPhoneNumber);

        // Assert
        Assert.Equal(newPhoneNumber, user.PhoneNumber.Value);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void GetAge_ShouldCalculateCorrectAge()
    {
        // Arrange
        var birthdate = DateTime.UtcNow.AddYears(-25);
        var user = TestHelper.CreateTestUser(birthdate: birthdate);

        // Act
        var age = user.GetAge();

        // Assert
        Assert.Equal(25, age);
    }

    [Fact]
    public void IsAdult_UserOver18_ShouldReturnTrue()
    {
        // Arrange
        var birthdate = DateTime.UtcNow.AddYears(-25);
        var user = TestHelper.CreateTestUser(birthdate: birthdate);

        // Act
        var isAdult = user.IsAdult();

        // Assert
        Assert.True(isAdult);
    }

    [Fact]
    public void IsAdult_UserUnder18_ShouldReturnFalse()
    {
        // Arrange
        var birthdate = DateTime.UtcNow.AddYears(-16);
        var user = TestHelper.CreateTestUser(birthdate: birthdate);

        // Act
        var isAdult = user.IsAdult();

        // Assert
        Assert.False(isAdult);
    }

    [Fact]
    public void Create_UserUnder13_ShouldThrowException()
    {
        // Arrange
        var birthdate = DateTime.UtcNow.AddYears(-10);

        // Act & Assert
        Assert.Throws<HexagonalSkeleton.Domain.Exceptions.UserDomainException>(() => 
            HexagonalSkeleton.Domain.User.Create(
                "test@example.com", "salt", "hash", "John", "Doe", 
                birthdate, "+1234567890", 40.7128, -74.0060, "About me"));
    }

    [Fact]
    public void SetProfileImage_ValidFileName_ShouldSetImage()
    {
        // Arrange
        var user = TestHelper.CreateTestUser();
        var fileName = "profile.jpg";

        // Act
        user.SetProfileImage(fileName);

        // Assert
        Assert.Equal(fileName, user.ProfileImageName);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void RemoveProfileImage_ShouldClearImage()
    {
        // Arrange
        var user = TestHelper.CreateTestUser();
        user.SetProfileImage("profile.jpg");

        // Act
        user.RemoveProfileImage();

        // Assert
        Assert.Null(user.ProfileImageName);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]  
    public void ChangePassword_ValidCredentials_ShouldUpdatePassword()
    {
        // Arrange
        var user = TestHelper.CreateTestUser();
        var newSalt = "newSalt";
        var newHash = "newHash";

        // Act
        user.ChangePassword(newSalt, newHash);

        // Assert
        Assert.Equal(newSalt, user.PasswordSalt);
        Assert.Equal(newHash, user.PasswordHash);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void CalculateDistanceTo_SameUser_ShouldReturnZero()
    {
        // Arrange
        var user = TestHelper.CreateTestUser();

        // Act
        var distance = user.CalculateDistanceTo(user);

        // Assert
        Assert.Equal(0, distance, 1); // Allow 1km tolerance
    }

    [Fact]
    public void CalculateDistanceTo_DifferentUsers_ShouldReturnCorrectDistance()
    {
        // Arrange
        var user1 = TestHelper.CreateTestUser(latitude: 40.7128, longitude: -74.0060); // NYC
        var user2 = TestHelper.CreateTestUser(id: Guid.NewGuid(), email: "user2@example.com", phoneNumber: "+1234567891",
                                              latitude: 51.5074, longitude: -0.1278); // London

        // Act
        var distance = user1.CalculateDistanceTo(user2);

        // Assert
        Assert.True(distance > 5500 && distance < 5600, $"Expected distance around 5545km, but got {distance}km");
    }

    [Fact]
    public void IsNearby_UsersWithinRadius_ShouldReturnTrue()
    {
        // Arrange
        var user1 = TestHelper.CreateTestUser(latitude: 40.7128, longitude: -74.0060);
        var user2 = TestHelper.CreateTestUser(id: Guid.NewGuid(), email: "user2@example.com", phoneNumber: "+1234567891",
                                              latitude: 40.7589, longitude: -73.9851); // Times Square (nearby)

        // Act
        var isNearby = user1.IsNearby(user2, 50); // 50km radius

        // Assert
        Assert.True(isNearby);
    }

    [Fact]
    public void IsNearby_UsersOutsideRadius_ShouldReturnFalse()
    {
        // Arrange
        var user1 = TestHelper.CreateTestUser(latitude: 40.7128, longitude: -74.0060); // NYC
        var user2 = TestHelper.CreateTestUser(id: Guid.NewGuid(), email: "user2@example.com", phoneNumber: "+1234567891",
                                              latitude: 51.5074, longitude: -0.1278); // London

        // Act
        var isNearby = user1.IsNearby(user2, 1000); // 1000km radius (still not enough for NYC-London)

        // Assert
        Assert.False(isNearby);
    }

    [Fact]
    public void SetProfileImage_EmptyFileName_ShouldThrowException()
    {
        // Arrange
        var user = TestHelper.CreateTestUser();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => user.SetProfileImage(""));
        Assert.Throws<ArgumentException>(() => user.SetProfileImage("   "));
    }

    [Fact]
    public void ChangePassword_EmptyCredentials_ShouldThrowException()
    {
        // Arrange
        var user = TestHelper.CreateTestUser();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => user.ChangePassword("", "hash"));
        Assert.Throws<ArgumentException>(() => user.ChangePassword("salt", ""));
        Assert.Throws<ArgumentException>(() => user.ChangePassword("   ", "hash"));
        Assert.Throws<ArgumentException>(() => user.ChangePassword("salt", "   "));
    }    [Fact]
    public void UpdateLocation_DeletedUser_ShouldThrowException()
    {
        // Arrange
        var user = TestHelper.CreateTestUser();
        user.Delete();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => user.UpdateLocation(34.0522, -118.2437));
    }

    [Fact]
    public void UpdatePhoneNumber_DeletedUser_ShouldThrowException()
    {
        // Arrange
        var user = TestHelper.CreateTestUser();
        user.Delete();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => user.UpdatePhoneNumber("+1987654321"));
    }

    [Fact]
    public void SetProfileImage_DeletedUser_ShouldThrowException()
    {
        // Arrange
        var user = TestHelper.CreateTestUser();
        user.Delete();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => user.SetProfileImage("profile.jpg"));
    }

    [Fact]
    public void ChangePassword_DeletedUser_ShouldThrowException()
    {
        // Arrange
        var user = TestHelper.CreateTestUser();
        user.Delete();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => user.ChangePassword("newSalt", "newHash"));
    }
}
