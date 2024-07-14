using HexagonalSkeleton.API.Config;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.Unit
{
    public interface IUnitTestFixture
    {
        IOptions<AppSettings> Settings { get; set; }

        bool ValidateToken(string tokenString);
    }
}
