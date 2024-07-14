using HexagonalSkeleton.API.Config;
using HexagonalSkeleton.API.Features.User.Domain;
using HexagonalSkeleton.API.Features.User.Infrastructure;
using HexagonalSkeleton.CommonCore.Data;
using HexagonalSkeleton.CommonCore.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace HexagonalSkeleton.API.Data
{
    public class UnitOfWork(AppDbContext context) : GenericUnitOfWork(context), IUnitOfWork
    {
        public IUserRepository Users { get; set; } = new UserRepository(context);

        public async Task DeleteByCondition(Expression<Func<UserEntity, bool>> expression)
            => await context.Set<UserEntity>().Where(expression).ExecuteDeleteAsync();
    }
}
