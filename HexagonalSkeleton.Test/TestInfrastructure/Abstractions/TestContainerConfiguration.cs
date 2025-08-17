namespace HexagonalSkeleton.Test.TestInfrastructure.Abstractions
{
    /// <summary>
    /// Configuration for test containers
    /// </summary>
    public record TestContainerConfiguration
    {
        public string? Image { get; init; }
        public string? Database { get; init; }
        public string? Username { get; init; }
        public string? Password { get; init; }
        public string? KafkaBootstrapServers { get; init; }
        public string? SchemaRegistryUrl { get; init; }
        public bool CleanUp { get; init; } = true;
        public TimeSpan StartupTimeout { get; init; } = TimeSpan.FromMinutes(2);
    }
}
