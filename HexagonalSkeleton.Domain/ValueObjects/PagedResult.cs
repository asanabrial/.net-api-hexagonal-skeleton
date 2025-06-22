namespace HexagonalSkeleton.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing paginated results
    /// Encapsulates pagination metadata along with data
    /// </summary>
    public record PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int TotalCount { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public int TotalPages { get; }
        public bool HasNextPage { get; }
        public bool HasPreviousPage { get; }

        public PagedResult(IEnumerable<T> items, int totalCount, PaginationParams pagination)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            
            if (totalCount < 0)
                throw new ArgumentException("Total count cannot be negative", nameof(totalCount));
            
            if (pagination == null)
                throw new ArgumentNullException(nameof(pagination));

            Items = items.ToList().AsReadOnly();
            TotalCount = totalCount;
            PageNumber = pagination.PageNumber;
            PageSize = pagination.PageSize;
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);
            HasNextPage = PageNumber < TotalPages;
            HasPreviousPage = PageNumber > 1;
        }

        public static PagedResult<T> Empty(PaginationParams pagination)
        {
            return new PagedResult<T>(Enumerable.Empty<T>(), 0, pagination);
        }
    }
}
