namespace HexagonalSkeleton.Test.TestInfrastructure.Configuration
{
    public class KafkaEnvironmentConfiguration
    {
        public string ZookeeperConnect { get; set; } = string.Empty;
        public string Listeners { get; set; } = string.Empty;
        public string AdvertisedListeners { get; set; } = string.Empty;
        public string ListenerSecurityProtocolMap { get; set; } = string.Empty;
        public string InterBrokerListenerName { get; set; } = string.Empty;
        public string OffsetsTopicReplicationFactor { get; set; } = string.Empty;
        public string LogCleanerDeleteRetentionMs { get; set; } = string.Empty;
        public string BrokerId { get; set; } = string.Empty;
        public string MinInSyncReplicas { get; set; } = string.Empty;
        public string AutoCreateTopicsEnable { get; set; } = string.Empty;
        public string GroupInitialRebalanceDelayMs { get; set; } = string.Empty;
        public string TransactionStateLogReplicationFactor { get; set; } = string.Empty;
        public string TransactionStateLogMinIsr { get; set; } = string.Empty;
        public string ZookeeperSessionTimeoutMs { get; set; } = string.Empty;
        public string ZookeeperConnectionTimeoutMs { get; set; } = string.Empty;
        public string SocketSendBufferBytes { get; set; } = string.Empty;
        public string SocketReceiveBufferBytes { get; set; } = string.Empty;
        public string RequestTimeoutMs { get; set; } = string.Empty;
        public string BootstrapServers { get; set; } = string.Empty;
    }
}
