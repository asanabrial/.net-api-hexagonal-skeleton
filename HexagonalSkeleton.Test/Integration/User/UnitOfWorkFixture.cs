using AutoFixture;
using EntityFrameworkCore.AutoFixture.InMemory;
using HexagonalSkeleton.API.Data;
using HexagonalSkeleton.API.Features.User.Domain;

namespace HexagonalSkeleton.Test.Integration.User
{
    public class UnitOfWorkFixture : IUnitOfWorkFixture
    {
        public UnitOfWork GenerateUnitOfWorkMock(int count, out IEnumerable<UserEntity> users)
        {
            var fixture = new Fixture().Customize(new InMemoryCustomization());

            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));

            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            users = fixture.CreateMany<UserEntity>(count).ToList();

            var appDbContext = fixture.Create<AppDbContext>();
            appDbContext.AddRange(users);
            appDbContext.SaveChanges();
            appDbContext.ChangeTracker.Clear();

            return new UnitOfWork(appDbContext);
        }
    }
}
