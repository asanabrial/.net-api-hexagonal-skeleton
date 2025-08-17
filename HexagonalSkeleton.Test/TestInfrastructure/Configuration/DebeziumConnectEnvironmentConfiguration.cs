namespace HexagonalSkeleton.Test.TestInfrastructure.Configuration
{
    public class DebeziumConnectEnvironmentConfiguration
    {
        public string GroupId { get; set; } = string.Empty;
        public string ConfigStorageTopic { get; set; } = string.Empty;
        public string OffsetStorageTopic { get; set; } = string.Empty;
        public string StatusStorageTopic { get; set; } = string.Empty;
        public string LogLayoutConversionPattern { get; set; } = string.Empty;
    }
}
