namespace HexagonalSkeleton.CommonCore.Data.Repository
{
    public interface IGenericRepository
    {
        public bool SaveChanges();
        public Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
