namespace HexagonalSkeleton.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing pagination parameters with sorting support
    /// Encapsulates pagination logic and validation
    /// </summary>
    public record PaginationParams
    {
        public const int MaxPageSize = 100;
        public const int DefaultPageSize = 10;
        public const int MinPageNumber = 1;
        public const string DefaultSortDirection = "asc";

        public int PageNumber { get; }
        public int PageSize { get; }
        public string? SortBy { get; }
        public string SortDirection { get; }
        
        public PaginationParams(int pageNumber, int pageSize, string? sortBy = null, string? sortDirection = null)
        {
            if (pageNumber < MinPageNumber)
                throw new ArgumentException($"Page number must be at least {MinPageNumber}", nameof(pageNumber));
            
            if (pageSize <= 0)
                throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));
            
            if (pageSize > MaxPageSize)
                throw new ArgumentException($"Page size cannot exceed {MaxPageSize}", nameof(pageSize));            PageNumber = pageNumber;
            PageSize = pageSize;
            SortBy = string.IsNullOrWhiteSpace(sortBy) ? null : sortBy.Trim();
            SortDirection = ValidateSortDirection(sortDirection?.Trim()) && !string.IsNullOrWhiteSpace(sortDirection) 
                            ? sortDirection.Trim().ToLowerInvariant() 
                            : DefaultSortDirection;
        }

        public static PaginationParams Create(int pageNumber, int? pageSize = null, string? sortBy = null, string? sortDirection = null)
        {
            return new PaginationParams(pageNumber, pageSize ?? DefaultPageSize, sortBy, sortDirection);
        }

        public int Skip => (PageNumber - 1) * PageSize;
        public int Take => PageSize;
        
        public bool HasSorting => !string.IsNullOrWhiteSpace(SortBy);
        public bool IsAscending => SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase);
        public bool IsDescending => SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        private static bool ValidateSortDirection(string? sortDirection)
        {
            if (string.IsNullOrWhiteSpace(sortDirection))
                return true;
                
            var normalized = sortDirection.Trim().ToLowerInvariant();
            return normalized == "asc" || normalized == "desc";
        }
    }
}
