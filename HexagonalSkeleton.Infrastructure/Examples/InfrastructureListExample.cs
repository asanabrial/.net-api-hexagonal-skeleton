using HexagonalSkeleton.Domain.Common.Extensions;

namespace HexagonalSkeleton.Infrastructure.Examples
{
    /// <summary>
    /// Example demonstrating ListExtension usage in Infrastructure layer
    /// </summary>
    public class InfrastructureListExample
    {
        public bool CheckInfrastructureList()
        {
            var entities = new List<string> { "entity1", "entity2" };
            return entities.HasElements(); // ✅ Works - Infrastructure can access Domain
        }
    }
}
