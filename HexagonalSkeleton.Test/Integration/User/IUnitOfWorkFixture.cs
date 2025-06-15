using HexagonalSkeleton.API.Data;
using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Test.Integration.User
{
    public interface IUnitOfWorkFixture
    {
        UnitOfWork GenerateUnitOfWorkMock(int count, out IEnumerable<UserEntity> users);
    }
}
