using Xunit;
using FluentValidation.TestHelper;
using HexagonalSkeleton.Application.Command;

namespace HexagonalSkeleton.Test.Unit.User.Application.Command;

public class RegisterUserCommandValidatorTest
{
    private readonly RegisterUserCommandValidator _validator;

    public RegisterUserCommandValidatorTest()
    {
        _validator = new RegisterUserCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand(email: string.Empty);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand(email: "invalid-email");

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Too_Short()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand(password: "123");

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Too_Long()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand(password: new string('a', 101));

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand(name: string.Empty);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Too_Long()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand(name: new string('a', 51));

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Surname_Is_Empty()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand(surname: string.Empty);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Surname);
    }

    [Fact]
    public void Should_Have_Error_When_Surname_Is_Too_Long()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand(surname: new string('a', 51));

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Surname);
    }

    [Fact]
    public void Should_Have_Error_When_PhoneNumber_Is_Empty()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand(phoneNumber: string.Empty);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Should_Have_Error_When_PhoneNumber_Is_Invalid()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand(phoneNumber: "invalid");

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Should_Have_Error_When_Passwords_Do_Not_Match()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand(
            password: "Password123!",
            passwordConfirmation: "DifferentPassword123!");

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PasswordConfirmation);
    }

    [Fact]
    public void Should_Have_Error_When_AboutMe_Is_Too_Long()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand();
        command.AboutMe = new string('a', 501);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.AboutMe);
    }

    [Fact]
    public void Should_Not_Have_Errors_When_All_Fields_Are_Valid()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand();

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Latitude_Is_Out_Of_Range()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand(latitude: 91.0);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Latitude);
    }

    [Fact]
    public void Should_Have_Error_When_Longitude_Is_Out_Of_Range()
    {
        // Arrange
        var command = TestHelper.CreateRegisterUserCommand(longitude: 181.0);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Longitude);
    }
}
