using HexagonalSkeleton.Domain.Common.Extensions;

namespace HexagonalSkeleton.Application.Examples
{
    /// <summary>
    /// Example demonstrating ListExtension usage in Application layer
    /// </summary>
    public class ApplicationListExample
    {
        public bool CheckApplicationList()
        {
            var commands = new List<string> { "command1", "command2" };
            return commands.HasElements(); // ✅ Works - Application can access Domain
        }
    }
}
