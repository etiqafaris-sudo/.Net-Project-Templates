namespace CompanyName.Infrastructure.Outbox;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    public int PollingIntervalSeconds { get; set; } = 5;

    public int BatchSize { get; set; } = 100;

    public int LockDurationSeconds { get; set; } = 60;

    public int RetryDelaySeconds { get; set; } = 30;

    public int MaxRetryAttempts { get; set; } = 5;

    public int MaxErrorLength { get; set; } = 4000;
}