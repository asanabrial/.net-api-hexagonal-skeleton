using HexagonalSkeleton.API.Features.User.Domain;
using HexagonalSkeleton.CommonCore.Data.UnitOfWork;
using HexagonalSkeleton.API.Features.User.Infrastructure;

namespace HexagonalSkeleton.API.Data
{
    public class UnitOfWork(AppDbContext context) : GenericUnitOfWork(context), IUnitOfWork
    {
        public IUserRepository Users { get; set; } = new UserRepository(context);
    }
}
