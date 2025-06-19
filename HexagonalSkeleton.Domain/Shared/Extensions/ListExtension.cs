namespace HexagonalSkeleton.Domain.Shared.Extensions
{
    /// <summary>
    /// Extension methods for List operations
    /// Generic technical utility (not domain logic)
    /// Placed in Domain.Shared to be accessible from all layers while keeping conceptual separation
    /// </summary>
    public static class ListExtension
    {
        /// <summary>
        /// Checks if a list has any elements
        /// </summary>
        /// <typeparam name="T">Type of list elements</typeparam>
        /// <param name="list">The list to check</param>
        /// <returns>True if list is not null and contains at least one element</returns>
        public static bool HasElements<T>(this List<T>? list) => list != null && list.Count != 0;
    }
}
