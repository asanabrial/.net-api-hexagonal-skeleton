namespace HexagonalSkeleton.CommonCore.Extension
{
    public static class ListExtension
    {
        public static bool HasElements<T>(this List<T>? list) => list != null && list.Count != 0;

    }
}
