using HexagonalSkeleton.API.Config;
using Microsoft.Extensions.Options;

namespace HexagonalSkeleton.Test.Unit
{
    public interface IUnitTestFixture
    {
        IOptions<AppSettings> Settings { get; set; }

        bool ValidateToken(string tokenString);
    }
}
