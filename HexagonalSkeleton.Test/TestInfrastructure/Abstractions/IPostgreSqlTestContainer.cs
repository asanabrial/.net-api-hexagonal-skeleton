using System;
using System.Threading;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Abstractions
{
    /// <summary>
    /// Specific abstraction for PostgreSQL containers
    /// </summary>
    public interface IPostgreSqlTestContainer : ITestDatabaseContainer
    {
        string DatabaseName { get; }
        string Username { get; }
        int Port { get; }
    }
}
