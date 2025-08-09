using Xunit;
using FluentValidation.TestHelper;
using HexagonalSkeleton.Application.Features.UserManagement.Commands;

namespace HexagonalSkeleton.Test.Application.Features.UserManagement.Commands;

public class SoftDeleteUserCommandValidatorTest
{
    private readonly SoftDeleteUserCommandValidator _validator;

    public SoftDeleteUserCommandValidatorTest()
    {
        _validator = new SoftDeleteUserCommandValidator();
    }

    [Fact]
    public void Validate_ValidId_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = TestHelper.CreateSoftDeleteUserCommand();

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public void Validate_EmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        var command = TestHelper.CreateSoftDeleteUserCommand(Guid.Empty);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Id);
    }
}
