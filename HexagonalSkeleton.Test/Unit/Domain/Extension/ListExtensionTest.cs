using HexagonalSkeleton.Domain.Shared.Extensions;
using Xunit;

namespace HexagonalSkeleton.Test.Unit.CommonCore.Extension
{
    public class ListExtensionTest
    {
        [Fact]
        public void HasElements_WithNullList_ShouldReturnFalse()
        {
            // Arrange
            List<string>? nullList = null;

            // Act
            var result = nullList.HasElements();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasElements_WithEmptyList_ShouldReturnFalse()
        {
            // Arrange
            var emptyList = new List<string>();

            // Act
            var result = emptyList.HasElements();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasElements_WithOneElement_ShouldReturnTrue()
        {
            // Arrange
            var listWithOneElement = new List<string> { "test" };

            // Act
            var result = listWithOneElement.HasElements();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasElements_WithMultipleElements_ShouldReturnTrue()
        {
            // Arrange
            var listWithMultipleElements = new List<string> { "test1", "test2", "test3" };

            // Act
            var result = listWithMultipleElements.HasElements();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasElements_WithDifferentTypes_ShouldWork()
        {
            // Arrange
            var intList = new List<int> { 1, 2, 3 };
            var boolList = new List<bool> { true };
            var emptyIntList = new List<int>();            // Act & Assert
            Assert.True(intList.HasElements());
            Assert.True(boolList.HasElements());
            Assert.False(emptyIntList.HasElements());
        }
    }
}
