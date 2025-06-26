using Xunit;
using FluentValidation.TestHelper;
using HexagonalSkeleton.Application.Features.UserProfile.Commands;

namespace HexagonalSkeleton.Test.Application.Features.UserProfile.Commands;

public class UpdateProfileUserCommandValidatorTest
{
    private readonly UpdateProfileUserCommandValidator _validator;

    public UpdateProfileUserCommandValidatorTest()
    {
        _validator = new UpdateProfileUserCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = TestHelper.CreateUpdateProfileUserCommand();

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.AboutMe);
        result.ShouldNotHaveValidationErrorFor(c => c.FirstName);
        result.ShouldNotHaveValidationErrorFor(c => c.LastName);
        result.ShouldNotHaveValidationErrorFor(c => c.Birthdate);
    }    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_InvalidAboutMe_ShouldHaveValidationError(string invalidAboutMe)
    {
        // Arrange
        var command = TestHelper.CreateUpdateProfileUserCommand(aboutMe: invalidAboutMe);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.AboutMe);
    }

    [Fact]
    public void Validate_NullAboutMe_ShouldHaveValidationError()
    {
        // Arrange
        var command = TestHelper.CreateUpdateProfileUserCommand(aboutMe: null!);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.AboutMe);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_InvalidFirstName_ShouldHaveValidationError(string invalidFirstName)
    {
        // Arrange
        var command = TestHelper.CreateUpdateProfileUserCommand(firstName: invalidFirstName);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.FirstName);
    }

    [Fact]
    public void Validate_NullFirstName_ShouldHaveValidationError()
    {
        // Arrange
        var command = TestHelper.CreateUpdateProfileUserCommand(firstName: null!);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.FirstName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_InvalidLastName_ShouldHaveValidationError(string invalidLastName)
    {
        // Arrange
        var command = TestHelper.CreateUpdateProfileUserCommand(lastName: invalidLastName);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.LastName);
    }

    [Fact]
    public void Validate_NullLastName_ShouldHaveValidationError()
    {
        // Arrange
        var command = TestHelper.CreateUpdateProfileUserCommand(lastName: null!);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.LastName);
    }
}
