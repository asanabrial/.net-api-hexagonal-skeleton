using System;
using System.Threading;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Abstractions
{
    /// <summary>
    /// Specific abstraction for MongoDB containers
    /// </summary>
    public interface IMongoDbTestContainer : ITestDatabaseContainer
    {
        string DatabaseName { get; }
        string Username { get; }
        int Port { get; }
    }
}
