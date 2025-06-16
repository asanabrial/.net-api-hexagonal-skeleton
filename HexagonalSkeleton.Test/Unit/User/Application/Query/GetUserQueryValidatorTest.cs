using Xunit;
using FluentValidation.TestHelper;
using HexagonalSkeleton.Application.Query;

namespace HexagonalSkeleton.Test.Unit.User.Application.Query;

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
        var query = new GetUserQuery(1);

        // Act & Assert
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(q => q.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InvalidId_ShouldHaveValidationError(int invalidId)
    {
        // Arrange
        var query = new GetUserQuery(invalidId);

        // Act & Assert
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(q => q.Id);
    }
}
