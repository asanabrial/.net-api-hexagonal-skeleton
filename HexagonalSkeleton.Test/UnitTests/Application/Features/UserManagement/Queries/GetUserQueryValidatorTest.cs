using Xunit;
using FluentValidation.TestHelper;
using HexagonalSkeleton.Application.Features.UserManagement.Queries;

namespace HexagonalSkeleton.Test.Application.Features.UserManagement.Queries;

public class GetUserQueryValidatorTest
{
    private readonly GetUserQueryValidator _validator;

    public GetUserQueryValidatorTest()
    {
        _validator = new GetUserQueryValidator();
    }

    [Fact]
    public void Validate_ValidId_ShouldNotHaveValidationError()
    {
        // Arrange
        var query = new GetUserQuery(Guid.NewGuid());

        // Act & Assert
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(q => q.Id);
    }

    [Fact]
    public void Validate_EmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        var query = new GetUserQuery(Guid.Empty);

        // Act & Assert
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(q => q.Id);
    }
}
