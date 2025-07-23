using Xunit;
using FluentValidation.TestHelper;
using HexagonalSkeleton.Application.Features.UserManagement.Commands;
using HexagonalSkeleton.Test.TestInfrastructure.Helpers;

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

    [Fact]
    public void Validate_EmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        var command = TestHelper.CreateHardDeleteUserCommand(Guid.Empty);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Id);
    }
}
