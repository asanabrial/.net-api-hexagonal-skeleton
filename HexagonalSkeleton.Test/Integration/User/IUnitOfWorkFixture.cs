using HexagonalSkeleton.API.Data;
using HexagonalSkeleton.API.Features.User.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HexagonalSkeleton.Test.Integration.User.UnitOfWorkFixture;

namespace HexagonalSkeleton.Test.Integration.User
{
    public interface IUnitOfWorkFixture
    {
        UnitOfWork GenerateUnitOfWorkMock(int count, out IEnumerable<UserEntity> users);
    }
}
