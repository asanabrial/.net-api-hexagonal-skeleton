namespace HexagonalSkeleton.Infrastructure.Services.Sync
{
    /// <summary>
    /// Represents sync status of a user in the query store
    /// </summary>
    public class UserSyncStatus
    {
        public Guid Id { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
