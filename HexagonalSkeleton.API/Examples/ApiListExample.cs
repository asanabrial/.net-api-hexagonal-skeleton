using HexagonalSkeleton.Domain.Common.Extensions;

namespace HexagonalSkeleton.API.Examples
{
    /// <summary>
    /// Example demonstrating ListExtension usage in API layer
    /// </summary>
    public class ApiListExample
    {
        public bool CheckApiList()
        {
            var items = new List<string> { "item1", "item2" };
            return items.HasElements(); // ✅ Works - API can access Domain
        }
    }
}
