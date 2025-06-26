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

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InvalidId_ShouldHaveValidationError(int invalidId)
    {
        // Arrange
        var command = TestHelper.CreateSoftDeleteUserCommand(invalidId);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Id);
    }
}
