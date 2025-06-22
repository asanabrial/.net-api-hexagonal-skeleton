namespace HexagonalSkeleton.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing pagination parameters
    /// Encapsulates pagination logic and validation
    /// </summary>
    public record PaginationParams
    {
        public const int MaxPageSize = 100;
        public const int DefaultPageSize = 10;
        public const int MinPageNumber = 1;

        public int PageNumber { get; }
        public int PageSize { get; }
        
        public PaginationParams(int pageNumber, int pageSize)
        {
            if (pageNumber < MinPageNumber)
                throw new ArgumentException($"Page number must be at least {MinPageNumber}", nameof(pageNumber));
            
            if (pageSize <= 0)
                throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));
            
            if (pageSize > MaxPageSize)
                throw new ArgumentException($"Page size cannot exceed {MaxPageSize}", nameof(pageSize));

            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public static PaginationParams Create(int pageNumber, int? pageSize = null)
        {
            return new PaginationParams(pageNumber, pageSize ?? DefaultPageSize);
        }

        public int Skip => (PageNumber - 1) * PageSize;
        public int Take => PageSize;
    }
}
