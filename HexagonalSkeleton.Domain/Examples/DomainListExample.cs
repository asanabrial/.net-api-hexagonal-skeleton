using HexagonalSkeleton.Domain.Common.Extensions;

namespace HexagonalSkeleton.Domain.Examples
{
    /// <summary>
    /// Example demonstrating ListExtension usage in Domain layer itself
    /// </summary>
    public class DomainListExample
    {
        public bool CheckDomainList()
        {
            var domainObjects = new List<string> { "aggregate1", "entity1" };
            return domainObjects.HasElements(); // ✅ Works - Domain can access its own extensions
        }
    }
}
