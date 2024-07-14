using HexagonalSkeleton.API.Features.User.Domain;
using HexagonalSkeleton.CommonCore.Data.UnitOfWork;

namespace HexagonalSkeleton.API.Data
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        public IUserRepository Users { get; }
    }
}
