namespace HexagonalSkeleton.Test.TestInfrastructure.Configuration;

public class CdcConfiguration
{
    public string TargetDatabase { get; set; } = string.Empty;
    public int WaitForSynchronizationTimeoutMs { get; set; }
    public bool ProcessOnlyTargetDatabase { get; set; }
    public int MaxSyncRetries { get; set; }
    public int SyncRetryDelayMs { get; set; }
}
