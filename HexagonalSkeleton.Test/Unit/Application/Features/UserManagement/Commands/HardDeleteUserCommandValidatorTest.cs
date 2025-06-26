using Xunit;
using FluentValidation.TestHelper;
using HexagonalSkeleton.Application.Features.UserManagement.Commands;

namespace HexagonalSkeleton.Test.Application.Features.UserManagement.Commands;

public class HardDeleteUserCommandValidatorTest
{
    private readonly HardDeleteUserCommandValidator _validator;

    public HardDeleteUserCommandValidatorTest()
    {
        _validator = new HardDeleteUserCommandValidator();
    }

    [Fact]
    public void Validate_ValidId_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = TestHelper.CreateHardDeleteUserCommand();

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
        var command = TestHelper.CreateHardDeleteUserCommand(invalidId);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Id);
    }
}
