using HexagonalSkeleton.API.Data;
using HexagonalSkeleton.API.Features.User.Domain;

namespace HexagonalSkeleton.Test.Integration.User
{
    public interface IUnitOfWorkFixture
    {
        UnitOfWork GenerateUnitOfWorkMock(int count, out IEnumerable<UserEntity> users);
    }
}
