namespace HexagonalSkeleton.CommonCore.Data.UnitOfWork
{
    public interface IDatabaseTransaction : IDisposable
    {
        void Commit();

        void Rollback();
    }
}
