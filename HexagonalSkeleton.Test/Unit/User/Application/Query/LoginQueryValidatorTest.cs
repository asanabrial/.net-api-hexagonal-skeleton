using Xunit;
using FluentValidation.TestHelper;
using HexagonalSkeleton.Application.Query;

namespace HexagonalSkeleton.Test.Unit.User.Application.Query;

public class LoginQueryValidatorTest
{
    private readonly LoginQueryValidator _validator;

    public LoginQueryValidatorTest()
    {
        _validator = new LoginQueryValidator();
    }

    [Fact]
    public void Validate_ValidEmailAndPassword_ShouldNotHaveValidationError()
    {
        // Arrange
        var query = new LoginQuery("test@example.com", "Password123!");

        // Act & Assert
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(q => q.Email);
        result.ShouldNotHaveValidationErrorFor(q => q.Password);
    }    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_InvalidEmail_ShouldHaveValidationError(string invalidEmail)
    {
        // Arrange
        var query = new LoginQuery(invalidEmail, "Password123!");

        // Act & Assert
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(q => q.Email);
    }

    [Fact]
    public void Validate_NullEmail_ShouldHaveValidationError()
    {
        // Arrange
        var query = new LoginQuery(null!, "Password123!");

        // Act & Assert
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(q => q.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_InvalidPassword_ShouldHaveValidationError(string invalidPassword)
    {
        // Arrange
        var query = new LoginQuery("test@example.com", invalidPassword);

        // Act & Assert
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(q => q.Password);
    }

    [Fact]
    public void Validate_NullPassword_ShouldHaveValidationError()
    {
        // Arrange
        var query = new LoginQuery("test@example.com", null!);

        // Act & Assert
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(q => q.Password);
    }

    [Fact]
    public void Validate_BothEmailAndPasswordEmpty_ShouldHaveValidationErrors()
    {
        // Arrange
        var query = new LoginQuery("", "");

        // Act & Assert
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(q => q.Email);
        result.ShouldHaveValidationErrorFor(q => q.Password);
    }
}
