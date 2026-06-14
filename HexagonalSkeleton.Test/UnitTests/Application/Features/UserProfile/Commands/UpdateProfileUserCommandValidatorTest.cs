using Xunit;
using FluentValidation.TestHelper;
using HexagonalSkeleton.Application.Features.UserProfile.Commands;
using HexagonalSkeleton.Test.TestHelpers;

namespace HexagonalSkeleton.Test.Application.Features.UserProfile.Commands;

public class UpdateProfileUserCommandValidatorTest
{
    private readonly UpdateProfileUserCommandValidator _validator;

    public UpdateProfileUserCommandValidatorTest()
    {
        _validator = new UpdateProfileUserCommandValidator();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_InvalidAboutMe_ShouldHaveValidationError(string? invalidAboutMe)
    {
        var command = CommandTestDataBuilder.CreateValidUpdateProfileCommand(aboutMe: invalidAboutMe!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.AboutMe);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_InvalidFirstName_ShouldHaveValidationError(string? invalidFirstName)
    {
        var command = CommandTestDataBuilder.CreateValidUpdateProfileCommand(firstName: invalidFirstName!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FirstName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_InvalidLastName_ShouldHaveValidationError(string? invalidLastName)
    {
        var command = CommandTestDataBuilder.CreateValidUpdateProfileCommand(lastName: invalidLastName!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.LastName);
    }
}
